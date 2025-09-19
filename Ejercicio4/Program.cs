using BusinessLogicManager.Managers;
using DataAccess.MongoDb.Models;
using DataAccess.MongoDb.Services;
using Microsoft.Extensions.Configuration;
using System.IO.Pipes;

namespace Ejercicio4;

internal class Program
{
    private static OrderDetailsManager? _mongoManager;
    private static MongoDbService _mongoDbService;
    private static SqlOrderDetailsManager? _sqlManager;

    static async Task Main(string[] args)
    {
        try
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var sqlConnStr = config.GetConnectionString("SqlServer") ?? throw new InvalidOperationException("SqlServer connection string missing");
            var mongoConnStr = config.GetConnectionString("Mongo") ?? throw new InvalidOperationException("Mongo connection string missing");
            var mongoDbName = config.GetSection("Mongo").GetValue<string>("Database") ?? "Northwind";

            _mongoDbService = new MongoDbService(mongoDbName, mongoConnStr);
            _mongoManager = new OrderDetailsManager(_mongoDbService);
            _sqlManager = new SqlOrderDetailsManager(sqlConnStr);


            while (true)
            {
                Console.WriteLine("Seleccione una opcion:");
                Console.WriteLine("1. Migrar Order Details de SQL Server a MongoDB");
                Console.WriteLine("2. Listar Order Details agrupados por CategoryName, ProductName y Quantity");
                Console.WriteLine("0. Salir");

                var option = Console.ReadLine();
                if (option == "1")
                {
                    await MigrateOrderDetails();
                }
                else if (option == "2")
                {
                    await ListOrderDetailsGrouped();
                }
                else if (option == "0")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Opcion no valida");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ReadKey();
        }

    }

    private static async Task ListOrderDetailsGrouped()
    {
        Console.WriteLine("Leyendo Order Details desde MongoDB...");

        var items = await _mongoManager.GetGroupedByCategoryAndProduct();

        Console.WriteLine("Agrupados por ProductID y Quantity:");
        foreach (var group in items)
        {
            Console.WriteLine($"Category: {group.CategoryName}");
            foreach (var product in group.Products)
            {
                Console.WriteLine($"\tProduct: {product.ProductName}, Total Quantity: {product.TotalQuantity}");
            }
        }
    }

    private static async Task MigrateOrderDetails()
    {
        Console.WriteLine("Leyendo Order Details desde SQL Server...");
        var sqlItems = await _sqlManager.GetOrderProductAndCategories();
        Console.WriteLine($"Registros obtenidos: {sqlItems.Count()}");
        Console.WriteLine("Insertando en MongoDB...");
        var inserted = 0;
        foreach (var item in sqlItems)
        {
            var doc = new OrderDetails
            {
                OrderID = item.OrderID,
                ProductID = item.ProductID,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Discount = item.Discount,
                CategoryID = item.CategoryID,
                CategoryName = item.CategoryName,
                ProductName = item.ProductName
            };
            await _mongoManager.InsertAsync(doc);
            inserted++;
        }
        Console.WriteLine($"Insertados en MongoDB: {inserted}");
    }
}
