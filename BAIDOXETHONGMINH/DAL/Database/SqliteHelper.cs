using Microsoft.Data.Sqlite;

namespace BAIDOXETHONGMINH.DAL.Database;

public static class SqliteHelper
{
    public static string DatabasePath { get; set; } = SqliteDbContext.DatabaseFilePath;

    private static async Task<SqliteConnection> OpenAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(DatabasePath))!);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath, ForeignKeys = true, DefaultTimeout = 30
        }.ToString());
        await connection.OpenAsync();
        return connection;
    }

    private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction? tx, string sql,
        (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = tx;
        foreach (var p in parameters) command.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
        return command;
    }

    public static async Task<List<T>> QueryAsync<T>(string sql, Func<SqliteDataReader, T> map,
        params (string Name, object? Value)[] parameters)
    {
        await using var connection = await OpenAsync();
        using var command = Command(connection, null, sql, parameters);
        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<T>();
        while (await reader.ReadAsync()) result.Add(map(reader));
        return result;
    }

    public static async Task<T?> QueryFirstAsync<T>(string sql, Func<SqliteDataReader, T> map,
        params (string Name, object? Value)[] parameters) where T : class =>
        (await QueryAsync(sql, map, parameters)).FirstOrDefault();

    public static async Task<int> ExecuteAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var connection = await OpenAsync();
        return await ExecuteAsync(connection, null, sql, parameters);
    }

    public static async Task<int> ExecuteAsync(SqliteConnection connection, SqliteTransaction? tx, string sql,
        params (string Name, object? Value)[] parameters)
    {
        using var command = Command(connection, tx, sql, parameters);
        return await command.ExecuteNonQueryAsync();
    }

    public static async Task<long> ScalarAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var connection = await OpenAsync();
        using var command = Command(connection, null, sql, parameters);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public static async Task<long> InsertAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var connection = await OpenAsync();
        return await InsertAsync(connection, null, sql, parameters);
    }

    public static async Task<long> InsertAsync(SqliteConnection connection, SqliteTransaction? tx, string sql,
        params (string Name, object? Value)[] parameters)
    {
        using var command = Command(connection, tx, sql + "; SELECT last_insert_rowid();", parameters);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public static async Task InTransactionAsync(Func<SqliteConnection, SqliteTransaction, Task> work)
    {
        await using var connection = await OpenAsync();
        using var tx = connection.BeginTransaction();
        await work(connection, tx);
        tx.Commit();
    }

    public static string? GetNullableString(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetString(index);
    public static bool IsConstraintError(SqliteException ex) => ex.SqliteErrorCode == 19;
}
