using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// Geliştirmeler sekmesini yöneten script.
// Kalıcı stat yükseltmelerini gösterir ve yönetir.
public class UpgradeManager : MonoBehaviour
{
    [Header("Hasar Geliştirmesi")]
    public TextMeshProUGUI txt_DamageLevel;
    public TextMeshProUGUI txt_DamageCost;
    public Button btn_DamageUpgrade;

    [Header("Can Geliştirmesi")]
    public TextMeshProUGUI txt_HealthLevel;
    public TextMeshProUGUI txt_HealthCost;
    public Button btn_HealthUpgrade;

    [Header("Altın Şansı Geliştirmesi")]
    public TextMeshProUGUI txt_GoldLevel;
    public TextMeshProUGUI txt_GoldCost;
    public Button btn_GoldUpgrade;

    [Header("Mıknatıs Menzili Geliştirmesi")]
    public TextMeshProUGUI txt_MagnetLevel;
    public TextMeshProUGUI txt_MagnetCost;
    public Button btn_MagnetUpgrade;

    [Header("Mesaj")]
    public TextMeshProUGUI txt_Message;

    private const int MAX_STAT_LEVEL   = 20;
    private const int MAX_MAGNET_LEVEL = 10;

    void OnEnable() => RefreshUI();

    void Start()
    {
        btn_DamageUpgrade?.onClick.AddListener(() => Upgrade("damage"));
        btn_HealthUpgrade?.onClick.AddListener(() => Upgrade("health"));
        btn_GoldUpgrade?.onClick.AddListener(  () => Upgrade("gold"));
        btn_MagnetUpgrade?.onClick.AddListener(() => Upgrade("magnet"));
    }


    void RefreshUI()
    {
        SaveData save = MainMenuManager.CurrentSave;
        if (save == null) return;

        int maxStat   = MAX_STAT_LEVEL;
        int maxMagnet = MAX_MAGNET_LEVEL;

        // Hasar
        int dmgCost = GetCost(save.permDamageLevel, 500);
        UpdateRow(txt_DamageLevel, txt_DamageCost, btn_DamageUpgrade,
                  save.permDamageLevel, maxStat, dmgCost, save.totalGold,
                  $"Hasar +%{save.permDamageLevel * 5}");

        // Can
        int hpCost = GetCost(save.permHealthLevel, 500);
        UpdateRow(txt_HealthLevel, txt_HealthCost, btn_HealthUpgrade,
                  save.permHealthLevel, maxStat, hpCost, save.totalGold,
                  $"Can +%{save.permHealthLevel * 5}");

        // Altın Şansı
        int goldCost = GetCost(save.permGoldChanceLevel, 1000);
        UpdateRow(txt_GoldLevel, txt_GoldCost, btn_GoldUpgrade,
                  save.permGoldChanceLevel, maxStat, goldCost, save.totalGold,
                  $"Altin Sansi +%{save.permGoldChanceLevel * 2}");

        // Mıknatıs
        int magCost = GetCost(save.permMagnetLevel, 1500);
        UpdateRow(txt_MagnetLevel, txt_MagnetCost, btn_MagnetUpgrade,
                  save.permMagnetLevel, maxMagnet, magCost, save.totalGold,
                  $"Miknatis +{save.permMagnetLevel * 0.5f:0.#}m");
    }

    void UpdateRow(TextMeshProUGUI levelTxt, TextMeshProUGUI costTxt, Button btn,
                   int currentLevel, int maxLevel, int cost, int playerGold, string effectText)
    {
        if (levelTxt != null)
            levelTxt.text = $"Seviye {currentLevel}/{maxLevel}\n<size=70%>{effectText}</size>";

        bool isMaxed = currentLevel >= maxLevel;

        if (costTxt != null)
            costTxt.text = isMaxed ? "MAX" : $"Fiyat: {cost}";

        if (btn != null)
        {
            btn.interactable = !isMaxed && playerGold >= cost;
        }
    }

    void Upgrade(string type)
    {
        SaveData save = MainMenuManager.CurrentSave;

        switch (type)
        {
            case "damage":
                if (save.permDamageLevel >= MAX_STAT_LEVEL) return;
                int dCost = GetCost(save.permDamageLevel, 500);
                if (save.totalGold < dCost) { ShowMessage("Yeterli altin yok!"); return; }
                save.totalGold -= dCost;
                save.permDamageLevel++;
                break;

            case "health":
                if (save.permHealthLevel >= MAX_STAT_LEVEL) return;
                int hCost = GetCost(save.permHealthLevel, 500);
                if (save.totalGold < hCost) { ShowMessage("Yeterli altin yok!"); return; }
                save.totalGold -= hCost;
                save.permHealthLevel++;
                break;

            case "gold":
                if (save.permGoldChanceLevel >= MAX_STAT_LEVEL) return;
                int gCost = GetCost(save.permGoldChanceLevel, 1000);
                if (save.totalGold < gCost) { ShowMessage("Yeterli altin yok!"); return; }
                save.totalGold -= gCost;
                save.permGoldChanceLevel++;
                break;

            case "magnet":
                if (save.permMagnetLevel >= MAX_MAGNET_LEVEL) return;
                int mCost = GetCost(save.permMagnetLevel, 1500);
                if (save.totalGold < mCost) { ShowMessage("Yeterli altin yok!"); return; }
                save.totalGold -= mCost;
                save.permMagnetLevel++;
                break;
        }

        SaveSystem.Save(save);
        FindAnyObjectByType<MainMenuManager>()?.RefreshTopBar();
        RefreshUI();
        ShowMessage("Guclendirildi!");
    }

    int GetCost(int currentLevel, int baseCost)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(1.2f, currentLevel));
    }

    void ShowMessage(string msg)
    {
        if (txt_Message != null)
        {
            txt_Message.text = msg;
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), 2f);
        }
    }

    void ClearMessage()
    {
        if (txt_Message != null) txt_Message.text = "";
    }
}
