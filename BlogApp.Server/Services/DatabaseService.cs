using MySql.Data.MySqlClient;

namespace BLOGAPP.SERVER
{
  public class DatabaseService(string connStr)
  {
    private readonly string _connStr = connStr;

    public MySqlConnection GetConnection()
    {
      var conn = new MySqlConnection(_connStr);
      conn.Open();
      return conn;
    }

  }
}