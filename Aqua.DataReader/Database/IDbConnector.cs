namespace Aqua.DataReader.Database;

public interface IDbConnector
{
    List<TDataModel> Select<TDataModel>(string query, object? parameters = null);
    Task<List<TDataModel>> SelectAsync<TDataModel>(string query, object? parameters = null);
    List<TDataModel> SelectMapped<TDataModel>(string query, Func<IDictionary<string, object>, TDataModel> mapper, object? parameters = null);
}