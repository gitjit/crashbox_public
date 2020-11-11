using System.Collections.Generic;
using System.Threading.Tasks;
using CrashBox.Models;

namespace CrashBox.Cosmos
{
    public interface ICosmosDbService
    {
        string DataBaseId { get; }

        string ContainerId { get; }

        Task<bool> CreateDatabaseAsync(string db);

        Task<bool> CreateContainerAsync(string db, string containerId, string pk, int throughput);

        Task<IEnumerable<string>> GetDatabasesAsync();

        Task<IEnumerable<string>> GetContainersAsync(string db);

        Task<bool> DeleteContainerAsync(string db, string container);

        Task<bool> AddItemAsync(Crash crash);

        Task<IEnumerable<T>> QueryDocumentsAsync<T>(string query, string pk = ""); //where T : BaseModel ;

        Task<IEnumerable<dynamic>> QueryContainerAsync(string query, string pk = "");

    }
}
