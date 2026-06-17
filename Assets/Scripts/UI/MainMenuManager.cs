using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Ana menünün genel yöneticisi.
// Sekme geçişlerini, üst bar güncellemesini ve ilk açılış ekranını yönetir.
public class MainMenuManager : MonoBehaviour
{
    [Header("Üst Bar")]
    public TextMeshProUGUI txt_PlayerName;
    public TextMeshProUGUI txt_Gold;

    [Header("İçerik Panelleri")]
    public GameObject panel_Shop;
    public GameObject panel_Character;
    public GameObject panel_Dungeon;
    public GameObject panel_Upgrades;
    public GameObject panel_FirstLaunch;

    [Header("İlk Açılış")]
    public TMP_InputField inputField_Name;
    public Button btn_Confirm;

    [Header("Bildirimler")]
    public GameObject notifDot_Quests;

    [Header("Zindan Seçimi")]
    public TextMeshProUGUI txt_DungeonName;
    public TextMeshProUGUI txt_Progress;
    public Button btn_EnterDungeon;
    private int selectedDungeon = 1;

    // Statik olarak tüm kodlardan erişilebilir güncel kayıt verisi
    public static SaveData CurrentSave { get; private set; }

    void Start()
    {
        // JSON'dan veriyi yükle
        CurrentSave = SaveSystem.Load();

        if (CurrentSave.isFirstLaunch)
        {
            // Hiç oynanmamış — İsim ekranını göster
            ShowFirstLaunch();
        }
        else
        {
            // Kayıtlı oyuncu — Direkt Zindan sekmesini aç
            ShowPanel(panel_Dungeon);
            selectedDungeon = CurrentSave.currentDungeon;
            RefreshTopBar();
            RefreshDungeonUI();
        }

        // Görev bildirimini kontrol et
        if (notifDot_Quests != null)
        {
            notifDot_Quests.SetActive(QuestSystem.HasUnclaimed(CurrentSave));
        }

        // Onayla butonunu dinle
        if (btn_Confirm != null)
            btn_Confirm.onClick.AddListener(OnConfirmName);
    }

    // İlk Açılış 
    void ShowFirstLaunch()
    {
        panel_FirstLaunch.SetActive(true);
        panel_Shop.SetActive(false);
        panel_Character.SetActive(false);
        panel_Dungeon.SetActive(false);
        panel_Upgrades.SetActive(false);
    }

    public void OnConfirmName()
    {
        string enteredName = inputField_Name?.text.Trim();
        if (string.IsNullOrEmpty(enteredName))
            enteredName = "Kahraman";

        CurrentSave.playerName = enteredName;
        CurrentSave.isFirstLaunch = false;
        SaveSystem.Save(CurrentSave);

        panel_FirstLaunch.SetActive(false);
        ShowPanel(panel_Dungeon);
        RefreshTopBar();
    }

    // Sekme Geçişleri 
    public void OnTabShop()      => ShowPanel(panel_Shop);
    public void OnTabCharacter() => ShowPanel(panel_Character);
    public void OnTabDungeon()   
    {
        ShowPanel(panel_Dungeon);
        RefreshDungeonUI();
    }
    public void OnTabUpgrades()  => ShowPanel(panel_Upgrades);

    void ShowPanel(GameObject target)
    {
        panel_Shop.SetActive(panel_Shop == target);
        panel_Character.SetActive(panel_Character == target);
        panel_Dungeon.SetActive(panel_Dungeon == target);
        panel_Upgrades.SetActive(panel_Upgrades == target);
    }

    // Üst Bar 
    public void RefreshTopBar()
    {
        if (txt_PlayerName != null)
            txt_PlayerName.text = CurrentSave.playerName;

        if (txt_Gold != null)
            txt_Gold.text = CurrentSave.totalGold.ToString();
    }

    // Oyun bittikten sonra altını güncelle
    void OnEnable()
    {
        // Eğer sahne yeniden yüklendiyse güncel altını göster
        if (CurrentSave != null) 
        {
            selectedDungeon = CurrentSave.currentDungeon;
            RefreshTopBar();
            RefreshDungeonUI();
            if (notifDot_Quests != null)
                notifDot_Quests.SetActive(QuestSystem.HasUnclaimed(CurrentSave));
        }
    }

    // Zindan Seçimi 
    public void NextDungeon()
    {
        // En fazla kilidi açılmış zindanlar + 1 (yani sıradaki kilitli zindan) görülebilir.
        if (selectedDungeon <= CurrentSave.dungeonsCleared + 1)
        {
            selectedDungeon++;
            CurrentSave.currentDungeon = selectedDungeon;
            SaveSystem.Save(CurrentSave);
            RefreshDungeonUI();
        }
    }

    public void PrevDungeon()
    {
        if (selectedDungeon > 1)
        {
            selectedDungeon--;
            CurrentSave.currentDungeon = selectedDungeon;
            SaveSystem.Save(CurrentSave);
            RefreshDungeonUI();
        }
    }

    void RefreshDungeonUI()
    {
        if (txt_DungeonName != null)
        {
            txt_DungeonName.text = selectedDungeon + ". Zindan";
        }

        bool isUnlocked = selectedDungeon <= CurrentSave.dungeonsCleared + 1;

        if (btn_EnterDungeon != null)
        {
            btn_EnterDungeon.interactable = isUnlocked;
            TextMeshProUGUI btnText = btn_EnterDungeon.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = isUnlocked ? "Zindana Gir" : "KİLİTLİ";
            }
        }

        if (txt_Progress != null)
        {
            if (!isUnlocked)
            {
                txt_Progress.text = "Önceki Zindanı Tamamla!";
            }
            else
            {
                int maxRoom = 1;
                int dIndex = selectedDungeon - 1;
                if (CurrentSave.maxRoomReachedPerDungeon != null && dIndex >= 0 && dIndex < CurrentSave.maxRoomReachedPerDungeon.Count)
                {
                    maxRoom = CurrentSave.maxRoomReachedPerDungeon[dIndex];
                }
                
                if (CurrentSave.dungeonsCleared >= selectedDungeon)
                {
                    txt_Progress.text = "Zindan Tamamlandı (40/40)";
                }
                else
                {
                    if (maxRoom <= 1)
                    {
                        txt_Progress.text = selectedDungeon + ". Zindanı Tamamla!";
                    }
                    else
                    {
                        txt_Progress.text = "İlerlenen Oda: " + maxRoom + " / 40";
                    }
                }
            }
        }
    }

    // Zindana Giriş
    public void EnterDungeon()
    {
        SaveSystem.Save(CurrentSave);
        GameManager.currentRoom = 1; // Zindana baştan girildiğinde sayacı sıfırla
        InGameUIManager.hasTimerRun = false; // Sayacın çalışmasını tetikle
        SceneManager.LoadScene("SampleScene");
    }
}
