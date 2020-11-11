using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrashBox.Cosmos;
using CrashBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CrashBox.WebApi.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CrashController : ControllerBase
    {

        private readonly ILogger<CrashController> _logger;
        private ICosmosDbService _cosmosDbService;

        public CrashController(ILogger<CrashController> logger, ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Crash crash)
        {
            var result = await _cosmosDbService.AddItemAsync(crash);

            if (result == true)
                return Created(crash.id, crash.ToString());
            else
                return BadRequest();
        }

        // Get calls
        [HttpGet]
        public async Task<IActionResult> Get(/*[FromQuery] string qp*/)
        {
            try
            {
                var queryParam = HttpContext.Request.Query;

                // RETURN FIRST PAGE IF NO QUERY PARAMS
                if (queryParam == null)
                {
                    return await GetRecentCrashes();
                }

                // var qs = HttpContext.Request.Query["qp"];
                // var pk = HttpContext.Request.Query["pk"];

                string id = queryParam["id"];
                string pk = queryParam["pk"];
                string qp = queryParam["qp"]; // query param
                string offset = queryParam["page"];
                string mhash = queryParam["mhash"];

                //By Offset
                if (!string.IsNullOrEmpty(offset) && !string.IsNullOrEmpty(pk))
                {
                    return await GetRecentCrashes(offset, pk);
                }

                // By Id
                if (!string.IsNullOrEmpty(id))
                {
                    return await GetCrashById(id, pk);
                }

                // By Method 
                if (!string.IsNullOrEmpty(mhash) && !string.IsNullOrEmpty(pk))
                {
                    return await GetCrashByMethod(mhash, pk);
                }

                // By Query Param
                if (!string.IsNullOrEmpty(qp))
                {
                    switch (qp.ToLower())
                    {
                        case "top10":
                            return await GetTop10Crashes(pk);
                        case "projects":
                            return await GetProjects();
                        case "count":
                            return await GetTotalCrashes(pk);
                        case "latest":
                            return await GetLatestCrash(pk);
                        default:
                            return new NotFoundResult();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Console.WriteLine(ex.Message);
                return new StatusCodeResult(503);
            }
            return new NotFoundObjectResult("Invalid Request: ");
        }

        // Helpers (Move to seperate class / library)
        // This method returns the  crashes insert in the Db based on offset
        // limit : number of documents to be returned from Db
        private async Task<IActionResult> GetRecentCrashes(string offset, string pk)
        {
            try
            {
                int offsetInt = 0;
                int limit = 15;
                List<Crash> result = new List<Crash>();

                if (Int32.TryParse(offset, out offsetInt))
                {
                    if (offsetInt > 0)
                    {
                        offset = (offsetInt * 15).ToString();
                    }
                }
                else
                {
                    offset = "0";
                }

                string query = $"SELECT* FROM c WHERE c.pk='{pk}' ORDER BY c._ts DESC OFFSET {offset} LIMIT { limit}";
                _logger.LogInformation(query);

                var documents = await _cosmosDbService.QueryContainerAsync(query);

                foreach (var doc in documents)
                {
                    try
                    {
                        var crash = JsonConvert.DeserializeObject<Crash>(doc.ToString());
                        result.Add(crash);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        Console.WriteLine(ex.Message);
                    }

                }
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Console.WriteLine("Exception :" + ex.Message);
                return new StatusCodeResult(503);
            }
        }


        // This method returs the last 15 crashes insert in the Db
        // limit : number of documents to be returned from Db
        private async Task<IActionResult> GetRecentCrashes(int limit = 15)
        {
            try
            {
                List<Crash> result = new List<Crash>();
                string query = $"SELECT TOP {limit} * FROM c  ORDER BY c._ts DESC";
                _logger.LogInformation(query);

                var documents = await _cosmosDbService.QueryContainerAsync(query);

                foreach (var doc in documents)
                {
                    var crash = JsonConvert.DeserializeObject<Crash>(doc.ToString());
                    result.Add(crash);
                }

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Console.WriteLine("Exception :" + ex.Message);
                return new StatusCodeResult(503);
            }
        }


        // Crash By Method Hash
        // mhash : Method hash 
        // pk : Partition key of the container
        private async Task<IActionResult> GetCrashByMethod(string mhash, string pk)
        {
            try
            {
                string query = $"SELECT TOP 1 * FROM c WHERE c.mhash={mhash} AND c.pk='{pk}' AND NOT(c.log = '0')";
                _logger.LogInformation(query);
                var documents = await _cosmosDbService.QueryContainerAsync(query);
                if (documents == null || documents.Count() == 0)
                {
                    query = $"SELECT TOP 1 * FROM c WHERE c.mhash={mhash} AND c.pk='{pk}'"; // No log
                    documents = await _cosmosDbService.QueryContainerAsync(query);
                }

                if (documents != null && documents.Count() > 0)
                {
                    var crash = JsonConvert.DeserializeObject<Crash>(documents.FirstOrDefault().ToString());
                    return new OkObjectResult(crash);
                }
                return new NotFoundResult();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }

        }

        // Query By Id & PK
        // Id : Id of the document
        // pk : Partition key of the container
        private async Task<IActionResult> GetCrashById(string id, string pk = "")
        {
            try
            {
                string query = $"SELECT *  FROM c WHERE c.id='{id}'";
                _logger.LogInformation(query);

                if (!string.IsNullOrEmpty(pk))
                    query = $"SELECT *  FROM c WHERE c.pk='{pk}' AND c.id='{id}'";

                var documents = await _cosmosDbService.QueryContainerAsync(query);
                if (documents != null && documents.Count() > 0)
                {
                    var crash = JsonConvert.DeserializeObject<Crash>(documents.FirstOrDefault().ToString());
                    return new OkObjectResult(crash);
                }
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }

        // Top 10 Crashes
        // pk : Partition key of the container
        private async Task<IActionResult> GetTop10Crashes(string pk = "")
        {
            List<TopCrash> topCrashes = new List<TopCrash>();
            try
            {
                string query = $"SELECT c.method, c.mhash, COUNT(1) AS count FROM c GROUP BY c.method,c.mhash";
                if (!string.IsNullOrEmpty(pk))
                    query = $"SELECT c.method, c.mhash, COUNT(1) AS count FROM c WHERE c.pk='{pk}' GROUP BY c.method,c.mhash";

                _logger.LogInformation(query);

                var documents = await _cosmosDbService.QueryContainerAsync(query);
                foreach (var doc in documents)
                {
                    try
                    {
                        var tp = JsonConvert.DeserializeObject<TopCrash>(doc.ToString());
                        topCrashes.Add(tp);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                if (topCrashes.Count > 0)
                {
                    var top10 = topCrashes.ToList().OrderByDescending(t => t.Count).Take(10).ToList();
                    return new OkObjectResult(top10);
                }

                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Console.WriteLine("Exception :" + ex.Message);
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }

        // Get Projects in container
        private async Task<IActionResult> GetProjects()
        {
            try
            {
                List<string> projects = new List<string>();
                string query = "SELECT DISTINCT c.pk  from c";
                var documents = await _cosmosDbService.QueryContainerAsync(query);
                _logger.LogInformation(query);

                foreach (var doc in documents)
                {
                    var app = JsonConvert.DeserializeObject<BaseModel>(doc.ToString());
                    if (app.pk != null)
                        projects.Add(app.pk);
                }
                return new OkObjectResult(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Console.WriteLine("Exception :" + ex.Message);
                return new StatusCodeResult(503);
            }
        }

        // Get total crashes in a project 
        private async Task<IActionResult> GetTotalCrashes(string pk)
        {
            try
            {
                if (!string.IsNullOrEmpty(pk))
                {
                    string query = $"SELECT VALUE COUNT(1)  FROM c WHERE c.pk = '{pk}'";
                    _logger.LogInformation(query);
                    var result = await _cosmosDbService.QueryContainerAsync(query);
                    if (result != null)
                        return new OkObjectResult(result.FirstOrDefault());
                }
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Console.WriteLine("Exception :" + ex.Message);
                return new StatusCodeResult(503);
            }
        }

        // Get Latest crash in a project
        private async Task<IActionResult> GetLatestCrash(string pk)
        {
            try
            {
                if (!string.IsNullOrEmpty(pk))
                {
                    string query = $"SELECT TOP 1 * FROM c WHERE c.pk = '{pk}' ORDER BY c._ts DESC";
                    _logger.LogInformation(query);
                    var documents = await _cosmosDbService.QueryContainerAsync(query);
                    if (documents != null && documents.Count() > 0)
                    {
                        var crash = JsonConvert.DeserializeObject<Crash>(documents.FirstOrDefault().ToString());
                        return new OkObjectResult(crash);
                    }
                }
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Console.WriteLine("Exception :" + ex.Message);
                return new StatusCodeResult(503);
            }
        }

    }// class
} // ns
