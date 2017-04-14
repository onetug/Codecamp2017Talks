using System;
using System.Data.SqlClient;

namespace CodeCamp2017.Cli
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");

      // Get password from environment variable:
      var password = Environment.GetEnvironmentVariable("SA_PASSWORD");
      var server = Environment.GetEnvironmentVariable("SQL_SERVER");

      using (SqlConnection conn = new SqlConnection($"Server={server};Database=master;User Id=sa;Password={password}"))
      {
        using (SqlCommand cmd = conn.CreateCommand())
        {
          Console.WriteLine($"Connecting to {server} with password {password}");
          // Wait for SQL to be available:
          while (true)
          {
            try
            {
              conn.Open();
              break;
            }
            catch (Exception exp)
            {
              Console.WriteLine(exp.Message);
              Console.WriteLine("Waiting for SQL...");
              System.Threading.Thread.Sleep(10000);
            }
          }

          // Create a table if it doesn't exist:
          cmd.CommandText = "USE code_camp_demo IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'code_camp_data' AND type = 'U') CREATE TABLE code_camp_data (data_key nvarchar(10) NOT NULL PRIMARY KEY, data_value nvarchar(MAX))";
          cmd.ExecuteNonQuery();

          // Insert or update a value:
          cmd.CommandText = "IF NOT EXISTS (SELECT 1 FROM code_camp_data WHERE data_key = 'foo') INSERT INTO code_camp_data VALUES('foo', 'bar') ELSE UPDATE code_camp_data SET data_value = 'bar' WHERE data_key = 'foo'";
          cmd.ExecuteNonQuery();

          // Insert data or update if already exists:
          cmd.CommandText = "SELECT data_value FROM code_camp_data WHERE data_key = 'foo'";
          var val = cmd.ExecuteScalar();
          Console.WriteLine($"The value of foo is {val}.");
        }
      }
    }
  }
}
