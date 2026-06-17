using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Dükkan sekmesini yöneten script.
// Silahları listeler, satın alma işlemini yönetir.
public class ShopManager : MonoBehaviour
{
    [Header("Silah Satır Prefabı")]
    public GameObject weaponRowPrefab; 
    public Transform weaponListParent; 

    [Header("Mesaj")]
    public TextMeshProUGUI txt_Message;

    // Silah Veritabanı 
    public static readonly List<WeaponShopEntry> AllWeapons = new List<WeaponShopEntry>
    {
        // Baltalar (Axe)
        new WeaponShopEntry("Data_Axe_Kazma",      "Çiftçi Kazması",      "Balta", 0,    0),     // Ücretsiz (Giriş)
        new WeaponShopEntry("Data_Axe",            "Çelik Savaş Baltası", "Balta", 30,   1000),
        new WeaponShopEntry("Data_Axe_Double",     "Cellat Baltası",      "Balta", 60,   3000),
        new WeaponShopEntry("Data_Axe_LongDouble", "Titan'ın Öfkesi",     "Balta", 100,  8000),

        // Kılıçlar (Sword)
        new WeaponShopEntry("Data_Sword",   "Çırak Kılıcı",   "Kılıç", 0,    0),     // Ücretsiz (Giriş)
        new WeaponShopEntry("Data_Sword_1", "Şövalye Kılıcı", "Kılıç", 30,   1000),
        new WeaponShopEntry("Data_Sword_2", "Rüzgar Kesen",   "Kılıç", 60,   3000),
        new WeaponShopEntry("Data_Sword_3", "Excalibur",      "Kılıç", 100,  8000),

        // Asalar (Staff)
        new WeaponShopEntry("Data_Staff_1", "Çırak Asası",    "Asa", 0,    0),       // Ücretsiz (Giriş)
        new WeaponShopEntry("Data_Staff_2", "Ateş Asası",     "Asa", 30,   1000),
        new WeaponShopEntry("Data_Staff_3", "Buz Asası",      "Asa", 60,   3000),
        new WeaponShopEntry("Data_Staff_4", "Başbüyücü Asası","Asa", 100,  8000),
    };

    void OnEnable()
    {
        // Sekme her açıldığında listeyi yenile
        BuildWeaponList();
    }

    void BuildWeaponList()
    {
        SaveData save = MainMenuManager.CurrentSave;
        if (save == null) return; 

        // Önceki satırları temizle
        foreach (Transform child in weaponListParent)
            Destroy(child.gameObject);

        foreach (var entry in AllWeapons)
        {
            GameObject row = Instantiate(weaponRowPrefab, weaponListParent);
            
            // Satırdaki UI elemanlarını bul
            TextMeshProUGUI nameTxt   = row.transform.Find("Txt_Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI bonusTxt  = row.transform.Find("Txt_Bonus")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceTxt  = row.transform.Find("Txt_Price")?.GetComponent<TextMeshProUGUI>();
            Button actionBtn          = row.transform.Find("Btn_Action")?.GetComponent<Button>();
            TextMeshProUGUI btnTxt    = actionBtn?.GetComponentInChildren<TextMeshProUGUI>();

            if (nameTxt  != null) nameTxt.text  = entry.displayName;
            if (bonusTxt != null) bonusTxt.text = entry.bonusPercent > 0 
                                                    ? $"+%{entry.bonusPercent} Hasar" 
                                                    : "Temel";

            bool owned    = save.OwnsWeapon(entry.id);
            bool equipped = save.equippedWeapon == entry.id;

            if (equipped)
            {
                if (btnTxt  != null) btnTxt.text   = "Kusanildi";
                if (actionBtn != null) actionBtn.interactable = false;
                if (priceTxt != null) priceTxt.text = "";
            }
            else if (owned)
            {
                if (priceTxt  != null) priceTxt.text  = "";
                if (btnTxt    != null) btnTxt.text     = "Kuşan";
                if (actionBtn != null)
                {
                    string weaponId = entry.id;
                    actionBtn.onClick.AddListener(() => EquipWeapon(weaponId));
                }
            }
            else
            {
                if (priceTxt  != null) priceTxt.text  = $"Fiyat: {entry.price}";
                if (btnTxt    != null) btnTxt.text     = "Satin Al";
                bool canAfford = save.totalGold >= entry.price;
                if (actionBtn != null)
                {
                    actionBtn.interactable = canAfford;
                    string weaponId = entry.id;
                    int price = entry.price;
                    actionBtn.onClick.AddListener(() => BuyWeapon(weaponId, price));
                }
            }
        }
    }

    void BuyWeapon(string weaponId, int price)
    {
        SaveData save = MainMenuManager.CurrentSave;
        if (save == null) return;

        if (save.totalGold < price)
        {
            ShowMessage("Yeterli altin yok!");
            return;
        }

        save.totalGold -= price;
        save.ownedWeapons.Add(weaponId);
        SaveSystem.Save(save);

        ShowMessage("Satin alindi!");
        FindAnyObjectByType<MainMenuManager>()?.RefreshTopBar();
        BuildWeaponList(); // Listeyi yenile
    }

    void EquipWeapon(string weaponId)
    {
        SaveData save = MainMenuManager.CurrentSave;
        if (save == null) return;

        save.equippedWeapon = weaponId;
        SaveSystem.Save(save);

        ShowMessage("Silah degistirildi!");
        BuildWeaponList();
        
        // Silah degisir degismez 3D modeli de aninda guncelle
        CharacterPreview preview = FindAnyObjectByType<CharacterPreview>();
        if (preview != null)
        {
            preview.UpdateWeaponPreview();
        }
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

//Silah Veri Modeli 
[System.Serializable]
public class WeaponShopEntry
{
    public string id;
    public string displayName;
    public string weaponType;
    public int    bonusPercent;
    public int    price;

    public WeaponShopEntry(string id, string displayName, string weaponType, int bonusPercent, int price)
    {
        this.id           = id;
        this.displayName  = displayName;
        this.weaponType   = weaponType;
        this.bonusPercent = bonusPercent;
        this.price        = price;
    }
}
