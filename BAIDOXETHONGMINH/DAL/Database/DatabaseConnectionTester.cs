using System.Data;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Database;

/// <summary>
/// Lớp kiểm tra kết nối tới CSDL SQLite (smart_parking.db) có tồn tại và mở được hay không
/// </summary>
public static class DatabaseConnectionTester
{
    /// <summary>
    /// Trả về true nếu file smart_parking.db tồn tại và mở kết nối thành công
    /// </summary>
    public static bool IsConnectionAvailable()
    {
        try
        {
            if (!File.Exists(SqliteDbContext.DatabaseFilePath))
            {
                return false;
            }

            using SqliteConnection connection = SqliteDbContext.GetConnection();
            return connection.State == ConnectionState.Open;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
}
