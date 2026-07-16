namespace Aqua.Framework.Components.Table;

public interface IComparableRow<TModel>
{
    Task<TModel> AsDataAsync();
}