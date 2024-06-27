using System.Text.Json.Serialization;

namespace SNNNLP
{
    public class Data
    {
        [JsonPropertyName("data")]
        public Dictionary<string, WordData> Words { get; set; }
    }
}