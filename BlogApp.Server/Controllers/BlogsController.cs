using Microsoft.AspNetCore.Mvc;

namespace BLOGAPP.SERVER
{
  [ApiController]
  [Route("blogs")]

  public class BlogsController : ControllerBase
  {
    private readonly DatabaseService _dbService;

    public BlogsController(DatabaseService dbService)
    {
      _dbService = dbService;
    }

    [HttpGet("get/all")]
    public IActionResult GetAllBlogs()
    {
      using var connection = _dbService.GetConnection();
      using var command = connection.CreateCommand();
      command.CommandText = "SELECT * FROM Blogs";
      using var reader = command.ExecuteReader();
      var blogs = new List<Blog>();
      while (reader.Read())
      {
        blogs.Add(new Blog(reader));
      }
      return Ok(blogs);
    }

    [HttpGet("get/{id}")]
    public IActionResult GetBlogById(int id)
    {
      using var connection = _dbService.GetConnection();
      using var command = connection.CreateCommand();
      command.CommandText = "SELECT * FROM Blogs WHERE id = @id";
      command.Parameters.AddWithValue("@id", id);
      using var reader = command.ExecuteReader();
      if (reader.Read())
      {
        return Ok(new Blog(reader));
      }
      return NotFound();
    }

    [HttpPost("create")]
    public IActionResult CreateBlog([FromBody] Blog blog)
    {
      using var connection = _dbService.GetConnection();
      using var command = connection.CreateCommand();
      command.CommandText = "INSERT INTO Blogs (title, content, creator_id) VALUES (@title, @content, @creator_id)";
      command.Parameters.AddWithValue("@title", blog.Title);
      command.Parameters.AddWithValue("@content", blog.Content);
      command.Parameters.AddWithValue("@creator_id", blog.CreatorId);
      command.ExecuteNonQuery();
      return Ok();
    }

    [HttpPut("update/{id}")]
    public IActionResult UpdateBlog(int id, [FromBody] BlogUpdateModel blog)
    {
      using var connection = _dbService.GetConnection();
      using var command = connection.CreateCommand();
      command.CommandText = "UPDATE Blogs SET title = @title, content = @content, creator_id = @creator_id WHERE id = @id";
      command.Parameters.AddWithValue("@title", blog.Title);
      command.Parameters.AddWithValue("@content", blog.Content);
      command.Parameters.AddWithValue("@creator_id", blog.CreatorId);
      command.Parameters.AddWithValue("@id", id);
      command.ExecuteNonQuery();
      return Ok();
    }

    [HttpDelete("delete/{id}")]
    public IActionResult DeleteBlog(int id)
    {
      using var connection = _dbService.GetConnection();
      using var command = connection.CreateCommand();
      command.CommandText = "DELETE FROM Blogs WHERE id = @id";
      command.Parameters.AddWithValue("@id", id);
      command.ExecuteNonQuery();
      return Ok();
    }

    public class BlogUpdateModel
    {
      public required string Title { get; set; }
      public required string Content { get; set; }
      public int CreatorId { get; set; }
    }
  }

}