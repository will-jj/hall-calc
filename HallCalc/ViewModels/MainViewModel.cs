using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HallCalc.Models;
using HallCalc.Models.Results;
using HallCalc.Services;
using Stats = HallCalc.Models.Stats;


namespace HallCalc.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [RelayCommand]
    public async Task GetData()
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
        string ourMonName = "Mudkip";
        ourMon.Ivs = new Stats();
        ourMon.Ivs.SetAll(31);
        ourMon.Evs = new Stats();
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
        StringBuilder csvContent = new StringBuilder();
        for (int group = 1; group <= 4; group++)
        {
            csvContent.AppendLine($"Group: {group}");
            csvContent.AppendLine();
            IOrderedEnumerable<KeyValuePair<string, PokemonSet>> groupMons = targetSets
                .Where(x => x.Value.Group.Equals(group))
                .OrderBy(x => x.Value.Id);
            foreach ((string pokemon, PokemonSet set) in groupMons)
            {
                csvContent.AppendLine(pokemon);
                PokemonSet oppMon = sets![pokemon];
                csvContent.AppendLine($"Rank, Op. Level, Op. IVs, Op. Speed, {string.Join(',', ourMon.Moves)}, {string.Join(',', oppMon.Moves)}");
                for (int rank = groupRanks[group - 1][0]; rank <= groupRanks[group - 1][1]; rank++)
                {
                    int oppLevel = CalculateLevel(userLevel, round, rank);
                    int oppIvs = ivs[rank-1];
                    Console.WriteLine(pokemon);
                    Console.WriteLine(set.Id);
                    Console.WriteLine(oppLevel);
                    Console.WriteLine(oppIvs);
                    
                    
                    oppMon.Level = oppLevel;
                    oppMon.Ivs = new();
                    oppMon.Ivs.SetAll(oppIvs);
                    
                    List<CalcResult> attackingOpp = new();
                    List<CalcResult> defendingFromOpp = new();
                    
                    foreach (string move in ourMon.Moves)
                    {
                        string calcRes = DamageCalcInterop.CalculateDamage(ourMonName,
                            JsonSerializer.Serialize(ourMon),
                            pokemon,
                            JsonSerializer.Serialize(oppMon),
                            move,
                            "");

                        var res = JsonSerializer.Deserialize<CalcResult>(calcRes);
                        res.CreateDamageStrings(res.defender.stats.hp);
                        attackingOpp.Add(res);
                    }

                    foreach (string move in oppMon.Moves)
                    {
                        string calcRes = DamageCalcInterop.CalculateDamage(pokemon,
                            JsonSerializer.Serialize(oppMon),
                            ourMonName,
                            JsonSerializer.Serialize(ourMon),
                            move,
                            "");

                        var res = JsonSerializer.Deserialize<CalcResult>(calcRes);
                        res.CreateDamageStrings(res.defender.stats.hp);
                        defendingFromOpp.Add(res);
                    }
                    //string damageLine = string.Join(",", attackingOpp.Select(x => x.damage.Last())) + "," + string.Join(",", defendingFromOpp.Select(x => x.damage.Last()));
                    string damageLine = string.Join(",", attackingOpp.Select(x => x.damageString)) + "," + string.Join(",", defendingFromOpp.Select(x => x.damageString));
                    csvContent.AppendLine($"{rank}, {oppLevel}, {oppIvs}, {attackingOpp[0].defender.stats.spe}, {damageLine}");
                }
                csvContent.AppendLine();
            }
            
        }
        TopLevel? topLevel = DialogManager.GetTopLevelForContext(this);
        if (topLevel == null) return;
        FilePickerSaveOptions pickeroptions = new()
        {
            SuggestedFileName = "out.csv"
        };
        IStorageFile? fileOut = await topLevel.StorageProvider.SaveFilePickerAsync(pickeroptions);
        if (fileOut != null)
        {
            await using Stream stream2 = await fileOut.OpenWriteAsync();
            // Convert the StringBuilder content to bytes
            byte[] csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent.ToString());
    
            // Write the bytes to the stream
            await stream2.WriteAsync(csvBytes, 0, csvBytes.Length);
        }
        

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


