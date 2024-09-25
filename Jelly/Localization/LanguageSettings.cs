using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jelly.Localization;

public class LanguageSettings(string identifier)
{
    [JsonIgnore]
    public string Identifier { get; set; } = identifier;

    public string Name { get; set; } = identifier;

    public Dictionary<string, string> Values { get; set; } = [];

    public string Fallback { get; set; }
}
