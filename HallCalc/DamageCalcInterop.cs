using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace HallCalc;

[SupportedOSPlatform("browser")]
public partial class DamageCalcInterop
{
    [JSImport("calculateDamage", "damage-calc.js")]
    internal static partial string CalculateDamage(string attackingPokemon, string  attackingPokemonOptions, string defendingPokemon, string defendingPokemonOptions, string moveName, string  field);
    
    [JSImport("showAlert", "jsinterop.js")]
    internal static partial void ShowAlert(string message);
    
    
}
