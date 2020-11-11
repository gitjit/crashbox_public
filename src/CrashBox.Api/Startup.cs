using System;
using System.Threading.Tasks;
using CrashBox.Cosmos;
using CrashBox.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(CrashBox.Api.Startup))]
namespace CrashBox.Api
{
    public class Startup : FunctionsStartup
    {
        // Configure
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync());
        }

        // Singleton injection
        private static  ICosmosDbService InitializeCosmosClientInstanceAsync()
        {
            string dbId = Environment.GetEnvironmentVariable("COSMOS_DB_NAME", EnvironmentVariableTarget.Process);
            string containerId = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_NAME", EnvironmentVariableTarget.Process);
            string account = Environment.GetEnvironmentVariable("COSMOS_ACCOUNT", EnvironmentVariableTarget.Process);
            string key = Environment.GetEnvironmentVariable("COSMOS_PRIMARY_KEY", EnvironmentVariableTarget.Process);

            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClient client = clientBuilder
                                .WithConnectionModeDirect()
                                .Build();
            ICosmosDbService cosmosDbService = new CosmosDbService(client,dbId,containerId);

            //DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            //await database.Database.CreateContainerIfNotExistsAsync(containerName, "/app");

            return cosmosDbService;
        }
    }
}
