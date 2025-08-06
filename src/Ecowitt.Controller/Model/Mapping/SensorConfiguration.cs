using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ecowitt.Controller.Model.Mapping
{
    public class SensorMappingConfiguration
    {
        public Dictionary<string, SensorMapping> ExactMappings { get; set; } = new();
        public List<PatternMapping> PatternMappings { get; set; } = new();
        public HashSet<string> IgnoredProperties { get; set; } = new();
    }

    public class SensorMapping
    {
        public string DisplayName { get; set; } = string.Empty;
        public string BuilderMethod { get; set; } = string.Empty;
        public List<BuilderParameter> Parameters { get; set; } = new();
    }

    public class PatternMapping
    {
        public string Pattern { get; set; } = string.Empty;
        public string DisplayNameFormat { get; set; } = string.Empty;
        public string BuilderMethod { get; set; } = string.Empty;
        public List<BuilderParameter> Parameters { get; set; } = new();
        
        [JsonIgnore]
        public Regex? CompiledPattern { get; set; }
    }

    public class BuilderParameter
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsProperty { get; set; } = false;
        public bool IsNamed { get; set; } = false;
        public string Name { get; set; } = string.Empty;
    }
}