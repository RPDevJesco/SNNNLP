using System.Text.Json.Serialization;

namespace SNNNLP
{
    public class WordData
    {
        [JsonPropertyName("WORD")]
        public string Word { get; set; } = string.Empty;

        [JsonPropertyName("MEANINGS")]
        public List<Meaning> Meanings { get; set; } = new List<Meaning>();

        [JsonPropertyName("ANTONYMS")]
        public List<string> Antonyms { get; set; } = new List<string>();

        [JsonPropertyName("SYNONYMS")]
        public List<string> Synonyms { get; set; } = new List<string>();
    }
}