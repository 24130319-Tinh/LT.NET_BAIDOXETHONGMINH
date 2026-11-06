using BAIDOXETHONGMINH.DAL.Database;
using BAIDOXETHONGMINH.DTO.Entities;
using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.GUI;

/// <summary>Truy vấn bảng Customers bằng ADO.NET (SQLite).</summary>
public class CustomerRepository
{
    private const string SelectColumns = "SELECT CustomerId, FullName, Phone, IdentityCard, CreatedAt FROM Customers";

    private static Customer Map(SqliteDataReader r) => new()
    {
        CustomerId = r.GetInt32(0),
        FullName = r.GetString(1),
        Phone = r.GetString(2),
        IdentityCard = r.GetString(3),
        CreatedAt = r.GetDateTime(4)
    };

    public Task<List<Customer>> GetAllAsync() =>
        SqliteHelper.QueryAsync(SelectColumns + " ORDER BY FullName", Map);

    /// <summary>Tìm theo họ tên, số điện thoại hoặc CCCD (chứa từ khóa).</summary>
    public Task<List<Customer>> SearchAsync(string keyword) =>
        SqliteHelper.QueryAsync(
            SelectColumns + " WHERE FullName LIKE @kw OR Phone LIKE @kw OR IdentityCard LIKE @kw ORDER BY FullName",
            Map, ("@kw", "%" + keyword + "%"));

    public Task<Customer?> GetByIdAsync(int customerId) =>
        SqliteHelper.QueryFirstAsync(SelectColumns + " WHERE CustomerId = @id", Map, ("@id", customerId));

    public Task<Customer?> GetByIdentityCardAsync(string identityCard) =>
        SqliteHelper.QueryFirstAsync(SelectColumns + " WHERE IdentityCard = @cccd", Map, ("@cccd", identityCard));

    /// <summary>Thêm khách hàng, điền CustomerId mới vào đối tượng.</summary>
    public async Task AddAsync(Customer customer)
    {
        long id = await SqliteHelper.InsertAsync(
            "INSERT INTO Customers (FullName, Phone, IdentityCard, CreatedAt) VALUES (@name, @phone, @cccd, @created)",
            ("@name", customer.FullName), ("@phone", customer.Phone),
            ("@cccd", customer.IdentityCard), ("@created", customer.CreatedAt));
        customer.CustomerId = (int)id;
    }

    public Task UpdateAsync(Customer customer) =>
        SqliteHelper.ExecuteAsync(
            "UPDATE Customers SET FullName = @name, Phone = @phone, IdentityCard = @cccd WHERE CustomerId = @id",
            ("@name", customer.FullName), ("@phone", customer.Phone),
            ("@cccd", customer.IdentityCard), ("@id", customer.CustomerId));

    public async Task DeleteAsync(int customerId)
    {
        try
        {
            await SqliteHelper.ExecuteAsync("DELETE FROM Customers WHERE CustomerId = @id", ("@id", customerId));
        }
        catch (SqliteException ex) when (SqliteHelper.IsConstraintError(ex))
        {
            throw new ServiceException("Khách hàng đang được dùng ở dữ liệu khác, không xóa được.");
        }
    }
}
