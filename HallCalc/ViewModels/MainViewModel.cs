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

    [ObservableProperty] public partial string ShowdownText { get; set; }

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

    [ObservableProperty] public partial string SelectedType { get; set; }

    [ObservableProperty] public partial int SelectedTypeIndex { get; set; }

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
        string json = reader.ReadToEnd();
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

        try
        {
            PokemonSet ourMon = new();

            ShowdownSet showdownSet = new(ShowdownText);

            ourMon.Item = GameInfo.Strings.Item[showdownSet.HeldItem];
            ourMon.Ivs = new Stats();
            ourMon.Ivs.SetFromShowdown(showdownSet.IVs);
            ourMon.Evs = new Stats();
            ourMon.Evs.SetFromShowdown(showdownSet.EVs);
            ourMon.Nature = showdownSet.Nature.ToString();
            ourMon.Level = showdownSet.Level;
            ourMon.Ability = GameInfo.Strings.Ability[showdownSet.Ability];

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

            CalcResult? resSelf =
                JsonSerializer.Deserialize<CalcResult>(calcResSelf, CalcResultSerializeOnlyContext.Default.CalcResult);
            StringBuilder csvContent = new StringBuilder();

            csvContent.AppendLine($"Calcing for {_ourMonName} ({ourMon.Item} / {ourMon.Ability})");
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
                    List<(string Ability, List<Result> calcResults)> results = [];
                    PokemonSet oppMon = _sets![pokemon];
                    foreach (string ability in GetAbilities(pokemon))
                    {
                        List<Result> calcResults = [];
                        set.Ability = ability;

                        if (pokemon == _ourMonName)
                        {
                            continue;
                        }

                        for (int rank = groupRanks[group - 1][0]; rank <= groupRanks[group - 1][1]; rank++)
                        {
                            Result calcResult = new();
                            calcResult.Rank = rank;

                            int oppLevel = CalculateLevel(ourMon.Level, SelectedRound, rank);
                            int oppIvs = ivs[rank - 1];

                            calcResult.OppIvs = oppIvs;
                            calcResult.OppLevel = oppLevel;

                            oppMon.Level = oppLevel;
                            oppMon.Ivs = new();
                            oppMon.Ivs.SetAll(oppIvs);

                            foreach (string move in ourMon.Moves)
                            {
                                try
                                {
                                    string calcRes = DamageCalcInterop.CalculateDamage(_ourMonName,
                                        JsonSerializer.Serialize(ourMon,
                                            PokemonSerializationContext.Default.PokemonSet),
                                        pokemon,
                                        JsonSerializer.Serialize(oppMon,
                                            PokemonSerializationContext.Default.PokemonSet),
                                        move,
                                        "");
                                    CalcResult? res = JsonSerializer.Deserialize<CalcResult>(calcRes,
                                        CalcResultSerializeOnlyContext.Default.CalcResult);
                                    res.CreateDamageStrings(res.defender.stats.hp);
                                    calcResult.Attacking.Add(res);
                                }
                                catch (Exception ex)
                                {
                                    DamageCalcInterop.ShowAlert(ex.Message);
                                }
                            }

                            foreach (string move in oppMon.Moves)
                            {
                                try
                                {
                                    string calcRes = DamageCalcInterop.CalculateDamage(pokemon,
                                        JsonSerializer.Serialize(oppMon,
                                            PokemonSerializationContext.Default.PokemonSet),
                                        _ourMonName,
                                        JsonSerializer.Serialize(ourMon,
                                            PokemonSerializationContext.Default.PokemonSet),
                                        move,
                                        "");
                                    CalcResult? res = JsonSerializer.Deserialize<CalcResult>(calcRes,
                                        CalcResultSerializeOnlyContext.Default.CalcResult);
                                    res.CreateDamageStrings(res.defender.stats.hp);
                                    calcResult.Defending.Add(res);
                                }
                                catch (Exception ex)
                                {
                                    DamageCalcInterop.ShowAlert(ex.Message);
                                }
                            }

                            calcResults.Add(calcResult);
                        }

                        results.Add((ability, calcResults));
                    }

                    // we have one or two abilites, and only want to show them separately if the results are... different 
                    bool differentAttacking = false;
                    bool differentDefending = false;
                    if (results.Count == 2)
                    {
                        Result firstOfFirst = results[0].calcResults[0];
                        Result firstOfSecond = results[1].calcResults[0];

                        // check each move for attack and defend across abilities
                        for (int ii = 0; ii < 4; ii++)
                        {
                            if (!firstOfFirst.Attacking[ii].damage.SequenceEqual(firstOfSecond.Attacking[ii].damage))
                            {
                                differentAttacking = true;
                            }

                            if (!firstOfFirst.Defending[ii].damage.SequenceEqual(firstOfSecond.Defending[ii].damage))
                            {
                                differentDefending = true;
                            }
                        }

                        if (differentDefending || differentAttacking)
                        {
                            // Need to print them out separately 
                            foreach ((string ability, List<Result> calcResults) calcResult in results)
                            {
                                AppendResultsToCsv(csvContent, pokemon, set.Item, calcResult.ability,
                                    calcResult.calcResults, ourMon.Moves, oppMon.Moves);
                            }
                        }
                        else
                        {
                            string abilitiesString = $"{results[0].Ability} or {results[1].Ability}";
                            AppendResultsToCsv(csvContent, pokemon, set.Item, abilitiesString, results[0].calcResults,
                                ourMon.Moves, oppMon.Moves);
                        }
                    }
                    else if (results.Count == 1)
                    {
                        AppendResultsToCsv(csvContent, pokemon, set.Item, results[0].Ability, results[0].calcResults, ourMon.Moves, oppMon.Moves);
                    }
                    
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

    private List<string> GetAbilities(string species)
    {
        // Why do you curse me
        if (species.Contains("Wormadam"))
        {
            species = "Wormadam";
        }

        species = species.Replace("-M", "♂");
        species = species.Replace("-F", "♀");
        species = species.Replace('\'', '’');

        List<string> abilities = [];
        PK4 pokemon = new PK4();
        List<string> speciesList = GameInfo.Strings.Species.ToList();
        int speciesIndex = speciesList.IndexOf(species);
        pokemon.Species = (ushort)speciesIndex;


        pokemon.RefreshAbility(0);
        string firstAbility = GameInfo.Strings.Ability[pokemon.Ability];
        abilities.Add(firstAbility);
        pokemon.RefreshAbility(1);
        string secondAbility = GameInfo.Strings.Ability[pokemon.Ability];
        if (firstAbility != secondAbility)
        {
            abilities.Add(secondAbility);
        }

        return abilities;
    }

    void AppendResultsToCsv(StringBuilder csvContent,
        string pokemon,
        string item,
        string abilities,
        IEnumerable<Result> calcResults,
        IEnumerable<string> ourMoves,
        IEnumerable<string> oppMoves)
    {
        csvContent.AppendLine($"{pokemon} ({item} / {abilities})");
        csvContent.AppendLine(
            $"Rank, Opp. Level, Opp. IVs, Opp. HP, Opp. Speed, Opp. Speed (-1), {string.Join(',', ourMoves)}, {string.Join(',', oppMoves)}");

        foreach (Result result in calcResults)
        {
            string damageLine = string.Join(",", result.Attacking.Select(x => x.damageString)) + "," +
                                string.Join(",", result.Defending.Select(x => x.damageString));
            int hp = result.Attacking[0].defender.stats.hp;
            int speed = result.Attacking[0].defender.stats.hp;
            int speedMinStat = (int)(speed * 2d / 3d);

            csvContent.AppendLine(
                $"{result.Rank}, {result.OppLevel}, {result.OppIvs}, {hp}, {speed}, {speedMinStat}, {damageLine}");
        }

        csvContent.AppendLine();
    }
}