using MySql.Data.MySqlClient;

namespace BLOGAPP.SERVER
{
  public class Blog(int id, string title, string content, int creatorId)
  {
    public int Id { get; set; } = id;
    public string Title { get; set; } = title;
    public string Content { get; set; } = content;
    public int CreatorId { get; set; } = creatorId;

    public Blog(MySqlDataReader reader) : this(0, "", "", 0)
    {
      try
      {
        Id = reader.GetInt32("id");
        Title = reader.GetString("title");
        Content = reader.GetString("content");
        CreatorId = reader.GetInt32("creator_id");
      }
      catch
      {
        Console.WriteLine("Reader isn't valid for Blog model");
      }

    }
  }
}