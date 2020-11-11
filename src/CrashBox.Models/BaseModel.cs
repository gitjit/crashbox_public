using System;
//using System.Text.Json.Serialization;
using Newtonsoft.Json;
///
///  I AM DISABLING THE JSONAttribute because of conflict between System.Text.JSON and NewtonSoft
//   Starting .Net Core 3.1 I should use System.Text.Json, but it won't work in Azure functions yet. 
///  So disable serializer for time being.
namespace CrashBox.Models
{
    // public class BaseModel
    // {
    //     // [JsonProperty(PropertyName = "id")]
    //     [JsonPropertyName("id")]
    //     public string Id { get; set; }

    //     //[JsonProperty(PropertyName = "pk")]
    //     [JsonPropertyName("pk")]
    //     public string PKey { get; set; }

    //     //[JsonProperty(PropertyName = "_ts")]
    //     [JsonPropertyName("_ts")]
    //     public ulong TStamp { get; set; }
    // }
     public class BaseModel
    {
        public string id { get; set; }
        public string pk { get; set; }
        public ulong _ts { get; set; }
    }
}