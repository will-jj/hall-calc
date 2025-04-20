using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HallCalc.Models;

public class PokemonSet
{
    [JsonPropertyName("evs")]
    public Stats Evs { get; set; }
    public List<string> Moves { get; set; }
    [JsonPropertyName("nature")]
    public string Nature { get; set; }
    [JsonPropertyName("item")]
    public string Item { get; set; }
    [JsonPropertyName("level")]
    public int Level { get; set; }
    public string Tier { get; set; }
}

public partial class Stats
{
    [JsonPropertyName("hp")]
    public int HP { get; set; }

    [JsonPropertyName("atk")]
    public int Atk { get; set; }

    [JsonPropertyName("def")]
    public int Def { get; set; }

    [JsonPropertyName("spa")]
    public int SpA { get; set; }

    [JsonPropertyName("spd")]
    public int SpD { get; set; }

    [JsonPropertyName("spe")]
    public int Spe { get; set; }
}