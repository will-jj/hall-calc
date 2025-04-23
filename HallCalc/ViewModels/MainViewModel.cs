using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HallCalc.Models;
using HallCalc.Models.Results;
using Stats = HallCalc.Models.Stats;


namespace HallCalc.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [RelayCommand]
    public void GetData()
    {
        using Stream stream = AssetLoader.Open(new Uri("avares://HallCalc/Assets/combined_data.json"));
        using StreamReader reader = new(stream);
        string json =  reader.ReadToEnd();
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.PropertyNameCaseInsensitive = true;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.IncludeFields = true;
        Dictionary<string, PokemonSet>? sets = JsonSerializer.Deserialize<Dictionary<string, PokemonSet>>(json, options);
        const string targetType = "Fire";
        // IEnumerable<KeyValuePair<string, PokemonSet>> fireSets = sets!
        //     .Where(x => x.Value.Types.Contains(targetType));
        // IOrderedEnumerable<KeyValuePair<string, PokemonSet>> memeay = fireSets.OrderBy(x => x.Value.Id);
        // foreach ((string hi, PokemonSet yo) in memeay)
        // {
        //     Console.WriteLine(hi);
        //     Console.WriteLine(yo.Id);
        // }
        
        PokemonSet ourMon = new();
        string attackerName = "Mudkip";
        ourMon.Ivs = new Stats();
        ourMon.Ivs.SetAll(31);
        ourMon.Evs = new Stats();
        ourMon.Evs.SetAll(40);
        ourMon.Evs.Atk = 252;
        ourMon.Evs.SpA = 252;
        ourMon.Nature = "Adamant";
        ourMon.Level = 35;
        ourMon.Moves = new List<string> { "Waterfall", "Aqua Tail", "Blizzard", "Double-Edge" };
        
        PokemonSet defender = sets!["Bulbasaur"];
        defender.Level = 20;
        defender.Ivs = new Stats();
        defender.Ivs.SetAll(20);

        int round = 7;
        int ivOpp = 8;
        int ivOppInc = 2;
        const int RANKS = 10;
        int[][] groupRanks =
        [
            [1, 5],
            [3, 8],
            [6, 10],
            [9, 10]
        ];
    
        // so linq rn xo
        int[] ivs = Enumerable.Range(0, 10).Select(x => 8 + x * 2).ToArray();
        
        if (!OperatingSystem.IsBrowser())
        {
            return;
        }
        
        // iterate mons by group
        
        IEnumerable<KeyValuePair<string, PokemonSet>> targetSets = sets!
            .Where(x => x.Value.Types.Contains(targetType));
        //IOrderedEnumerable<KeyValuePair<string, PokemonSet>> memeay = targetSets.OrderBy(x => x.Value.Id);
        int userLevel = 30;
        
        for (int group = 1; group <= 4; group++)
        {
            IOrderedEnumerable<KeyValuePair<string, PokemonSet>> groupMons = targetSets
                .Where(x => x.Value.Group.Equals(group))
                .OrderBy(x => x.Value.Id);
            foreach ((string pokemon, PokemonSet set) in groupMons)
            {
                for (int rank = groupRanks[group - 1][0]; rank <= groupRanks[group - 1][1]; rank++)
                {
                    int oppLevel = CalculateLevel(userLevel, round, rank);
                    int oppIvs = ivs[rank-1];
                    Console.WriteLine(pokemon);
                    Console.WriteLine(set.Id);
                    Console.WriteLine(oppLevel);
                    Console.WriteLine(oppIvs);
                    
                    PokemonSet oppMon = sets![pokemon];
                    oppMon.Level = oppLevel;
                    oppMon.Ivs = new();
                    oppMon.Ivs.SetAll(oppIvs);
                    
                    List<CalcResult> attackingOpp = new();
                    List<CalcResult> defendingFromOpp = new();
                    
                    foreach (string move in ourMon.Moves)
                    {
                        string calcRes = DamageCalcInterop.CalculateDamage(attackerName,
                            JsonSerializer.Serialize(ourMon),
                            pokemon,
                            JsonSerializer.Serialize(oppMon),
                            move,
                            "");

                        var res = JsonSerializer.Deserialize<CalcResult>(calcRes);
                        attackingOpp.Add(res);
                    }

                    foreach (string move in oppMon.Moves)
                    {
                        string calcRes = DamageCalcInterop.CalculateDamage(attackerName,
                            JsonSerializer.Serialize(ourMon),
                            pokemon,
                            JsonSerializer.Serialize(oppMon),
                            move,
                            "");

                        var res = JsonSerializer.Deserialize<CalcResult>(calcRes);
                        defendingFromOpp.Add(res);
                    }

                    int stop3= 1;
                }
            }
            
        }
      
        int stop = 1;
        

    }
    
    private static int CalculateLevel(int userLevel, int round, int rank)
    {
        double baseValue = userLevel - 3 * Math.Sqrt(userLevel);
        double increment = Math.Sqrt(userLevel) / 5;

        double rawValue = baseValue + ((round - 1) / 2.0) + (rank - 1) * increment;
        double ceilingValue = Math.Ceiling(rawValue);

        return (int)Math.Min(userLevel, ceilingValue);
    }
}


