//using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CrashBox.Models
{
    public class Crash : BaseModel
    {
        // //[JsonProperty(PropertyName = "app")]
        // [JsonPropertyName("app")]
        // public string App { get; set; }

        // //[JsonProperty(PropertyName = "version")]
        //  [JsonPropertyName("version")]
        // public string Version { get; set; }

        // //[JsonProperty(PropertyName = "os")]
        //  [JsonPropertyName("os")]
        // public string Os { get; set; }

        // //[JsonProperty(PropertyName = "region")]
        //  [JsonPropertyName("region")]
        // public string Region { get; set; }

        // //[JsonProperty(PropertyName = "language")]
        // [JsonPropertyName("language")]
        // public string Language { get; set; }

        // //[JsonProperty(PropertyName = "method")]
        // [JsonPropertyName("method")]
        // public string Method { get; set; } // Method call that create the exception

        // //[JsonProperty(PropertyName = "mhash")]
        // [JsonPropertyName("mhash")]
        // public ulong Mhash { get; set; }

        // [JsonPropertyName("log")]
        // //[JsonProperty(PropertyName = "log")]
        // public string Log { get; set; }

        // [JsonPropertyName("extype")]
        // //[JsonProperty(PropertyName = "extype")]
        // public string ExType { get; set; }


        // //[JsonProperty(PropertyName = "stack")]
        // [JsonPropertyName("stack")]
        // public string Stack { get; set; }


      
        public string app { get; set; }
        public string version { get; set; }
        public string os { get; set; }
        public string region { get; set; }
        public string language { get; set; }
        public string method { get; set; } // Method call that create the exception
        public ulong mhash { get; set; }
        public string log { get; set; }
        public string extype { get; set; }
        public string stack { get; set; }
    }
}