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
        var options = new JsonSerializerOptions();
        options.PropertyNameCaseInsensitive = true;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        var sets = JsonSerializer.Deserialize<SortedDictionary<string, PokemonSet>>(json, options);
        var targetType = "Fire";
        IEnumerable<KeyValuePair<string, PokemonSet>> fireSets = sets
            .Where(x => x.Value.Types.Contains("Fire"));
        var memeay = fireSets.OrderBy(x => x.Value.Identifier);
        
        //IEnumerable<KeyValuePair<string, PokemonSet>>  fireSets = sets.Where(x => x.Key == "Fire").Select<KeyValuePair<string, PokemonSet>, KeyValuePair<string, PokemonSet>>(x => x.Value);
        // Output the results

        PokemonSet pachies = sets["Pachirisu"];
        PokemonSet hallPachie = null;//pachies["Pachirisu-H"];
        hallPachie.Level = 30;
      
      var meme = DamageCalcInterop.CalculateDamage("Squirtle", JsonSerializer.Serialize(hallPachie), "Bulbasaur", "","earthquake", "");
      var meme2 =  DamageCalcInterop.CalculateDamage("Squirtle", JsonSerializer.Serialize(hallPachie), "Bulbasaur", "","earthquake", "");
      var meme3 =  DamageCalcInterop.CalculateDamage("Squirtle", JsonSerializer.Serialize(hallPachie), "Bulbasaur", "","earthquake", "");
    //     var hello = JsonSerializer.Deserialize<CalcResult>(meme);
        // await Task.Delay(100);
         var meme4 = DamageCalcInterop.CalculateDamage("Squirtle", "{}", "Pidgey", "","earthquake", "");
         try
         {
             var hello2 = JsonSerializer.Deserialize<CalcResult>(meme2);
         }
         catch(Exception e)
         {
             int stop = 1;
         }

    }
}


