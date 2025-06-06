using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
        "Steel",
        "Argenta-1",
        "Argenta-2",
        "Post-Argenta-2"
    ];

    [ObservableProperty] public partial string SelectedType { get; set; }

    [ObservableProperty] public partial int SelectedTypeIndex { get; set; }

    public ObservableCollection<int> Rounds { get; set; } = new(Enumerable.Range(1, 17));

    [ObservableProperty] public partial int SelectedRound { get; set; } = 1;
    [ObservableProperty] public partial int SelectedRoundIndex { get; set; }

    [ObservableProperty] public partial bool ShowProbability { get; set; }
    [ObservableProperty] public partial bool ShowHallIndex { get; set; }
    [ObservableProperty] public partial bool IncludeShowdown { get; set; }
    [ObservableProperty] public partial int ChanceType { get; set; }

    [ObservableProperty] public partial int DecimalPlaces { get; set; } = 1;

    private Dictionary<string, PokemonSet>? _sets;

    private string _ourMonName;
    private string _csvOut;

    private readonly int[][] _groupRanks =
    [
        [1, 5],
        [3, 8],
        [6, 10],
        [9, 10]
    ];

    private readonly int[] _ivs = Enumerable.Range(0, 10).Select(x => 8 + x * 2).ToArray();

    enum ChanceTypeEnum
    {
        Fraction,
        Percentage
    }

    private enum RankToArgenta
    {
        Argenta1 = 100,
        Argenta2 = 200,
    }

    private enum CalcType
    {
        SingleType,
        Argenta,
        PostArgenta2
    }

    public MainViewModel()
    {
        SelectedRoundIndex = 0;
        SelectedTypeIndex = 0;
        LoadData();
        PokemonSet set = new PokemonSet();
        PokemonSet userSet = new();
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
        (PokemonSet ourMon, _ourMonName) = PokemonSetFromShowdown();
        var who = _sets.TryGetValue(_ourMonName, out PokemonSet? poSet);

        int ourGroup = -1;
        if (who)
        {
            ourGroup = poSet!.Group;
        }
        else
        {
            if (OperatingSystem.IsBrowser())
            {
                DamageCalcInterop.ShowAlert("Unable to match Pokemon Name to Set");
            }
            return;
        }

        if (!OperatingSystem.IsBrowser())
        {
            return;
        }

        if (SelectedTypeIndex is 17 or 18)
        {
            // No probabilities on Argenta
            ShowProbability = false;
        }

        try
        {
            StringBuilder csvContent = new StringBuilder();
          
            WriteHeader(ourMon, csvContent);

            int groupStart;
            int groupEnd;
            
            CalcType calcType;

            switch (SelectedType)
            {
                case "Argenta-1":
                    groupStart = ourGroup;
                    groupEnd = ourGroup;
                    calcType = CalcType.Argenta;
                    break;
                case "Argenta-2":
                    groupStart = 4;
                    groupEnd = 4;
                    calcType = CalcType.Argenta;
                    break;
                case "Post-Argenta-2":
                    groupStart = 3;
                    groupEnd = 4;
                    calcType = CalcType.PostArgenta2;
                    break;
                default:
                    groupStart = 1;
                    groupEnd = 4;
                    calcType = CalcType.SingleType;
                    break;
            }

            IEnumerable<KeyValuePair<string, PokemonSet>> targetSets;
            if (calcType == CalcType.SingleType)
            {
                targetSets = _sets!
                    .Where(x => x.Value.Types.Contains(SelectedType));
            }
            else
            {
                targetSets = _sets!;
            }



                
                // Small repetition for quick post Argenta fix 

                if (calcType == CalcType.PostArgenta2)
                {
                    for (int typeIndex=0;typeIndex<=17;typeIndex++)
                    {
                        string type = Types[typeIndex];
                        csvContent.AppendLine(type);
                        csvContent.AppendLine();
                        for (int group = groupStart; group <= groupEnd; group++)
                        {
                            csvContent.AppendLine($"Group: {group}");
                            csvContent.AppendLine();
                            IOrderedEnumerable<KeyValuePair<string, PokemonSet>> groupTypeMons = targetSets
                                .Where(x => x.Value.Group.Equals(group))
                                .Where(x => x.Value.Types.Contains(type))
                                .OrderBy(x => x.Value.Id);
                            IterateGroup(groupTypeMons, group, calcType, type, ourMon, csvContent);
                        }

                    }
                }
                else
                {
                    for (int group = groupStart; group <= groupEnd; group++)
                    {
                        csvContent.AppendLine($"Group: {group}");
                        csvContent.AppendLine();
                        IOrderedEnumerable<KeyValuePair<string, PokemonSet>> groupMons = targetSets
                            .Where(x => x.Value.Group.Equals(group))
                            .OrderBy(x => x.Value.Id);
                        IterateGroup(groupMons, group, calcType, SelectedType, ourMon, csvContent);
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

    private void IterateGroup(IOrderedEnumerable<KeyValuePair<string, PokemonSet>> groupMons,
        int group,
        CalcType calcType,
        string oppType,
        PokemonSet ourMon,
        StringBuilder csvContent)
    {
        foreach ((string pokemon, PokemonSet set) in groupMons)
        {
            (List<(string Ability, List<Result> calcResults)> results, PokemonSet oppMon) = GetResultsForMon(
                pokemon,
                set,
                group,
                calcType,
                oppType,
                ourMon);

            AddResultsToString(results, csvContent, pokemon, set, ourMon, oppMon);
        }
    }

    private void WriteHeader(PokemonSet ourMon, StringBuilder csvContent)
    {
        if (!OperatingSystem.IsBrowser())
        {
            return;
        }

        // do a simple calc to get stats without effort...
        string calcResSelf = DamageCalcInterop.CalculateDamage(_ourMonName,
            JsonSerializer.Serialize(ourMon, PokemonSerializationContext.Default.PokemonSet),
            _ourMonName,
            JsonSerializer.Serialize(ourMon, PokemonSerializationContext.Default.PokemonSet),
            "tackle",
            "");

        CalcResult? resSelf =
            JsonSerializer.Deserialize<CalcResult>(calcResSelf, CalcResultSerializeOnlyContext.Default.CalcResult);


        csvContent.AppendLine($"Calcing for {_ourMonName} ({ourMon.Item} / {ourMon.Ability})");
        csvContent.AppendLine($"HP, Speed");
        csvContent.AppendLine($"{resSelf.attacker.stats.hp}, {resSelf.attacker.stats.spe}");
        if (IncludeShowdown)
        {
            csvContent.AppendLine();
            csvContent.AppendLine(ShowdownText);
            csvContent.AppendLine();
        }
    }

    private void AddResultsToString(List<(string Ability, List<Result> calcResults)> results, StringBuilder csvContent,
        string pokemon, PokemonSet set,
        PokemonSet ourMon, PokemonSet oppMon)
    {
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
                    AppendResultsToCsv(csvContent, pokemon, set, calcResult.ability,
                        calcResult.calcResults, ourMon.Moves, oppMon.Moves);
                }
            }
            else
            {
                string abilitiesString = $"{results[0].Ability} or {results[1].Ability}";
                AppendResultsToCsv(csvContent, pokemon, set, abilitiesString, results[0].calcResults,
                    ourMon.Moves, oppMon.Moves);
            }
        }
        else if (results.Count == 1)
        {
            AppendResultsToCsv(csvContent, pokemon, set, results[0].Ability, results[0].calcResults,
                ourMon.Moves, oppMon.Moves);
        }
    }


    private (List<(string Ability, List<Result> calcResults)> results, PokemonSet oppMon) GetResultsForMon(
        string pokemon, PokemonSet set, int group, CalcType calcType, string oppType, PokemonSet ourMon)
    {
        List<(string Ability, List<Result> calcResults)> results = [];
        PokemonSet oppMon = _sets![pokemon];

        if (!OperatingSystem.IsBrowser())
        {
            return (results, oppMon);
        }

        foreach (string ability in GetAbilities(pokemon))
        {
            List<Result> calcResults = [];
            set.Ability = ability;

            if (pokemon == _ourMonName)
            {
                continue;
            }

            int rankStart;
            int rankEnd;
            switch (calcType)
            {
                case CalcType.SingleType:
                    rankStart = _groupRanks[group - 1][0];
                    rankEnd = _groupRanks[group - 1][1];
                    break;
                case CalcType.Argenta:
                    rankStart = 99;
                    rankEnd = 99;
                    break;
                case CalcType.PostArgenta2:
                    rankStart = 10;
                    rankEnd = 10;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(calcType), calcType, null);
            }
            

            for (int rank = rankStart; rank <= rankEnd; rank++)
            {
                Result calcResult = new();
                calcResult.Rank = rank;

                int oppLevel;
                int oppIvs;

                if (calcType == CalcType.Argenta)
                {
                    oppLevel = ourMon.Level;
                    oppIvs = 31;
                }
                else
                {
                    oppLevel = CalculateLevel(ourMon.Level, calcType == CalcType.PostArgenta2 ? 17 : SelectedRound, rank);
                    oppIvs = _ivs[rank - 1];
                }

                calcResult.OppIvs = oppIvs;
                calcResult.OppLevel = oppLevel;

                oppMon.Level = oppLevel;
                oppMon.Ivs = new Stats();
                oppMon.Ivs.SetAll(oppIvs);

                foreach (string move in ourMon.Moves)
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
                    res.CreateDamageStrings(res.defender.stats.hp, DecimalPlaces);
                    calcResult.Attacking.Add(res);
                }

                foreach (string move in oppMon.Moves)
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
                    res.CreateDamageStrings(res.defender.stats.hp, DecimalPlaces);
                    calcResult.Defending.Add(res);
                }

                if (ShowProbability)
                {
                    (int count, int setSize) chanceRes = CalcBaseChance(rank, oppMon, ourMon, oppType);
                    calcResult.Probability = (ChanceTypeEnum)ChanceType == ChanceTypeEnum.Fraction
                        ? $"{chanceRes.count} / {chanceRes.setSize}"
                        : $"{(100d * chanceRes.count / chanceRes.setSize):F2}";
                }

                calcResults.Add(calcResult);
            }


            results.Add((ability, calcResults));
        }

        return (results, oppMon);
    }

    private (PokemonSet ourMon, string ourMonName) PokemonSetFromShowdown()
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

        string ourMonName = GameInfo.Strings.Species[showdownSet.Species];

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
        return (ourMon, ourMonName);
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

    private void AppendResultsToCsv(StringBuilder csvContent,
        string pokemon,
        PokemonSet set,
        string abilities,
        IEnumerable<Result> calcResults,
        IEnumerable<string> ourMoves,
        IEnumerable<string> oppMoves)
    {
        string probabilityHeader = string.Empty;
        bool includeProbabilities = false;
        if (!string.IsNullOrEmpty(calcResults.First().Probability))
        {
            probabilityHeader = (ChanceTypeEnum)ChanceType == ChanceTypeEnum.Fraction ? "Prob. ," : "Prob. [%],";

            includeProbabilities = true;
        }

        string hallIndex = string.Empty;
        if (ShowHallIndex)
        {
            hallIndex = $", H{set.Id}";
        }

        csvContent.AppendLine($"{pokemon} ({set.Item} / {abilities}){hallIndex}");
        csvContent.AppendLine(
            $"Rank,{probabilityHeader} Opp. Level, Opp. IVs, Opp. HP, Opp. Speed, {string.Join(',', ourMoves)}, {string.Join(',', oppMoves)}");

        foreach (Result result in calcResults)
        {
            string damageLine = string.Join(",", result.Attacking.Select(x => x.damageString)) + "," +
                                string.Join(",", result.Defending.Select(x => x.damageString));
            int hp = result.Attacking[0].defender.stats.hp;
            int speed = result.Attacking[0].defender.stats.spe;
            string probValue = string.Empty;
            if (includeProbabilities)
            {
                probValue = result.Probability + ",";
            }
            
            // Quick fix for Argenta
            string rankString = result.Rank == 99 ? "A" : result.Rank.ToString();
            csvContent.AppendLine(
                $"{rankString},{probValue} {result.OppLevel}, {result.OppIvs}, {hp}, {speed}, {damageLine}");
        }

        csvContent.AppendLine();
    }

    private (int count, int setSize) CalcBaseChance(int rank, PokemonSet set, PokemonSet userSet, string selectedType)
    {
        // proof of concept hacky code

        int[][] groupRanks =
        [
            [1, 5],
            [3, 8],
            [6, 10],
            [9, 10]
        ];

        // get the groups based on rank
        List<int> indices = groupRanks
            .Select((range, index) => new { range, index })
            .Where(x => rank >= x.range[0] && rank <= x.range[1])
            // add one for group
            .Select(x => x.index + 1)
            .ToList();


        IOrderedEnumerable<KeyValuePair<string, PokemonSet>> groupMonsE = _sets!
            .Where(x => indices.Any(i => x.Value.Group.Equals(i)))
            .OrderBy(x => x.Value.Id);

        List<KeyValuePair<string, PokemonSet>> groupMons = groupMonsE.ToList();
        KeyValuePair<string, PokemonSet> firstInGroup = groupMons!.First();
        KeyValuePair<string, PokemonSet> lastInGroup = groupMons!.Last();
        int firstGroupId = firstInGroup.Value.Id;
        int lastGroupId = lastInGroup.Value.Id;

        IEnumerable<KeyValuePair<string, PokemonSet>> typeMonsE =
            groupMons.Where(x => x.Value.Types.Contains(selectedType));
        List<KeyValuePair<string, PokemonSet>> typeMons = typeMonsE.ToList();

        // need to handle the first, and last differently, maybe also second last if mon is last
        int endTypeId = typeMons.Last().Value.Id;
        bool isEndMon = endTypeId == lastGroupId && lastGroupId == set.Id;
        int groupCount = groupMons.Count;

        if (isEndMon)
        {
            return (1, groupCount);
        }

        int index = typeMons.FindIndex(x => x.Value.Id.Equals(set.Id));
        int diff = 0;

        // wrap the index if needed...
        int beforeIdx = index - 1;

        // skip user mon
        if (typeMons[(beforeIdx + typeMons.Count) % typeMons.Count].Value.Id == userSet.Id)
        {
            beforeIdx -= 1;
        }

        // naive but logical
        if (beforeIdx < 0)
        {
            int wrappedId = typeMons[(beforeIdx + typeMons.Count) % typeMons.Count].Value.Id;

            if (wrappedId == lastGroupId)
            {
                // if the type of the mon is the same as last idx, remove the additional chance of it but
                // don't break the rollover count
                if (endTypeId == lastGroupId)
                {
                    diff -= 1;
                }

                beforeIdx -= 1;
            }

            int beforeId = typeMons[(beforeIdx + typeMons.Count) % typeMons.Count].Value.Id;
            diff += lastGroupId - beforeId;
            diff += set.Id - firstGroupId + 1;
        }
        else
        {
            diff += set.Id - typeMons[beforeIdx].Value.Id;
        }

        return (diff, groupCount);
    }
}