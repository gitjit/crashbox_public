using System;
using System.Threading.Tasks;
using CrashBox.Cosmos;
using CrashBox.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrashBox.TH
{
    class Program
    {
        static void Main(string[] args)
        {
            var _cosmosDbService = InitializeCosmos();
            ExecuteQueries(_cosmosDbService).Wait();
            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        // Initialize Cosmos Client
        private static ICosmosDbService InitializeCosmos()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build(); // Expect appsettings.json in output directory
            var dbId = config["CosmosDB"];
            var containerId = config["CosmosContainer"];
            var endpoint = config["CosmosEndpoint"];
            var masterKey = config["CosmosMasterKey"];
            var client = new CosmosClient(endpoint, masterKey);
            var cosmosDbService = new CosmosDbService(client, dbId, containerId);
            return cosmosDbService;
        }

        //Test Queries
        private static async Task ExecuteQueries(ICosmosDbService cosmosDbService)
        {
            string query = String.Empty;

            // Query 1: Get last 20 Crashes
            {
                query = "SELECT * FROM c ORDER BY c._ts OFFSET 1 LIMIT 20";
                var documents = await cosmosDbService.QueryContainerAsync(query);
                System.Console.WriteLine("Query Result : \n");
                foreach (var doc in documents)
                {
                    var crash = JsonConvert.DeserializeObject<Crash>(doc.ToString());
                    Console.WriteLine(crash.Id + " , " + crash.PKey + " , " + crash.Region);
                }
            }


            // Query 2 : GET crashes based on PK
            {
                query = "SELECT * FROM c WHERE c.pk='CBox_1.0' ORDER BY c._ts OFFSET 1 LIMIT 10";
                var documents = await cosmosDbService.QueryContainerAsync(query);
                System.Console.WriteLine("Query Result : \n");
                foreach (var doc in documents)
                {
                    var crash = JsonConvert.DeserializeObject<Crash>(doc.ToString());
                    Console.WriteLine(crash.Id + " , " + crash.PKey + " , " + crash.Region);
                }
            }


            // Query 3: Get crash count in a version
            {
                var search = "CBox_1.0";
                query = $"SELECT VALUE COUNT(1) FROM c WHERE CONTAINS(c.pk,'{search}')";
                var result = await cosmosDbService.QueryContainerAsync(query);
                System.Console.WriteLine("Query Result : \n");
                System.Console.WriteLine($"Total crashes in {search} = {result.FirstOrDefault()}");
              
            }

            // Query 4: Get crash count in a version (This will reduce RU)
            {
                var search = "CBox_1.0";
                query = $"SELECT VALUE COUNT(1) FROM c WHERE c.pk = '{search}'";
                var result = await cosmosDbService.QueryContainerAsync(query);
                System.Console.WriteLine("Query Result : \n");
                System.Console.WriteLine($"Total crashes in {search} = {result.FirstOrDefault()}");
            }

            // Query 5: Get Top 10 crashes  in a version (This will cause cross partition scan)
            {
                var pk = "CBox_1.0";
                query = $"SELECT c.method, c.mhash ,  COUNT(1) as count FROM c WHERE c.pk = '{pk}' GROUP BY c.method, c.mhash";
                List<TopCrash> topCrashes = new List<TopCrash>();
                var result = await cosmosDbService.QueryContainerAsync(query);
                if(result.Count() > 0){
                    System.Console.WriteLine("Query Result : \n");
                    foreach (var item in result)
                    {
                        var tp = new TopCrash{
                            Method = item.method,
                            MHash = item.mhash,
                            Count = item.count
                        };
                        topCrashes.Add(tp);
                        Console.WriteLine($"{tp.Method} - {tp.Count}");
                    }
                }
            }

        }

    }
}
