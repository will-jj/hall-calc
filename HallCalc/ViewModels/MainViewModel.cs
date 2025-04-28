using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using PKHeX.Core;
using Stats = HallCalc.Models.Stats;


namespace HallCalc.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
    
    [ObservableProperty]
    public partial string ShowdownText {get;set;}

    public ObservableCollection<string> Types { get; set; } =
    [
        "Normal",
        "Fire",
        "Water",
        "Electric",
        "Grass",
        "Ice",
        "Fighting",
        "Poison",
        "Ground",
        "Flying",
        "Psychic",
        "Bug",
        "Rock",
        "Ghost",
        "Dragon",
        "Dark",
        "Steel"
    ];
    
    [ObservableProperty]
    public partial string SelectedType {get; set; }
    
    [ObservableProperty]
    public partial int SelectedTypeIndex { get; set; }

    public ObservableCollection<int> Rounds { get; set; } = new(Enumerable.Range(1, 17));

    [ObservableProperty] public partial int SelectedRound { get; set; } = 1;
    [ObservableProperty] public partial int SelectedRoundIndex { get; set; }

    private Dictionary<string, PokemonSet>? _sets;

    private string _ourMonName;
    private string _csvOut;

    public MainViewModel()
    {
        SelectedRoundIndex = 0;
        SelectedTypeIndex = 0;
        LoadData();
    }

    [RelayCommand]
    public Task LoadData()
    {
        using Stream stream = AssetLoader.Open(new Uri("avares://HallCalc/Assets/combined_data.json"));
        using StreamReader reader = new(stream);
        string json =  reader.ReadToEnd();
        _sets = JsonSerializer.Deserialize(json, PokemonSerializationContext.Default.DictionaryStringPokemonSet);
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    public async Task GetData()
    {
        
        if (!OperatingSystem.IsBrowser())
        {
            return;
        }

        
        
        // IEnumerable<KeyValuePair<string, PokemonSet>> fireSets = sets!
        //     .Where(x => x.Value.Types.Contains(targetType));
        // IOrderedEnumerable<KeyValuePair<string, PokemonSet>> memeay = fireSets.OrderBy(x => x.Value.Id);
        // foreach ((string hi, PokemonSet yo) in memeay)
        // {
        //     Console.WriteLine(hi);
        //     Console.WriteLine(yo.Id);
        // }
        try
        {


            PokemonSet ourMon = new();

            ShowdownSet showdownSet = new(ShowdownText);


            // fire bae
            var versions = GameVersion.HGSS;

            ourMon.Item = GameInfo.Strings.Item[showdownSet.HeldItem];
            ourMon.Ivs = new Stats();
            ourMon.Ivs.SetFromShowdown(showdownSet.IVs);
            ourMon.Evs = new Stats();
            ourMon.Evs.SetFromShowdown(showdownSet.EVs);
            ourMon.Nature = showdownSet.Nature.ToString();
            ourMon.Level = showdownSet.Level;
            _ourMonName = GameInfo.Strings.Species[showdownSet.Species];

            ourMon.Moves = [];
            foreach (ushort moveId in showdownSet.Moves)
            {
                ourMon.Moves.Add(GameInfo.Strings.Move[moveId]);
            }

            // redo moves animal style (for hidden power)
            ourMon.Moves = ShowdownText
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(line => line.Trim().StartsWith('-'))
                .Select(line => line.TrimStart(' ', '-').Trim())
                .ToList();

            // do a simple calc to get stats without effort...
            string calcResSelf = DamageCalcInterop.CalculateDamage(_ourMonName,
                JsonSerializer.Serialize(ourMon, PokemonSerializationContext.Default.PokemonSet),
                _ourMonName,
                JsonSerializer.Serialize(ourMon, PokemonSerializationContext.Default.PokemonSet),
                "tackle",
                "");

            CalcResult? resSelf = JsonSerializer.Deserialize<CalcResult>(calcResSelf, CalcResultSerializeOnlyContext.Default.CalcResult);
            StringBuilder csvContent = new StringBuilder();

            csvContent.AppendLine($"Calcing for {_ourMonName} ({ourMon.Item})");
            csvContent.AppendLine($"HP, Speed");
            csvContent.AppendLine($"{resSelf.attacker.stats.hp}, {resSelf.attacker.stats.spe}");


            int[][] groupRanks =
            [
                [1, 5],
                [3, 8],
                [6, 10],
                [9, 10]
            ];

            // so linq rn xo
            int[] ivs = Enumerable.Range(0, 10).Select(x => 8 + x * 2).ToArray();


            // iterate mons by group

            IEnumerable<KeyValuePair<string, PokemonSet>> targetSets = _sets!
                .Where(x => x.Value.Types.Contains(SelectedType));
            for (int group = 1; group <= 4; group++)
            {
                csvContent.AppendLine($"Group: {group}");
                csvContent.AppendLine();
                IOrderedEnumerable<KeyValuePair<string, PokemonSet>> groupMons = targetSets
                    .Where(x => x.Value.Group.Equals(group))
                    .OrderBy(x => x.Value.Id);
                foreach ((string pokemon, PokemonSet set) in groupMons)
                {
                    csvContent.AppendLine($"{pokemon} ({set.Item})");
                    PokemonSet oppMon = _sets![pokemon];
                    if (pokemon == _ourMonName)
                    {
                        continue;
                    }
                    for (int rank = groupRanks[group - 1][0]; rank <= groupRanks[group - 1][1]; rank++)
                    {
                        int oppLevel = CalculateLevel(ourMon.Level, SelectedRound, rank);
                        int oppIvs = ivs[rank - 1];


                        oppMon.Level = oppLevel;
                        oppMon.Ivs = new();
                        oppMon.Ivs.SetAll(oppIvs);

                        List<CalcResult> attackingOpp = new();
                        List<CalcResult> defendingFromOpp = new();
                        string resouter;
                        foreach (string move in ourMon.Moves)
                        {
                            try
                            {
                                string calcRes = DamageCalcInterop.CalculateDamage(_ourMonName,
                                    JsonSerializer.Serialize(ourMon, PokemonSerializationContext.Default.PokemonSet),
                                    pokemon,
                                    JsonSerializer.Serialize(oppMon, PokemonSerializationContext.Default.PokemonSet),
                                    move,
                                    "");
                                resouter = calcRes;
                                var res = JsonSerializer.Deserialize<CalcResult>(calcRes,
                                    CalcResultSerializeOnlyContext.Default.CalcResult);
                                res.CreateDamageStrings(res.defender.stats.hp);
                                attackingOpp.Add(res);
                            }
                            catch (Exception eee)
                            {
                                DamageCalcInterop.ShowAlert(eee.Message);

                                int stop = 1;
                            }


                        }

                        string outerscope;
                        foreach (string move in oppMon.Moves)
                        {
                            try
                            {
                                string calcRes = DamageCalcInterop.CalculateDamage(pokemon,
                                    JsonSerializer.Serialize(oppMon, PokemonSerializationContext.Default.PokemonSet),
                                    _ourMonName,
                                    JsonSerializer.Serialize(ourMon, PokemonSerializationContext.Default.PokemonSet),
                                    move,
                                    "");
                                outerscope = calcRes;
                                var res = JsonSerializer.Deserialize<CalcResult>(calcRes,
                                    CalcResultSerializeOnlyContext.Default.CalcResult);
                                res.CreateDamageStrings(res.defender.stats.hp);
                                defendingFromOpp.Add(res);
                            }
                            catch (Exception eee)
                            {
                                DamageCalcInterop.ShowAlert(eee.Message);

                                int stop = 1;
                            }
                        }

                        //string damageLine = string.Join(",", attackingOpp.Select(x => x.damage.Last())) + "," + string.Join(",", defendingFromOpp.Select(x => x.damage.Last()));
                        string damageLine = string.Join(",", attackingOpp.Select(x => x.damageString)) + "," +
                                            string.Join(",", defendingFromOpp.Select(x => x.damageString));
                        int speed = attackingOpp[0].defender.stats.spe;
                        int speedMinStat = (int)(speed * 2d / 3d);
                        csvContent.AppendLine(
                            $"{rank}, {oppLevel}, {oppIvs}, {attackingOpp[0].defender.stats.hp}, {speed}, {speedMinStat}, {damageLine}");
                    }

                    csvContent.AppendLine();
                }

            }

            _csvOut = csvContent.ToString();
            await DownloadResult();

        }
        catch (Exception e)
        {
            DamageCalcInterop.ShowAlert(e.Message);
        }


    }

    [RelayCommand]
    public async Task DownloadResult()
    {
        TopLevel? topLevel = DialogManager.GetTopLevelForContext(this);
        if (topLevel == null) return;
        FilePickerSaveOptions pickeroptions = new()
        {
            SuggestedFileName = $"{_ourMonName}-{SelectedRound}-{SelectedType}.csv"
        };
        IStorageFile? fileOut = await topLevel.StorageProvider.SaveFilePickerAsync(pickeroptions);
        if (fileOut != null)
        {
            await using Stream stream2 = await fileOut.OpenWriteAsync();
            // Convert the StringBuilder content to bytes
            byte[] csvBytes = System.Text.Encoding.UTF8.GetBytes(_csvOut);

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


