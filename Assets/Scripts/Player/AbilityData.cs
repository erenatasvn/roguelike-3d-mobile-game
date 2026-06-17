using UnityEngine;

public enum AbilityID
{
    // Stat Boosts
    AttackBoost, MinorAttackBoost,
    AttackSpeedBoost, MinorAttackSpeedBoost,
    HPBoost, MinorHPBoost,
    CritMaster, MinorCritMaster,
    Smart,

    // Silah
    Multishot,
    FrontArrowPlus1,
    Ricochet,
    Piercing,

    // Element
    Blaze, Poison, Freeze, OBSOLETE_Bolt, DarkTouch,

    // Çember
    PoisonCircle, FrostCircle, BlazingCircle, OBSOLETE_BoltCircle, ObsidianCircle,

    // Hayatta Kalma
    Bloodthirst, DodgeMaster, ExtraLife,

    SwordSweep,
}

public enum AbilityCategory { StatBoost, Arrow, Element, Circle, Survival }
public enum AbilityRarity { Common, Rare, Epic }

public enum ElementType { None, Blaze, Poison, Freeze, OBSOLETE_Bolt, Dark }

[CreateAssetMenu(fileName = "NewAbility", menuName = "Roguelike/Ability Data")]
public class AbilityData : ScriptableObject
{
    public AbilityID id;
    public string displayName;
    [TextArea(2, 4)] public string description;
    public AbilityCategory category;
    public AbilityRarity rarity;
    public int maxStacks = 1;       
    [Range(0.1f, 3f)] public float weight = 1f; // havuzdan çekilme ağırlığı
}
