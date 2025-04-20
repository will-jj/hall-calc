namespace HallCalc.Models.Results;

public class CalcResult
{
    public Gen gen { get; set; }
    public Attacker attacker { get; set; }
    public Defender defender { get; set; }
    public Move move { get; set; }
    public Field field { get; set; }
    public int[] damage { get; set; }
    public RawDesc rawDesc { get; set; }
}

public class Gen
{
    public int num { get; set; }
    public Abilities abilities { get; set; }
    public Items items { get; set; }
    public Moves moves { get; set; }
    public Species species { get; set; }
    public Types types { get; set; }
    public Natures natures { get; set; }
}

public class Abilities
{
    public int gen { get; set; }
}

public class Items
{
    public int gen { get; set; }
}

public class Moves
{
    public int gen { get; set; }
}

public class Species
{
    public int gen { get; set; }
}

public class Types
{
    public int gen { get; set; }
}

public class Natures
{
}

public class Attacker
{
    public Species1 species { get; set; }
    public Gen1 gen { get; set; }
    public string name { get; set; }
    public string[] types { get; set; }
    public int weightkg { get; set; }
    public int level { get; set; }
    public string gender { get; set; }
    public string ability { get; set; }
    public bool abilityOn { get; set; }
    public bool isDynamaxed { get; set; }
    public bool isSaltCure { get; set; }
    public string nature { get; set; }
    public Ivs ivs { get; set; }
    public Evs evs { get; set; }
    public Boosts boosts { get; set; }
    public RawStats rawStats { get; set; }
    public Stats stats { get; set; }
    public int originalCurHP { get; set; }
    public string status { get; set; }
    public int toxicCounter { get; set; }
    public object[] moves { get; set; }
}

public class Species1
{
    public string kind { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public BaseStats baseStats { get; set; }
    public string[] types { get; set; }
    public int weightkg { get; set; }
    public bool nfe { get; set; }
    public Abilities1 abilities { get; set; }
}

public class BaseStats
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Abilities1
{
    public string _ { get; set; }
}

public class Gen1
{
    public int num { get; set; }
    public Abilities2 abilities { get; set; }
    public Items1 items { get; set; }
    public Moves1 moves { get; set; }
    public Species2 species { get; set; }
    public Types1 types { get; set; }
    public Natures1 natures { get; set; }
}

public class Abilities2
{
    public int gen { get; set; }
}

public class Items1
{
    public int gen { get; set; }
}

public class Moves1
{
    public int gen { get; set; }
}

public class Species2
{
    public int gen { get; set; }
}

public class Types1
{
    public int gen { get; set; }
}

public class Natures1
{
}

public class Ivs
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Evs
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Boosts
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class RawStats
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Stats
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Defender
{
    public Species3 species { get; set; }
    public Gen2 gen { get; set; }
    public string name { get; set; }
    public string[] types { get; set; }
    public double weightkg { get; set; }
    public int level { get; set; }
    public string gender { get; set; }
    public string ability { get; set; }
    public bool abilityOn { get; set; }
    public bool isDynamaxed { get; set; }
    public bool isSaltCure { get; set; }
    public string nature { get; set; }
    public Ivs1 ivs { get; set; }
    public Evs1 evs { get; set; }
    public Boosts1 boosts { get; set; }
    public RawStats1 rawStats { get; set; }
    public Stats1 stats { get; set; }
    public int originalCurHP { get; set; }
    public string status { get; set; }
    public int toxicCounter { get; set; }
    public object[] moves { get; set; }
}

public class Species3
{
    public string kind { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public BaseStats1 baseStats { get; set; }
    public string[] types { get; set; }
    public double weightkg { get; set; }
    public bool nfe { get; set; }
    public Abilities3 abilities { get; set; }
}

public class BaseStats1
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Abilities3
{
    public string _ { get; set; }
}

public class Gen2
{
    public int num { get; set; }
    public Abilities4 abilities { get; set; }
    public Items2 items { get; set; }
    public Moves2 moves { get; set; }
    public Species4 species { get; set; }
    public Types2 types { get; set; }
    public Natures2 natures { get; set; }
}

public class Abilities4
{
    public int gen { get; set; }
}

public class Items2
{
    public int gen { get; set; }
}

public class Moves2
{
    public int gen { get; set; }
}

public class Species4
{
    public int gen { get; set; }
}

public class Types2
{
    public int gen { get; set; }
}

public class Natures2
{
}

public class Ivs1
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Evs1
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Boosts1
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class RawStats1
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Stats1
{
    public int hp { get; set; }
    public int atk { get; set; }
    public int def { get; set; }
    public int spa { get; set; }
    public int spd { get; set; }
    public int spe { get; set; }
}

public class Move
{
    public string originalName { get; set; }
    public int hits { get; set; }
    public Gen3 gen { get; set; }
    public string name { get; set; }
    public int bp { get; set; }
    public string type { get; set; }
    public string category { get; set; }
    public int timesUsed { get; set; }
    public string target { get; set; }
    public bool hasCrashDamage { get; set; }
    public bool mindBlownRecoil { get; set; }
    public bool struggleRecoil { get; set; }
    public bool isCrit { get; set; }
    public bool isStellarFirstUse { get; set; }
    public Flags flags { get; set; }
    public int priority { get; set; }
    public bool ignoreDefensive { get; set; }
    public bool breaksProtect { get; set; }
    public bool isZ { get; set; }
    public bool isMax { get; set; }
    public bool multiaccuracy { get; set; }
}

public class Gen3
{
    public int num { get; set; }
    public Abilities5 abilities { get; set; }
    public Items3 items { get; set; }
    public Moves3 moves { get; set; }
    public Species5 species { get; set; }
    public Types3 types { get; set; }
    public Natures3 natures { get; set; }
}

public class Abilities5
{
    public int gen { get; set; }
}

public class Items3
{
    public int gen { get; set; }
}

public class Moves3
{
    public int gen { get; set; }
}

public class Species5
{
    public int gen { get; set; }
}

public class Types3
{
    public int gen { get; set; }
}

public class Natures3
{
}

public class Flags
{
}

public class Field
{
    public string gameType { get; set; }
    public bool isMagicRoom { get; set; }
    public bool isWonderRoom { get; set; }
    public bool isGravity { get; set; }
    public bool isAuraBreak { get; set; }
    public bool isFairyAura { get; set; }
    public bool isDarkAura { get; set; }
    public bool isBeadsOfRuin { get; set; }
    public bool isSwordOfRuin { get; set; }
    public bool isTabletsOfRuin { get; set; }
    public bool isVesselOfRuin { get; set; }
    public AttackerSide attackerSide { get; set; }
    public DefenderSide defenderSide { get; set; }
}

public class AttackerSide
{
    public int spikes { get; set; }
    public bool steelsurge { get; set; }
    public bool vinelash { get; set; }
    public bool wildfire { get; set; }
    public bool cannonade { get; set; }
    public bool volcalith { get; set; }
    public bool isSR { get; set; }
    public bool isReflect { get; set; }
    public bool isLightScreen { get; set; }
    public bool isProtected { get; set; }
    public bool isSeeded { get; set; }
    public bool isForesight { get; set; }
    public bool isTailwind { get; set; }
    public bool isHelpingHand { get; set; }
    public bool isFlowerGift { get; set; }
    public bool isFriendGuard { get; set; }
    public bool isAuroraVeil { get; set; }
    public bool isBattery { get; set; }
    public bool isPowerSpot { get; set; }
    public bool isSteelySpirit { get; set; }
}

public class DefenderSide
{
    public int spikes { get; set; }
    public bool steelsurge { get; set; }
    public bool vinelash { get; set; }
    public bool wildfire { get; set; }
    public bool cannonade { get; set; }
    public bool volcalith { get; set; }
    public bool isSR { get; set; }
    public bool isReflect { get; set; }
    public bool isLightScreen { get; set; }
    public bool isProtected { get; set; }
    public bool isSeeded { get; set; }
    public bool isForesight { get; set; }
    public bool isTailwind { get; set; }
    public bool isHelpingHand { get; set; }
    public bool isFlowerGift { get; set; }
    public bool isFriendGuard { get; set; }
    public bool isAuroraVeil { get; set; }
    public bool isBattery { get; set; }
    public bool isPowerSpot { get; set; }
    public bool isSteelySpirit { get; set; }
}

public class RawDesc
{
    public string attackerName { get; set; }
    public string moveName { get; set; }
    public string defenderName { get; set; }
    public string HPEVs { get; set; }
    public string attackEVs { get; set; }
    public string defenseEVs { get; set; }
}