using Aqua.AppConfig.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Aqua.DataReader.Database;

public class DbConnector(IConfig config) : IDbConnector
{
    private string ConnectionString => config.Env.DbConnectionString 
                                       ?? throw new InvalidOperationException("DB Connection string is not configured.");

    public List<TDataModel> Select<TDataModel>(string query, object? parameters = null)
    {
        using var connection = new SqlConnection(ConnectionString);
        return connection.Query<TDataModel>(query, parameters).ToList();
    }
    public async Task<List<TDataModel>> SelectAsync<TDataModel>(string query, object? parameters = null)
    {
        await using var connection = new SqlConnection(ConnectionString);
        return (await connection.QueryAsync<TDataModel>(query, parameters)).AsList();
    }
    
    /// <summary>
    /// Executes a query and uses a custom mapper function to build the model from a Dictionary.
    /// Dapper does the heavy lifting of converting the SQL row to a dictionary automatically.
    /// </summary>
    public List<TDataModel> SelectMapped<TDataModel>(
        string query, 
        Func<IDictionary<string, object>, TDataModel> mapper, 
        object? parameters = null)
    {
        using var connection = new SqlConnection(ConnectionString);
        var rows = connection.Query(query, parameters).Cast<IDictionary<string, object>>();
        return rows.Select(mapper).ToList();
    }
    
    public async Task<List<TDataModel>> SelectMappedAsync<TDataModel>(
        string query, 
        Func<IDictionary<string, object>, TDataModel> mapper, 
        object? parameters = null)
    {
        await using var connection = new SqlConnection(ConnectionString);
        var rows = (await connection.QueryAsync(query, parameters)).Cast<IDictionary<string, object>>();
        return rows.Select(mapper).ToList();
    }
}