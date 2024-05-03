using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BLOGAPP.SERVER
{
  [ApiController]
  [Route("auth")]

  public class AuthController(DatabaseService dbService, IConfiguration configuration) : ControllerBase
  {
    private readonly DatabaseService _dbService = dbService;
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("register")]
    public ActionResult<User> Register([FromBody] UserUpdateModel user)
    {
      CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);
      using var connection = _dbService.GetConnection();
      using var command = connection.CreateCommand();
      command.CommandText = "INSERT INTO Users (username, password_hash, password_salt, email, role_id) VALUES (@username, @password_hash, @password_salt, @email, @role_id)";
      command.Parameters.AddWithValue("@username", user.Username);
      command.Parameters.AddWithValue("@password_hash", passwordHash);
      command.Parameters.AddWithValue("@password_salt", passwordSalt);
      command.Parameters.AddWithValue("@email", user.Email);
      command.Parameters.AddWithValue("@role_id", user.RoleId);
      command.ExecuteNonQuery();
      return Ok();
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      using (var hmac = new HMACSHA512())
      {
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      }
    }

    [HttpPost("login")]
    public ActionResult<User> Login([FromBody] UserLoginModel user)
    {
      using var connection = _dbService.GetConnection();
      using var command = connection.CreateCommand();
      command.CommandText = "SELECT * FROM Users WHERE username = @username";
      command.Parameters.AddWithValue("@username", user.Username);
      using var reader = command.ExecuteReader();
      if (reader.Read())
      {
        var dbUser = new User(reader);
        if (dbUser == null || dbUser.Username != user.Username || !VerifyPasswordHash(user.Password, dbUser.PasswordHash, dbUser.PasswordSalt))
        {
          return Unauthorized();
        }
        var jwt = GenerateJwtToken(dbUser);
        return Ok(new { jwt });
      }
      return NotFound();
    }


    private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
      using (var hmac = new HMACSHA512(passwordSalt))
      {
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
      }
    }

    private string GenerateJwtToken(User user)
    {
      List<Claim> claims =
      [
        new("id", user.Id.ToString()),
        new("username", user.Username),
        new("role_id", user.RoleId.ToString())
      ];

      var secret = _configuration["Secret"];
      var key = new SymmetricSecurityKey(secret != null ? System.Text.Encoding.UTF8.GetBytes(secret) : throw new ArgumentNullException(nameof(secret)));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: creds
      );

      var jwt = new JwtSecurityTokenHandler().WriteToken(token);

      return jwt;
    }

    [HttpGet("verify")]
    public ActionResult VerifyToken(string token)
    {
      // var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
      var secret = _configuration["Secret"];
      var key = new SymmetricSecurityKey(secret != null ? System.Text.Encoding.UTF8.GetBytes(secret) : throw new ArgumentNullException(nameof(secret)));

      var tokenHandler = new JwtSecurityTokenHandler();
      try
      {
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = key,
          ValidateIssuer = false,
          ValidateAudience = false,
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var claims = jwtToken.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);

        return Ok(claims);
      }
      catch (Exception)
      {
        return Unauthorized();
      }
    }
  }
}