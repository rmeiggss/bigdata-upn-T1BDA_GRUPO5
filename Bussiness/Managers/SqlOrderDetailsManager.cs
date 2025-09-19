using DataAccess.SqlServer.Repositories;
using DataAccess.SqlServer;
using DataAccess.SqlServer.Models;
using Dapper;
using DataAccess.SqlServer.Readonly;

namespace BusinessLogicManager.Managers;

public class SqlOrderDetailsManager
{
    private readonly OrderDetailRepository _repo;

    public SqlOrderDetailsManager(string sqlConnectionString)
    {
        var factory = new SqlConnectionFactory(sqlConnectionString);
        _repo = new OrderDetailRepository(factory.Create);
    }

    public Task<IEnumerable<OrderDetail>> GetAllAsync() => _repo.GetAllAsync();
    public Task<OrderDetail?> GetAsync(int orderId, int productId) => _repo.GetAsync(orderId, productId);
    public Task<long> InsertAsync(OrderDetail entity) => _repo.InsertAsync(entity);
    public Task<bool> UpdateAsync(OrderDetail entity) => _repo.UpdateAsync(entity);
    public Task<bool> DeleteAsync(int orderId, int productId) => _repo.DeleteAsync(orderId, productId);
    public Task<IEnumerable<OrderDetailsReadonly>> GetOrderProductAndCategories() => _repo.GetOrderProductAndCategories();


}
