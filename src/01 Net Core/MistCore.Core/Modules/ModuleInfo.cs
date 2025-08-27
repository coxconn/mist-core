using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace MistCore.Core.Modules
{
    public class ModuleInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("version")]
        public Version Version { get; set; }

        [JsonIgnore]
        public Type Type { get; internal set; }
    }
}
