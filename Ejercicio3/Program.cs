using DataAccess.SqlServer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ejercicio3
{
    internal class Program
    {
        private readonly string strConnection;
        private readonly IMongoCollection<CustomerOrders> mongoCollection;

        public Program()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            strConnection = config.GetConnectionString("SqlServer") ?? throw new ArgumentException(nameof(strConnection));
            string mongoCn = config.GetConnectionString("Mongo") ?? throw new ArgumentException(nameof(mongoCn));
            string mongoDb = config["Mongo:Database"] ?? throw new ArgumentException(nameof(mongoDb));

            var mongoClient = new MongoClient(mongoCn);
            var database = mongoClient.GetDatabase(mongoDb);
            mongoCollection = database.GetCollection<CustomerOrders>("CustomerOrders");
        }

        public static async Task Main(string[] args)
        {
            Program p = new();

            bool continueLoop = true;

            do
            {
                Console.WriteLine("{0} ==== Iniciando proceso de migración", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                List<CustomerOrders> cos = p.ListaCustomersOrders();

                Console.WriteLine("Se encontraron {0} registro(s) en la Db origen.", cos.Count);
                Console.WriteLine("Iniciando verificación para su inserción.");

                long totalInserts = await p.InsertarDatosEnMongo(cos);

                Console.WriteLine("Se insertaron {0} registro(s) en la Db destino.", totalInserts);
                Console.WriteLine("Presiona cualquier tecla para continuar o F para finalizar.");

                var key = Console.ReadKey(true);

                if (key.KeyChar == 'F' || key.KeyChar == 'f')
                {
                    continueLoop = false;
                    Console.WriteLine("Finalizando proceso");
                }

                Console.WriteLine();
                Console.WriteLine();
            } while (continueLoop);
        }

        private async Task<long> InsertarDatosEnMongo(List<CustomerOrders> cos)
        {
            long totalInserts = 0;

            if(cos.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No hay registros en la lista de datos.");
                Console.ResetColor();
            }
            else
            {
                try
                {
                    List<CustomerOrders> eCos = await mongoCollection.Find(new BsonDocument()).ToListAsync();

                    cos = [.. cos.Where(x => !eCos.Exists(y => y.CustomerId == x.CustomerId))];

                    if (cos.Count != 0)
                    {
                        List<InsertOneModel<CustomerOrders>> iomCos = [.. cos.Select(x => new InsertOneModel<CustomerOrders>(x))];

                        BulkWriteResult<CustomerOrders> bwrCos = await mongoCollection.BulkWriteAsync(iomCos, new BulkWriteOptions() { BypassDocumentValidation = true, IsOrdered = false });

                        totalInserts = bwrCos.InsertedCount;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error durante la ejecución: " + e.Message);
                }
            }

            return totalInserts;
        }

        private List<CustomerOrders> ListaCustomersOrders()
        {
            const string query = @"
            SELECT [c].[CustomerID]
	            ,[c].[CompanyName]
	            ,[c].[ContactName]
	            ,COUNT([o].[OrderID]) AS [TotalOrders]
	            ,FORMAT(MAX([o].[OrderDate]), 'yyyy-MM-dd') AS [LastOrderDate]
	            ,[c].[Country] 
            FROM [Northwind].[dbo].[Customers] [c]
	            INNER JOIN [Northwind].[dbo].[Orders] [o] ON [o].[CustomerID] = [c].[CustomerID]
            GROUP BY [c].[CustomerID], [c].[CompanyName], [c].[ContactName], [c].[Country] 
            HAVING COUNT([o].[OrderID]) >= 5";

            List<CustomerOrders> customerList = new();

            try
            {
                using var cnx = new SqlConnection(strConnection);
                cnx.Open();
                using var cmd = new SqlCommand(query, cnx);
                using var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    CustomerOrders co = new()
                    {
                        CustomerId = rd["CustomerId"].ToString(),
                        CompanyName = rd["CompanyName"].ToString(),
                        ContactName = rd["ContactName"].ToString(),
                        Country = rd["Country"].ToString(),
                        TotalOrders = int.Parse(rd["TotalOrders"].ToString()),
                        LastOrderDate = rd["LastOrderDate"].ToString(),
                    };

                    customerList.Add(co);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error procesando la operación: " + e.Message);
            }

            return customerList;
        }
    }
}
