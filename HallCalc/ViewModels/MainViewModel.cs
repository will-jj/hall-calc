using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
    public async Task GetData()
    {
        using Stream stream = AssetLoader.Open(new Uri("avares://HallCalc/Assets/sets.json"));
        using StreamReader reader = new(stream);
        string json =  reader.ReadToEnd();
        var options = new JsonSerializerOptions();
        options.PropertyNameCaseInsensitive = true;
        var sets = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, PokemonSet>>>(json, options);
        var pachies = sets["Pachirisu"];
        var hallPachie = pachies["Pachirisu-H"];
        hallPachie.Level = 30;
        var meme = DamageCalcInterop.CalculateDamage("Squirtle", JsonSerializer.Serialize(hallPachie), "Bulbasaur", "","earthquake", "");
         var hello = JsonSerializer.Deserialize<CalcResult>(meme);
         await Task.Delay(1000);
         var meme2 = DamageCalcInterop.CalculateDamage("Squirtle", "", "Pidgey", "","earthquake", "");
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


