using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CrashBox.Cosmos;
using CrashBox.Models;
using System.Collections.Generic;
using System.Linq;
using SendGrid.Helpers.Mail;
using System.Text;
using System.Net.Http;
using tables;

namespace CrashBox.Api
{
    public class CrashBoxApi
    {
        private readonly ICosmosDbService _cosmosDbService;
        private ILogger _logger;
        private const string DB = "crashes";
        private const string CONTAINER = "crashes";

        public CrashBoxApi(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;

        }

       
        [FunctionName("InsertCrash")]
        public static async Task<IActionResult> InsertCrash(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = "crash")] HttpRequest req,
          [CosmosDB(databaseName: DB,
            collectionName: CONTAINER,
            ConnectionStringSetting = "CosmosDBConnection")
            ]IAsyncCollector<Crash>  crashes,
          ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var crash = JsonConvert.DeserializeObject<Crash>(requestBody);
                crash.id = Guid.NewGuid().ToString();
                crash.pk = crash.app + "_" + crash.version;
                await crashes.AddAsync(crash); // payload we are sending 
                return new OkObjectResult(crash);
            }
            catch (Exception ex)
            {
                log.LogError($"Couldn't insert item. Exception thrown: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        [FunctionName("Crashes")]
        public async Task<IActionResult> GetCrashes(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "crash")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("JitCrashBox: Test Jithesh Get Crashes Called...!!!!");
            _logger = log;
            try
            {
                // RETURN FIRST PAGE IF NO QUERY PARAMS
                if (string.IsNullOrEmpty(req.QueryString.Value))
                {
                    return await GetRecentCrashes();
                }
                string id = req.Query["id"];
                string pk = req.Query["pk"];
                string qp = req.Query["qp"]; // query param
                string offset = req.Query["page"];
                string mhash = req.Query["mhash"];

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

        [FunctionName("SendEmail")]
        // It works on UTC time, so ensure you corrected based on PST. 
        // https://arminreiter.com/2017/02/azure-functions-time-trigger-cron-cheat-sheet/
        // https://savvytime.com/converter/utc-to-pst
        // 0 0 13 * * *    means trigger @ 1 PM (13.00) UTC which is 6 AM PST (Every day)
        // ("0 0 13 * * *")
        // 0 */1 * * * *"  every 1 minute
        public void SendEmail([TimerTrigger("0 0 13 * * *")] TimerInfo myTimer,
            ILogger log, [SendGrid(ApiKey = "SEND_GRID_KEY")] out SendGridMessage message)
        {
            _logger = log;
            string sender = "crashbox.email@gmail.com";
            string primaryEmail = "jitheshc@gmail.com";
            message = new SendGridMessage();

            message.From = new EmailAddress(sender);
            message.AddTo(new EmailAddress(primaryEmail));

            var htmlTable = CreateHTMLEmailContent();

            message.Subject = "CrashBox Daily Digest";
            //message.PlainTextContent = "Testing my sendgrid function";
            message.HtmlContent = htmlTable;

        }


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

        private string CreateHTMLEmailContent()
        {
            try
            {
                string tableTop = @"<html><head><style>table,th,td{border:1px solid black;border-collapse: collapse;}th,td {padding: 15px;} table.center {margin-left: auto ; margin-right: auto;}</style></head><body>";
                string tableBottom = @"</body></html>";

                var result =  GetTop10Crashes("").Result;
                var topCrashes = result as OkObjectResult;

                if (topCrashes == null) return null;
                var tp = topCrashes.Value as IEnumerable<TopCrash>;
                if (tp == null) return null;

                StringBuilder sb = new StringBuilder();
                using (Html.Table table = new Html.Table(sb, id: "some-id"))
                {
                    table.StartHead();
                    using (var thead = table.AddRow())
                    {
                        //thead.AddCell ("Project");
                        thead.AddCell("Method");
                        thead.AddCell("Count");
                    }
                    table.EndHead();
                    table.StartBody();

                    foreach (var cr in tp)
                    {
                        using (var tr = table.AddRow(classAttributes: "someattributes"))
                        {
                            tr.AddCell(cr.Method);
                            tr.AddCell(cr.Count.ToString());
                            // string version = crash.App.Replace ("HPSmart.", "");
                            // string atag = "<a href = https://smartex-stage.azurewebsites.net/crashes/summary/" + crash.Lochash + "/" + version + ">" + crash.Loc + "</a>";
                            // tr.AddCell (atag);
                            //tr.AddCell (crash.Count.ToString ());
                        }
                    }
                    table.EndBody();

                    var output = tableTop;

                    output += "<p>Hi team,</p>";
                    output += "<p> These are the top crashes reported. For more details visit <a href=\"https://crashbox.z5.web.core.windows.net/\">website</a> .This is an auto-generated email from a webjob. If any questions,please contact <a href=\"mailto:crashbox.email@gmail.com\">CrashBox</a>.</p>";
                    output += "<h4> Top Crashes </h4>";

                    output = output + sb.ToString() + tableBottom;
                    //var tbl = sb.ToString();
                    return output;
                }
            }
            catch (System.Exception ex)
            {
                // _logger.LogError(ex.Message);
                Console.WriteLine(ex.Message);
                return "null";
            }

        }

    } // Class

}
