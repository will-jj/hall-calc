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
        Dictionary<string, PokemonSet>? sets = JsonSerializer.Deserialize<Dictionary<string, PokemonSet>>(json, options);
        const string targetType = "Fire";
        IEnumerable<KeyValuePair<string, PokemonSet>> fireSets = sets!
            .Where(x => x.Value.Types.Contains(targetType));
        IOrderedEnumerable<KeyValuePair<string, PokemonSet>> memeay = fireSets.OrderBy(x => x.Value.Id);
        foreach ((string hi, PokemonSet yo) in memeay)
        {
            Console.WriteLine(hi);
            Console.WriteLine(yo.Id);
        }
        
        PokemonSet hallPachie = sets!["Pachirisu"];
        hallPachie.Level = 30;
        if (!OperatingSystem.IsBrowser())
        {
            return;
        }
        string meme = DamageCalcInterop.CalculateDamage("Squirtle",
            JsonSerializer.Serialize(hallPachie),
            "Bulbasaur",
            "",
            "earthquake",
            "");

    }
}


