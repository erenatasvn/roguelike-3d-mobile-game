using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpManager : MonoBehaviour
{
    public GameObject levelUpPanel;

    [Header("Yetenek Havuzu")]
    public List<AbilityData> abilityPool; // Inspector'dan tüm AbilityData SO'larını sürüklenir

    [Header("UI Butonları (3 adet)")]
    public Button[] abilityButtons;           // 3 buton
    public TextMeshProUGUI[] abilityNames;    // her butonun başlık metni
    public TextMeshProUGUI[] abilityDescs;    // her butonun açıklama metni

    private PlayerHealth playerHealth;
    private AbilityData[] currentChoices = new AbilityData[3];
    private int pendingLevelUps = 0;

    void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
    }

    public void QueueLevelUp()
    {
        pendingLevelUps++;
        if (pendingLevelUps == 1) ShowLevelUpScreen();
    }

    void ShowLevelUpScreen()
    {
        DrawRandomAbilities();

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    // Ağırlıklı rastgele 3 farklı yetenek seç
    void DrawRandomAbilities()
    {
        List<AbilityData> available = BuildAvailablePool();
        HashSet<int> picked = new HashSet<int>();

        for (int slot = 0; slot < 3; slot++)
        {
            if (available.Count == 0) break;

            float totalWeight = 0f;
            foreach (AbilityData a in available) totalWeight += a.weight;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            int chosenIndex = 0;

            for (int i = 0; i < available.Count; i++)
            {
                cumulative += available[i].weight;
                if (roll <= cumulative) { chosenIndex = i; break; }
            }

            currentChoices[slot] = available[chosenIndex];
            available.RemoveAt(chosenIndex);

            if (slot < abilityNames.Length && abilityNames[slot] != null)
            {
                var tmp = abilityNames[slot];

                tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
                tmp.overflowMode = TMPro.TextOverflowModes.Truncate;

                // Dinamik Seviye (Level) Gösterimi
                int currentLvl = AbilityManager.Instance != null ? AbilityManager.Instance.GetStacks(currentChoices[slot].id) : 0;
                int nextLvl = currentLvl + 1;
                string title = currentChoices[slot].displayName;
                if (currentChoices[slot].maxStacks > 1) title += $" (Lv.{nextLvl})";

                // Dinamik Etki Açıklaması Ekleme
                string desc = currentChoices[slot].description;
                switch(currentChoices[slot].id)
                {
                    case AbilityID.Freeze:
                    case AbilityID.FrostCircle:
                        float freezeTime = nextLvl == 1 ? 0.3f : (nextLvl == 2 ? 0.5f : 0.7f);
                        desc += $"\n<color=#00FF00>Süre: {freezeTime} saniye</color>";
                        break;
                    case AbilityID.Poison:
                    case AbilityID.PoisonCircle:
                        int poisonPercent = nextLvl == 1 ? 15 : (nextLvl == 2 ? 25 : 35);
                        desc += $"\n<color=#00FF00>Hasar: Silahtan %{poisonPercent}</color>";
                        break;
                    case AbilityID.Blaze:
                    case AbilityID.BlazingCircle:
                        int blazePercent = nextLvl == 1 ? 8 : (nextLvl == 2 ? 12 : 16);
                        desc += $"\n<color=#00FF00>Hasar: Silahtan %{blazePercent}</color>";
                        break;
                    case AbilityID.DarkTouch:
                    case AbilityID.ObsidianCircle:
                        float darkMult = 0.5f + (nextLvl * 0.5f);
                        desc += $"\n<color=#00FF00>Patlama Hasarı: {darkMult}x</color>";
                        break;
                }

                bool hasDesc = (slot < abilityDescs.Length && abilityDescs[slot] != null);
                if (hasDesc)
                {
                    tmp.text = title;
                    abilityDescs[slot].textWrappingMode = TMPro.TextWrappingModes.Normal;
                    abilityDescs[slot].overflowMode = TMPro.TextOverflowModes.Truncate;
                    abilityDescs[slot].text = desc;
                }
                else
                {
                    tmp.text = $"<b>{title}</b>\n{desc}";
                }
            }
        }
    }

    // Max stack'e ulaşmış ve mevcut silahla uyumsuz yetenekleri havuzdan çıkar
    List<AbilityData> BuildAvailablePool()
    {
        WeaponData.WeaponType wType = GetWeaponType();
        List<AbilityData> list = new List<AbilityData>();
        
        int activeCircleTypes = AbilityManager.Instance != null ? AbilityManager.Instance.GetActiveCircleTypesCount() : 0;

        foreach (AbilityData a in abilityPool)
        {
            if (a == null) continue;
            int current = AbilityManager.Instance?.GetStacks(a.id) ?? 0;
            if (a.maxStacks > 0 && current >= a.maxStacks) continue;

            // Ok/fırlatma yetenekleri kılıçta çıkmasın
            if (IsProjectileAbility(a.id) && wType == WeaponData.WeaponType.Sword) continue;

            // Piercing ve Ricochet SADECE Asa'da (Staff) çıksın (Balta ve Kılıçta çıkma)
            if ((a.id == AbilityID.Ricochet || a.id == AbilityID.Piercing) && wType != WeaponData.WeaponType.Staff) continue;

            // Kılıç özel yetenekleri yay/balta da çıkmasın
            if (a.id == AbilityID.SwordSweep && wType != WeaponData.WeaponType.Sword) continue;

            // En fazla 2 farklı çember tipine sahip olunabilir
            if (AbilityManager.Instance != null && AbilityManager.Instance.IsCircleAbility(a.id))
            {
                if (activeCircleTypes >= 2 && current == 0) continue; 
            }

            list.Add(a);
        }
        return list;
    }

    bool IsProjectileAbility(AbilityID id)
    {
        return id == AbilityID.Multishot    || id == AbilityID.FrontArrowPlus1 ||
               id == AbilityID.Ricochet    || id == AbilityID.Piercing;
    }

    WeaponData.WeaponType GetWeaponType()
    {
        WeaponManager wm = FindAnyObjectByType<WeaponManager>();
        if (wm != null && wm.currentWeapon != null) return wm.currentWeapon.type;
        return WeaponData.WeaponType.Sword; // bilinmiyorsa kılıç say
    }

    // Butonlar bu metodu çağırır: 0, 1 veya 2 index ile
    public void ChooseAbilityByIndex(int index)
    {
        if (index < 0 || index >= currentChoices.Length) return;
        AbilityData chosen = currentChoices[index];
        if (chosen == null) return;

        AbilityManager.Instance?.Apply(chosen);

        pendingLevelUps--;

        if (pendingLevelUps > 0)
        {
            ShowLevelUpScreen(); // sırada başka level-up var
        }
        else
        {
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}