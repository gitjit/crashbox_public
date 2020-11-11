using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CrashBox.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos;

namespace CrashBox.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container. Add cosmos.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
            //     .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));
            services.AddControllers();

           services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(Configuration));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Allows all need to fix later
            app.UseCors(builder =>
           {
               builder
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
           });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private ICosmosDbService InitializeCosmosClientInstanceAsync(IConfiguration configuration)
        {
            // string dbId = Environment.GetEnvironmentVariable("COSMOS_DB_NAME", EnvironmentVariableTarget.Process);
            // string containerId = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_NAME", EnvironmentVariableTarget.Process);
            // string account = Environment.GetEnvironmentVariable("COSMOS_ACCOUNT", EnvironmentVariableTarget.Process);
            // string key = Environment.GetEnvironmentVariable("COSMOS_PRIMARY_KEY", EnvironmentVariableTarget.Process);

            string dbId = configuration["COSMOS_DB_NAME"];
            string containerId = configuration["COSMOS_CONTAINER_NAME"];
            string account = configuration["COSMOS_ACCOUNT"];
            string key = configuration["COSMOS_PRIMARY_KEY"];

            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(account, key);
            CosmosClient client = clientBuilder
                                .WithConnectionModeDirect()
                                .Build();
            ICosmosDbService cosmosDbService = new CosmosDbService(client, dbId, containerId);

            //DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            //await database.Database.CreateContainerIfNotExistsAsync(containerName, "/app");

            return cosmosDbService;
        }

    } //class
} // ns
