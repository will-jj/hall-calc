using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HallCalc.Models;
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    IncludeFields = true)]
[JsonSerializable(typeof(Dictionary<string, PokemonSet>))]
[JsonSerializable(typeof(PokemonSet))]
[JsonSerializable(typeof(Stats))]
public partial class PokemonSerializationContext : JsonSerializerContext
{
}
    public class PokemonSet
{
    [JsonPropertyName("evs")]
    public Stats Evs { get; set; }
    [JsonPropertyName("ivs")]
    public Stats Ivs { get; set; }
    public List<string> Moves { get; set; }
    [JsonPropertyName("nature")]
    public string Nature { get; set; }
    [JsonPropertyName("item")]
    public string Item { get; set; }
    [JsonPropertyName("level")]
    public int Level { get; set; }
    public string Tier { get; set; }
    public int Group {get; set;}
    public int Id {get; set;} 
    [JsonPropertyName("ability")]
    public string Ability {get; set;}
    public List<string> Types { get; set; }
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

    public void SetAll(int all)
    {
        HP = all;
        Atk = all;
        Def = all;
        SpA = all;
        SpD = all;
        Spe = all;
    }

    public void SetFromShowdown(int[] showDownStats)
    {
        HP = showDownStats[0];
        Atk = showDownStats[1];
        Def = showDownStats[2];
        SpA = showDownStats[4];
        SpD = showDownStats[5];
        Spe = showDownStats[3];
    }
}

