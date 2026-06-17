using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    // Aktif yetenekler: AbilityID → sahip olunan yığın sayısı
    private Dictionary<AbilityID, int> stacks = new Dictionary<AbilityID, int>();

    // Circle prefabları
    public GameObject poisonCirclePrefab;
    public GameObject frostCirclePrefab;
    public GameObject blazingCirclePrefab;
    public GameObject obsidianCirclePrefab;

    private List<GameObject> activeCircles = new List<GameObject>();
    private Transform playerTransform;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Odalar arası geçişte yok et
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // Yeni sahne yüklenince çağrılır circleleri yeniden spawn et
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        RespawnCircles();
    }

    // Stacks'te olan circle yeteneklerini sahnede yeniden oluşturur
    void RespawnCircles()
    {
        foreach (var c in activeCircles) if (c != null) Destroy(c);
        activeCircles.Clear();

        int typeIndex = 0;
        if (HasAbility(AbilityID.PoisonCircle))   SpawnCircle(AbilityID.PoisonCircle, poisonCirclePrefab, ElementType.Poison, typeIndex++);
        if (HasAbility(AbilityID.FrostCircle))    SpawnCircle(AbilityID.FrostCircle, frostCirclePrefab, ElementType.Freeze, typeIndex++);
        if (HasAbility(AbilityID.BlazingCircle))  SpawnCircle(AbilityID.BlazingCircle, blazingCirclePrefab, ElementType.Blaze, typeIndex++);
        if (HasAbility(AbilityID.ObsidianCircle)) SpawnCircle(AbilityID.ObsidianCircle, obsidianCirclePrefab, ElementType.Dark, typeIndex++);
    }

    public bool IsCircleAbility(AbilityID id)
    {
        return id == AbilityID.PoisonCircle || id == AbilityID.FrostCircle || 
               id == AbilityID.BlazingCircle || id == AbilityID.ObsidianCircle;
    }

    public int GetActiveCircleTypesCount()
    {
        int count = 0;
        if (HasAbility(AbilityID.PoisonCircle)) count++;
        if (HasAbility(AbilityID.FrostCircle)) count++;
        if (HasAbility(AbilityID.BlazingCircle)) count++;
        if (HasAbility(AbilityID.ObsidianCircle)) count++;
        return count;
    }

    public bool HasAbility(AbilityID id) => stacks.TryGetValue(id, out int v) && v > 0;
    public int GetStacks(AbilityID id) => stacks.TryGetValue(id, out int v) ? v : 0;

    // Herhangi bir elementin ulaştığı maksimum seviyeyi döndür
    public int GetElementStack(ElementType type)
    {
        int stack = 0;
        switch(type) {
            case ElementType.Poison: stack = Mathf.Max(GetStacks(AbilityID.Poison), GetStacks(AbilityID.PoisonCircle)); break;
            case ElementType.Freeze: stack = Mathf.Max(GetStacks(AbilityID.Freeze), GetStacks(AbilityID.FrostCircle)); break;
            case ElementType.Blaze:  stack = Mathf.Max(GetStacks(AbilityID.Blaze), GetStacks(AbilityID.BlazingCircle)); break;
            case ElementType.Dark:   stack = Mathf.Max(GetStacks(AbilityID.DarkTouch), GetStacks(AbilityID.ObsidianCircle)); break;
        }
        return Mathf.Max(1, stack);
    }

    // LevelUpManager bu metodu çağırır
    public void Apply(AbilityData data)
    {
        if (!stacks.ContainsKey(data.id)) stacks[data.id] = 0;
        if (data.maxStacks > 0 && stacks[data.id] >= data.maxStacks) return;
        stacks[data.id]++;
        ExecuteEffect(data.id);
    }

    void ExecuteEffect(AbilityID id)
    {
        switch (id)
        {
            // Stat Boosts
            case AbilityID.AttackBoost:           PlayerStats.damageMultiplier += 0.25f; break;
            case AbilityID.MinorAttackBoost:      PlayerStats.damageMultiplier += 0.10f; break;
            case AbilityID.AttackSpeedBoost:      PlayerStats.attackSpeedMultiplier += 0.20f; break;
            case AbilityID.MinorAttackSpeedBoost: PlayerStats.attackSpeedMultiplier += 0.10f; break;

            case AbilityID.HPBoost:
                FindAnyObjectByType<PlayerHealth>()?.IncreaseMaxHealth(30f); break;
            case AbilityID.MinorHPBoost:
                FindAnyObjectByType<PlayerHealth>()?.IncreaseMaxHealth(15f); break;

            case AbilityID.CritMaster:
                PlayerStats.critChance = Mathf.Min(PlayerStats.critChance + 0.10f, 0.80f);
                PlayerStats.critMultiplier += 0.30f;
                break;
            case AbilityID.MinorCritMaster:
                PlayerStats.critChance = Mathf.Min(PlayerStats.critChance + 0.05f, 0.80f);
                break;

            case AbilityID.Smart:
                PlayerStats.xpMultiplier += 0.25f; break;

            // Hayatta Kalma
            case AbilityID.Bloodthirst:
                PlayerStats.lifeStealPerKill += 4f; break;

            case AbilityID.DodgeMaster:
                PlayerStats.dodgeChance = Mathf.Min(PlayerStats.dodgeChance + 0.20f, 0.60f); break;

            case AbilityID.ExtraLife:
                PlayerStats.hasExtraLife = true; break;

            // Silah Modifierlari
            case AbilityID.Multishot:
                PlayerStats.attackSpeedMultiplier *= 0.85f; // %15 hız cezası
                break;
            case AbilityID.FrontArrowPlus1:
                // Pasif — WeaponManager'a atandı
                break;
            case AbilityID.Piercing:
                // Pasif — Arrow.cs Start()'ta HasAbility() ile kontrol eder
                break;
            case AbilityID.Ricochet:
                // Pasif — Arrow.cs Start()'ta HasAbility() ile kontrol eder
                break;

            // Element Yetenekleri 
            // Pasif — WeaponManager.GetEquippedElement() her atışta kontrol eder
            case AbilityID.Blaze:
            case AbilityID.Poison:
            case AbilityID.Freeze:
            case AbilityID.DarkTouch:
                break;

            // Çemberler
            case AbilityID.PoisonCircle:
            case AbilityID.FrostCircle:
            case AbilityID.BlazingCircle:
            case AbilityID.ObsidianCircle:
                RespawnCircles();
                break;
        }
    }

    void SpawnCircle(AbilityID id, GameObject prefab, ElementType element, int typeIndex)
    {
        if (prefab == null || playerTransform == null) return;

        int stack = GetStacks(id);
        int circleCount = stack * 2; // Level 1: 2, Level 2: 4, Level 3: 6
        float damage = 10f;
        if (stack == 2) damage = 17.5f;
        if (stack >= 3) damage = 25f;

        float angleStep = 360f / circleCount;
        float offsetAngle = typeIndex * 45f; // Farklı çemberler iç içe geçmesin diye 45 derece kaydır

        for (int i = 0; i < circleCount; i++)
        {
            GameObject circle = Instantiate(prefab, playerTransform.position, Quaternion.identity);
            activeCircles.Add(circle);
            OrbitWeapon orb = circle.GetComponent<OrbitWeapon>();
            if (orb != null)
            {
                orb.elementType = element;
                orb.startAngle  = i * angleStep + offsetAngle; 
                orb.baseDamage = damage;
                orb.Initialize(playerTransform);
            }
        }
    }

    // Ölüm / yeniden deneme anında her şeyi sıfırla
    public void ResetAll()
    {
        stacks.Clear();
        foreach (var c in activeCircles) if (c != null) Destroy(c);
        activeCircles.Clear();
    }
}
