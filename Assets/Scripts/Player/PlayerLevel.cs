using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevel : MonoBehaviour
{
    public static int currentLevel = 1;
    public static int currentXP = 0;
    public static int xpToNextLevel = 100;

    public Slider xpSlider;
    public TextMeshProUGUI levelText;
    private LevelUpManager levelUpManager;

    // Boss Bar Modu
    private bool isBossMode = false;
    private Image fillImage;
    private Color originalFillColor;
    private float currentBossHealth;
    private float maxBossHealth;

    void Start()
    {
        if (xpSlider == null)
        {
            GameObject barObj = GameObject.Find("XPSlider");
            if (barObj != null) xpSlider = barObj.GetComponent<Slider>();
        }

        levelUpManager = FindAnyObjectByType<LevelUpManager>();

        if (xpSlider != null)
        {
            Transform fillTransform = xpSlider.transform.Find("Fill Area/Fill");
            if (fillTransform != null)
            {
                fillImage = fillTransform.GetComponent<Image>();
                if (fillImage != null) originalFillColor = fillImage.color;
            }
        }

        UpdateXPBar();
    }

    void Update()
    {
        if (xpSlider != null)
        {
            if (isBossMode)
            {
                xpSlider.value = Mathf.Lerp(xpSlider.value, currentBossHealth, Time.deltaTime * 10f);
            }
            else
            {
                xpSlider.value = Mathf.Lerp(xpSlider.value, currentXP, Time.deltaTime * 5f);
            }
        }
    }

    public void AddXP(int amount)
    {
        // Smart yeteneği XP kazanımını artırır
        int boosted = Mathf.RoundToInt(amount * PlayerStats.xpMultiplier);
        currentXP += boosted;
        
        if (!isBossMode)
        {
            UpdateXPBar();
        }

        while (currentXP >= xpToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(95f * Mathf.Pow(currentLevel, 1.35f));
        UpdateXPBar();
        levelUpManager?.QueueLevelUp();
    }

    void UpdateXPBar()
    {
        if (isBossMode) return;

        if (xpSlider != null) xpSlider.maxValue = xpToNextLevel;
        if (levelText != null)
        {
            levelText.text = "Lv. " + currentLevel;
            levelText.color = Color.white;
        }
        if (fillImage != null) fillImage.color = originalFillColor;
    }

    public void SetBossMode(bool active, float maxHealth = 100f, string bossName = "BOSS")
    {
        isBossMode = active;
        if (active)
        {
            maxBossHealth = maxHealth;
            currentBossHealth = maxHealth;
            
            if (xpSlider != null) xpSlider.maxValue = maxHealth;
            if (levelText != null)
            {
                levelText.text = bossName;
                levelText.color = Color.red;
            }
            if (fillImage != null) fillImage.color = Color.red;
        }
        else
        {
            // Boss öldü, normal Level barna dön ve arkada birikmiş XP'leri göster
            UpdateXPBar();
        }
    }

    public void UpdateBossHealth(float health)
    {
        if (!isBossMode) return;
        currentBossHealth = health;
    }

    public void ResetLevel(int startingLevel)
    {
        currentLevel = 1;
        currentXP = 0;
        xpToNextLevel = 100;

        if (levelUpManager == null)
            levelUpManager = FindAnyObjectByType<LevelUpManager>();

        if (startingLevel > 1)
        {
            int levelsToGain = startingLevel - 1;
            currentLevel = startingLevel;
            xpToNextLevel = Mathf.RoundToInt(95f * Mathf.Pow(currentLevel, 1.35f));
            
            if (levelUpManager != null)
            {
                for (int i = 0; i < levelsToGain; i++)
                {
                    levelUpManager.QueueLevelUp();
                }
            }
        }
        
        UpdateXPBar();
    }
}