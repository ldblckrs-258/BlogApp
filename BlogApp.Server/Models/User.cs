using System.Data;
using MySql.Data.MySqlClient;

namespace BLOGAPP.SERVER
{
  public class User
  {
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = new byte[0];
    public byte[] PasswordSalt { get; set; } = new byte[0];
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }

    public User(MySqlDataReader reader)
    {
      try
      {
        Id = reader.GetInt32("id");
        Username = reader.GetString("username");
        PasswordHash = (byte[])reader.GetValue("password_hash");
        PasswordSalt = (byte[])reader.GetValue("password_salt");
        Email = reader.GetString("email");
        RoleId = reader.GetInt32("role_id");
      }
      catch
      {
        Console.WriteLine("Reader isn't valid for User model");
      }
    }
  }

  public class UserUpdateModel
  {
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
  }

  public class UserLoginModel
  {
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
  }

}