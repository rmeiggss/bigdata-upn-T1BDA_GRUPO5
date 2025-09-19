using Dapper;
using Dapper.Contrib.Extensions;
using DataAccess.SqlServer.Models;
using DataAccess.SqlServer.Readonly;
using System.Data;

namespace DataAccess.SqlServer.Repositories;

public interface IOrderDetailRepository
{
    Task<IEnumerable<OrderDetail>> GetAllAsync();
    Task<OrderDetail?> GetAsync(int orderId, int productId);
    Task<long> InsertAsync(OrderDetail entity);
    Task<bool> UpdateAsync(OrderDetail entity);
    Task<bool> DeleteAsync(int orderId, int productId);
}

public class OrderDetailRepository : IOrderDetailRepository
{
    private readonly Func<IDbConnection> _connectionFactory;

    public OrderDetailRepository(Func<IDbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OrderDetail>> GetAllAsync()
    {
        using var conn = _connectionFactory();
        const string sql = "SELECT [OrderID],[ProductID],[UnitPrice],[Quantity],[Discount] FROM [dbo].[Order Details]";
        return await conn.QueryAsync<OrderDetail>(sql);
    }

    public async Task<OrderDetail?> GetAsync(int orderId, int productId)
    {
        const string sql = "SELECT [OrderID],[ProductID],[UnitPrice],[Quantity],[Discount] FROM [dbo].[Order Details] WHERE [OrderID]=@orderId AND [ProductID]=@productId";
        using var conn = _connectionFactory();
        return await conn.QueryFirstOrDefaultAsync<OrderDetail>(sql, new { orderId, productId });
    }

    public async Task<long> InsertAsync(OrderDetail entity)
    {
        using var conn = _connectionFactory();
        return await conn.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(OrderDetail entity)
    {
        const string sql = @"UPDATE [dbo].[Order Details]
SET [UnitPrice]=@UnitPrice, [Quantity]=@Quantity, [Discount]=@Discount
WHERE [OrderID]=@OrderID AND [ProductID]=@ProductID";
        using var conn = _connectionFactory();
        var rows = await conn.ExecuteAsync(sql, entity);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int orderId, int productId)
    {
        const string sql = "DELETE FROM [dbo].[Order Details] WHERE [OrderID]=@orderId AND [ProductID]=@productId";
        using var conn = _connectionFactory();
        var rows = await conn.ExecuteAsync(sql, new { orderId, productId });
        return rows > 0;
    }

    public async Task<IEnumerable<OrderDetailsReadonly>> GetOrderProductAndCategories()
    {
        var query = "select od.*, c.CategoryID, c.CategoryName, p.ProductName from dbo.[Order Details] od\r\njoin dbo.Products p on p.ProductID = od.ProductID\r\njoin dbo.Categories c on c.CategoryID = p.CategoryID";
        using var conn = _connectionFactory();
        return await conn.QueryAsync<OrderDetailsReadonly>(query);
    }

    //Crear sobrecargar metodo ToString que devuelva la cadena de conexion
    public virtual string ToString() => (_connectionFactory().ConnectionString) ?? string.Empty;
}
