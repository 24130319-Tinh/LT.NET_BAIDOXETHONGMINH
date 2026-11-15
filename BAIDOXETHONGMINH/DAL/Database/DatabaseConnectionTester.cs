using System.Data;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Database;

public static class DatabaseConnectionTester
{
    // Hàm kiểm tra file cơ sở dữ liệu smart_parking.db có tồn tại và mở kết nối được hay không
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
