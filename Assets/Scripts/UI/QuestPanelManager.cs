using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class QuestPanelManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel_Quests;       // Ana quest paneli
    public Transform  contentParent;      // ScrollView > Viewport > Content
    public GameObject questRowPrefab;     // Tek bir gorev satiri prefabi

    [Header("Filtre Butonlari (Opsiyonel)")]
    public Button btn_All;
    public Button btn_Claimable;

    [Header("Bildirim Noktasi")]
    public GameObject notifDot;           // Alinacak odul varsa kirmizi nokta

    [Header("Tier Ikonlari (Gorev Madalyalari)")]
    public Sprite iconBronze;
    public Sprite iconSilver;
    public Sprite iconGold;

    private string currentFilter = "ALL";

    void OnEnable()
    {
        RefreshPanel();
    }

    public void OpenPanel()
    {
        panel_Quests.SetActive(true);
        RefreshPanel();
    }

    public void ClosePanel()
    {
        panel_Quests.SetActive(false);
    }

    public void FilterAll()      { currentFilter = "ALL";      RefreshPanel(); }
    public void FilterClaimable(){ currentFilter = "CLAIMABLE"; RefreshPanel(); }

    void RefreshPanel()
    {
        SaveData save = MainMenuManager.CurrentSave;
        if (save == null) return;

        // Mevcut satirlari temizle
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Tüm görevleri kategoriye gore grupla ve satir olustur
        string[] prefixes = { "KILL", "GOLD", "ROOM", "BOSS", "DMG" };
        string[] catNames  = { "Savascı", "Hazinedar", "Kasif", "Boss Avcısı", "Katil Makine" };

        for (int c = 0; c < prefixes.Length; c++)
        {
            List<QuestStep> steps = QuestSystem.GetCategory(prefixes[c]);

            // Kategori basligini ekle
            AddCategoryHeader(catNames[c], prefixes[c], save);

            foreach (var step in steps)
            {
                bool completed = QuestSystem.IsCompleted(step, save);
                bool claimed   = QuestSystem.IsClaimed(step, save);

                if (currentFilter == "CLAIMABLE" && !(completed && !claimed)) continue;

                AddQuestRow(step, save, completed, claimed);
            }
        }

        // Bildirim noktasini guncelle
        if (notifDot != null)
            notifDot.SetActive(QuestSystem.HasUnclaimed(save));
    }

    void AddCategoryHeader(string name, string prefix, SaveData save)
    {
        if (questRowPrefab == null) return;

        GameObject header = Instantiate(questRowPrefab, contentParent);
        // Baslik satirlari icin sadece isim yazdir, gerisi gizle
        TextMeshProUGUI title = header.transform.Find("Txt_Title")?.GetComponent<TextMeshProUGUI>();
        if (title != null)
        {
            title.text = $"── {name} ──";
            title.fontSize = title.fontSize * 0.85f;
        }

        // Diger elemanlari gizle
        SetActive(header, "Txt_Desc",     false);
        SetActive(header, "Txt_Progress", false);
        SetActive(header, "Slider",       false);
        SetActive(header, "Btn_Claim",    false);
        SetActive(header, "Txt_Reward",   false);
        SetActive(header, "Img_Tier",     false);
    }

    void AddQuestRow(QuestStep step, SaveData save, bool completed, bool claimed)
    {
        if (questRowPrefab == null) return;

        GameObject row = Instantiate(questRowPrefab, contentParent);

        // Baslik
        TextMeshProUGUI title = row.transform.Find("Txt_Title")?.GetComponent<TextMeshProUGUI>();
        if (title != null) title.text = step.title;

        // Aciklama
        TextMeshProUGUI desc = row.transform.Find("Txt_Desc")?.GetComponent<TextMeshProUGUI>();
        if (desc != null) desc.text = step.description;

        // Ilerleme çubugu
        Slider slider = row.transform.Find("Slider")?.GetComponent<Slider>();
        long current = QuestSystem.GetCurrentValue(step, save);
        if (slider != null)
        {
            slider.maxValue = 1f;
            slider.value    = QuestSystem.GetProgress(step, save);
        }

        // Ilerleme yazisi
        TextMeshProUGUI prog = row.transform.Find("Txt_Progress")?.GetComponent<TextMeshProUGUI>();
        if (prog != null)
            prog.text = claimed ? "Tamamlandi!" : $"{FormatNumber(current)} / {FormatNumber(step.targetValue)}";

        // Odul yazisi
        TextMeshProUGUI reward = row.transform.Find("Txt_Reward")?.GetComponent<TextMeshProUGUI>();
        if (reward != null)
            reward.text = $"Odul: <color=#ffd700>{FormatNumber(step.rewardGold)} Altin</color>";

        // Tier rengi / ikonu 
        Image tierImg = row.transform.Find("Img_Tier")?.GetComponent<Image>();
        if (tierImg != null)
        {
            tierImg.sprite = step.tier switch
            {
                QuestTier.Bronze => iconBronze,
                QuestTier.Silver => iconSilver,
                QuestTier.Gold   => iconGold,
                _                => null
            };
            
            // Resimlerin kendi rengi gozuksun diye beyaza ceviriyoruz
            tierImg.color = Color.white;
        }

        // Al butonu
        Button claimBtn = row.transform.Find("Btn_Claim")?.GetComponent<Button>();
        if (claimBtn != null)
        {
            claimBtn.gameObject.SetActive(completed && !claimed);
            claimBtn.interactable = completed && !claimed;

            if (completed && !claimed)
            {
                string stepId = step.id;
                int gold = step.rewardGold;
                claimBtn.onClick.AddListener(() => ClaimReward(stepId, gold));
            }
        }

        // Tamamlanmis ve alinmissa satirı soluk goster
        if (claimed)
        {
            var cg = row.GetComponent<CanvasGroup>();
            if (cg == null) cg = row.AddComponent<CanvasGroup>();
            cg.alpha = 0.45f;
        }
    }

    void ClaimReward(string stepId, int gold)
    {
        SaveData save = MainMenuManager.CurrentSave;
        if (save == null) return;
        if (save.claimedQuests.Contains(stepId)) return;

        save.claimedQuests.Add(stepId);
        save.totalGold += gold;
        SaveSystem.Save(save);

        FindAnyObjectByType<MainMenuManager>()?.RefreshTopBar();
        RefreshPanel();
    }

    // Sayi formatla (10000 → 10K)
    string FormatNumber(long n)
    {
        if (n >= 1_000_000) return $"{n / 1_000_000f:0.#}M";
        if (n >= 1_000)     return $"{n / 1_000f:0.#}K";
        return n.ToString();
    }

    void SetActive(GameObject parent, string childName, bool active)
    {
        Transform t = parent.transform.Find(childName);
        if (t != null) t.gameObject.SetActive(active);
    }
}
