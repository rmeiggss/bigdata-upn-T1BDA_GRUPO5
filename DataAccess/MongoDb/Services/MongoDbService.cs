using DataAccess.MongoDb.Readonly;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.MongoDb.Models;

namespace DataAccess.MongoDb.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbService(string databaseName, string connectionString)
        {
            var client = new MongoClient(connectionString);
            _mongoDatabase = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName) => _mongoDatabase.GetCollection<T>(collectionName);

        public async Task InsertAsync<T>(string collectionName, T document) => await GetCollection<T>(collectionName).InsertOneAsync(document);

        public async Task<List<T>> GetAllAsync<T>(string collectionName) => await GetCollection<T>(collectionName)
            .Find(Builders<T>.Filter.Empty)
            .ToListAsync();

        public async Task<(List<T> Items, long Total)> GetPagedAsync<T>(string collectionName, int page, int pageSize)
        {
            if (page < 1) page = 1;
            var collection = GetCollection<T>(collectionName);
            var total = await collection.CountDocumentsAsync(Builders<T>.Filter.Empty);
            var items = await collection.Find(Builders<T>.Filter.Empty)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
            return (items, total);
        }

        public async Task<T?> GetByIdAsync<T>(string collectionName, ObjectId id)
        {
            var result = await GetCollection<T>(collectionName)
                .Find(Builders<T>.Filter.Eq("_id", id))
                .FirstOrDefaultAsync();
            return result;
        }

        public async Task<bool> UpdateFieldsAsync<T>(string collectionName, ObjectId id, Dictionary<string, object?> updates)
        {
            if (!updates.Any()) return false;
            var updateBuilder = Builders<T>.Update;
            UpdateDefinition<T>? updateDef = null;
            foreach (var kv in updates)
            {
                if (kv.Value == null) continue;
                updateDef = updateDef == null ? updateBuilder.Set(kv.Key, kv.Value) : updateDef.Set(kv.Key, kv.Value);
            }
            if (updateDef == null) return false;
            var result = await GetCollection<T>(collectionName).UpdateOneAsync(Builders<T>.Filter.Eq("_id", id), updateDef);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync<T>(string collectionName, ObjectId id)
        {
            var result = await GetCollection<T>(collectionName).DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
            return result.DeletedCount > 0;
        }

        public async Task<long> DeleteManyAsync<T>(string collectionName, IEnumerable<ObjectId> ids)
        {
            var list = ids.ToList();
            if (!list.Any()) return 0;
            var result = await GetCollection<T>(collectionName).DeleteManyAsync(Builders<T>.Filter.In("_id", list));
            return result.DeletedCount;
        }

        public async Task<List<T?>> GetByCompositeIdAsync<T>(string collectionName, Dictionary<string, object> keyValues)
        {
            if (!keyValues.Any()) return new List<T?>();
            var filterBuilder = Builders<T>.Filter;
            FilterDefinition<T>? filterDef = null;
            foreach (var kv in keyValues)
            {
                filterDef = filterDef == null ? filterBuilder.Eq(kv.Key, kv.Value) : filterDef & filterBuilder.Eq(kv.Key, kv.Value);
            }
            if (filterDef == null) return new List<T?>();
            var results = await GetCollection<T>(collectionName).Find(filterDef).ToListAsync();
            return results;
        }

        public async Task<List<CategoryProductsReadonly>> GetGroupedByCategoryAndProductAsync(string collectionName)
        {
            // Trabajamos sobre la colección de OrderDetails (documentos planos con CategoryName, ProductName y Quantity)
            var collection = GetCollection<OrderDetails>(collectionName);

            var pipeline = new BsonDocument[]
            {
                // Agrupar por CategoryName y ProductName sumando Quantity
                new BsonDocument("$group", new BsonDocument
                {
                    {"_id", new BsonDocument
                        {
                            {"CategoryName", "$CategoryName"},
                            {"ProductName", "$ProductName"}
                        }
                    },
                    {"TotalQuantity", new BsonDocument("$sum", "$Quantity")}
                }),
                // Ordenar por CategoryName asc y luego por TotalQuantity desc para obtener Top 3 por categoría
                new BsonDocument("$sort", new BsonDocument
                {
                    {"_id.CategoryName", 1},
                    {"TotalQuantity", -1}
                }),
                // Agrupar por categoría acumulando productos en el orden ya ordenado (TotalQuantity desc)
                new BsonDocument("$group", new BsonDocument
                {
                    {"_id", "$_id.CategoryName"},
                    {"Products", new BsonDocument("$push", new BsonDocument
                        {
                            {"ProductName", "$_id.ProductName"},
                            {"TotalQuantity", "$TotalQuantity"}
                        })
                    }
                }),
                // Proyectar solo los 3 primeros productos por categoría
                new BsonDocument("$project", new BsonDocument
                {
                    {"_id", 0},
                    {"CategoryName", "$_id"},
                    {"Products", new BsonDocument("$slice", new BsonArray { "$Products", 3 })}
                }),
                // Ordenar categorías alfabéticamente
                new BsonDocument("$sort", new BsonDocument
                {
                    {"CategoryName", 1}
                })
            };

            var results = await collection.Aggregate<CategoryProductsReadonly>(pipeline).ToListAsync();
            return results;
        }
    }
}
