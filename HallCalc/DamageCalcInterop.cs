using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace HallCalc;

[SupportedOSPlatform("browser")]
public partial class DamageCalcInterop
{
    [JSImport("DamageCalcInterop.calculateDamage", "main.js")]
    internal static partial Task<string> CalculateDamage([JSMarshalAs<JSType.String>] string attackingPokemon, [JSMarshalAs<JSType.String>] string  attackingPokemonOptions, [JSMarshalAs<JSType.String>] string defendingPokemon, [JSMarshalAs<JSType.String>] string defendingPokemonOptions, [JSMarshalAs<JSType.String>] string moveName, [JSMarshalAs<JSType.String>] string  field);
    
    [JSImport("showAlert", "jsinterop.js")]
    internal static partial void ShowAlert(string message);
}
