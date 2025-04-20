using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace HallCalc.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [RelayCommand]
    public async Task GetData()
    {
          DamageCalcInterop.ShowAlert("hi");
         //string meme =await  DamageCalcInterop.CalculateDamage("1", "2", "3", "4","5", "6");
         int stop = 1;
    }
}


