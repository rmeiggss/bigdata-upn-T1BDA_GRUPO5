using DataAccess.MongoDb.Models;
using DataAccess.MongoDb.Readonly;
using DataAccess.MongoDb.Services;
using MongoDB.Bson;
using System;
using System.Runtime.CompilerServices;

namespace BusinessLogicManager.Managers
{
    public class OrderDetailsManager
    {
        private readonly MongoDbService _service;
        private readonly string _collectionName;

        public OrderDetailsManager(MongoDbService service, string collectionName = "OrderDetails")
        {
            _service = service;
            _collectionName = "OrderDetails";
        }

        public Task<(List<OrderDetails> Items, long Total)> GetPagedAsync(int page, int size)
        => _service.GetPagedAsync<OrderDetails>(_collectionName, page, size);

        public Task InsertAsync(OrderDetails person) => _service.InsertAsync(_collectionName, person);

        public Task<OrderDetails?> GetByIdAsync(ObjectId id) => _service.GetByIdAsync<OrderDetails  >(_collectionName, id);

        public Task<bool> UpdateAsync(ObjectId id, Dictionary<string, object?> updates)
            => _service.UpdateFieldsAsync<OrderDetails>(_collectionName, id, updates);

        public Task<bool> DeleteAsync(ObjectId id) => _service.DeleteAsync<OrderDetails>(_collectionName, id);

        public Task<long> DeleteManyAsync(IEnumerable<ObjectId> ids) => _service.DeleteManyAsync<OrderDetails>(_collectionName, ids);


        public async Task<bool> CheckIfExist(Dictionary<string, object?> filters)
        {
            var result = await _service.GetByCompositeIdAsync<OrderDetails>(_collectionName, filters);
            return result != null;
        }

        public async Task<List<CategoryProductsReadonly>> GetGroupedByCategoryAndProduct()
        {
            return await _service.GetGroupedByCategoryAndProductAsync(_collectionName);
        }

    }
}
