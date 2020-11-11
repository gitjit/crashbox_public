using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrashBox.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace CrashBox.Cosmos
{
    public class CosmosDbService : ICosmosDbService
    {
        private CosmosClient _cosmosClient;
        private string _databaseId;
        private string _containerId;
        private Container _container;

        public CosmosDbService(CosmosClient cosmosClient, string databaseId, string containerId)
        {
            _cosmosClient = cosmosClient;
            _databaseId = databaseId;
            _containerId = containerId;
            _container = _cosmosClient.GetContainer(_databaseId, _containerId);
        }


        public string DataBaseId
        {
            get
            {
                return _databaseId;
            }
        }

        public string ContainerId
        {
            get
            {
                return _containerId;
            }
        }

        // Creates a new database
        public async Task<bool> CreateDatabaseAsync(string db)
        {
            try
            {
                var result = await _cosmosClient.CreateDatabaseIfNotExistsAsync(db);
                if (result.Resource.Id != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        // Creates a new container
        public async Task<bool> CreateContainerAsync(string db, string containerId, string pk, int throughput)
        {
            try
            {
                var containerDef = new ContainerProperties
                {
                    Id = containerId,
                    PartitionKeyPath = "/" + pk
                };
                var database = _cosmosClient.GetDatabase(db);
                var result = await database.CreateContainerIfNotExistsAsync(containerDef, throughput);
                if (result.Resource.Id != null)
                    return true;
                return false;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        // Get databases
        public async Task<IEnumerable<string>> GetDatabasesAsync()
        {
            List<string> result = new List<string>();

            var iterator = _cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
            var databases = await iterator.ReadNextAsync();

            foreach (var db in databases)
            {
                Console.WriteLine($"Database Id: {db.Id}; Modified: {db.LastModified}");
                result.Add($"Database Id: {db.Id}; Modified: {db.LastModified}");
            }
            return result;
        }

        // Get all containers in a Db
        public async Task<IEnumerable<string>> GetContainersAsync(string db)
        {
            var database = _cosmosClient.GetDatabase(db);
            var iterator = database.GetContainerQueryIterator<ContainerProperties>();
            var containers = await iterator.ReadNextAsync();
            var result = new List<string>();

            foreach (var container in containers)
            {
                var output = $"{container.Id} , pk = {container.PartitionKeyPath}";
                result.Add(output);
                var cur = _cosmosClient.GetContainer(db, container.Id);
                var throughPut = await cur.ReadThroughputAsync();
                Console.WriteLine($" Throughput : {throughPut}");
            }

            return result;
        }

        // Delete a container 
        public async Task<bool> DeleteContainerAsync(string dbId, string containerId)
        {
            try
            {
                var container = _cosmosClient.GetContainer(dbId, containerId);
                if (container != null)
                {
                    await container.DeleteContainerAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> AddItemAsync(Crash crash)
        {
            try
            {
                crash.id = Guid.NewGuid().ToString();
                crash.pk = crash.app + "_" + crash.version;
                ItemResponse<Crash> response = await this._container.CreateItemAsync<Crash>(crash, new PartitionKey(crash.pk));
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<IEnumerable<T>> QueryDocumentsAsync<T>(string query, string pk = "") //where T : BaseModel
        {
            Console.WriteLine($"\n ----- Executing Query ------");

            Console.WriteLine($"\nQuery = {query}");

            List<T> result = new List<T>();
            try
            {
                if (_container != null)
                    _container = _cosmosClient.GetContainer(_databaseId, _containerId);
                if (_container == null) return result;
                QueryRequestOptions rqOptions = null;
                if (pk != "")
                {
                    rqOptions = new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(pk)
                    };
                }
                FeedIterator<dynamic> iterator = null;
                if (rqOptions == null)
                    iterator = _container.GetItemQueryIterator<dynamic>(query);
                else
                    iterator = _container.GetItemQueryIterator<dynamic>(query, requestOptions: rqOptions); // Create an Iterator

                while (iterator.HasMoreResults)
                {
                    var documents = await iterator.ReadNextAsync(); // This is where actual query happens
                    Console.WriteLine($"\nRequest Charge = {documents.RequestCharge}\n");

                    foreach (var doc in documents)
                    {
                        var t = doc.GetType();
                        if (doc is String || doc is Int32 || doc is Int64)
                        {
                            result.Add(doc);
                        }
                        else
                        {
                            try
                            {
                                var document = JsonConvert.DeserializeObject<T>(doc.ToString());
                                result.Add(document);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        }
                    }
                    //if (result.Count > 100) break; // Return only 100 in a query
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        public async Task<IEnumerable<dynamic>> QueryContainerAsync(string query, string pk = "") //where T : BaseModel
        {
            Console.WriteLine($"\n ----- Executing Query ------");

            Console.WriteLine($"\nQuery = {query}");

            List<dynamic> result = new List<dynamic>();

            try
            {
                if (_container != null)
                    _container = _cosmosClient.GetContainer(_databaseId, _containerId);
                if (_container == null) return null;

                QueryRequestOptions rqOptions = null;
                if (pk != "")
                {
                    rqOptions = new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(pk)
                    };
                }
                FeedIterator<dynamic> iterator = null;
                if (rqOptions == null)
                    iterator = _container.GetItemQueryIterator<dynamic>(query);
                else
                    iterator = _container.GetItemQueryIterator<dynamic>(query, requestOptions: rqOptions); // Create an Iterator

                while (iterator.HasMoreResults)
                {
                    var documents = await iterator.ReadNextAsync(); // This is where actual query happens
                    Console.WriteLine($"\nRequest Charge = {documents.RequestCharge}\n");

                    foreach (var doc in documents)
                    {
                        result.Add(doc);
                    }
                    //if (result.Count > 100) break; // Return only 100 in a query
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        private string GetUTCNow(ulong epoch)
        {
            var utc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
            var local = utc.ToLocalTime().ToString();
            return local;
        }
    }// End class
} // End Ns
