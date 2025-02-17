using MySql.Data.MySqlClient;

namespace TikTokToFacebook.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool RecordExists(string id, string user)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM TIKTOKTOFACEBOOK WHERE Id = @Id AND User = @User", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@User", user);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void InsertRecord(string id, long createTime, string user)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var cmd = new MySqlCommand("INSERT INTO TIKTOKTOFACEBOOK (Id, CreateTime, User) VALUES (@Id, @CreateTime, @User)", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@CreateTime", createTime);
            cmd.Parameters.AddWithValue("@User", user);
            cmd.ExecuteNonQuery();
        }
    }

}
