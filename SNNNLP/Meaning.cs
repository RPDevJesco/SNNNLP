using System.Text.Json.Serialization;

namespace SNNNLP
{
    public class Meaning
    {
        [JsonPropertyName("partsOfSpeech")]
        public string PartsOfSpeech { get; set; }

        [JsonPropertyName("definition")]
        public string Definition { get; set; }

        [JsonPropertyName("relatedWords")]
        public List<string> RelatedWords { get; set; }

        [JsonPropertyName("exampleSentence")]
        public List<string> ExampleSentence { get; set; }
    }
}