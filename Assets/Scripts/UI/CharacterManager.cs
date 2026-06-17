using UnityEngine;
using TMPro;
using System.Linq; 

public class CharacterManager : MonoBehaviour
{
    [Header("Karakter Bilgileri")]
    public TextMeshProUGUI txt_Stats;
    public TextMeshProUGUI txt_EquippedWeapon;

    void OnEnable()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        // 3D Modeli guncelle (Eger silah degistirdiysek hemen yansisin)
        FindAnyObjectByType<CharacterPreview>()?.UpdateWeaponPreview();

        SaveData save = MainMenuManager.CurrentSave;
        if (save == null) return;

        // Stat hesaplamalari 
        float baseHealth = 100f;
        float baseDamage = 10f; 
        float baseMagnet = 2.5f;

        // Silah bonusunu bul
        float weaponBonus = 0f;
        string wName = save.equippedWeapon;
        foreach (var w in ShopManager.AllWeapons)
        {
            if (w.id == save.equippedWeapon)
            {
                weaponBonus = w.bonusPercent / 100f;
                wName = w.displayName;
                break;
            }
        }

        // Toplam Can
        float totalHealth = baseHealth * (1f + (save.permHealthLevel * 0.05f));
        
        // Toplam Hasar
        float totalDamage = baseDamage * (1f + weaponBonus) * (1f + (save.permDamageLevel * 0.05f));

        // Digerleri
        float goldBonus = 50f + (save.permGoldChanceLevel * 2f); // Sans yuzdesi olarak kalsin
        float totalMagnet = baseMagnet + (save.permMagnetLevel * 0.5f);

        if (txt_Stats != null)
        {
            txt_Stats.text = 
                $"<color=#ff5555>Toplam Can: {Mathf.RoundToInt(totalHealth)}</color>\n" +
                $"<color=#ffaa00>Toplam Hasar: {Mathf.RoundToInt(totalDamage)}</color>\n" +
                $"<color=#ffd700>Altin Sansi: %{goldBonus}</color>\n" +
                $"<color=#00ccff>Miknatis: {totalMagnet:0.0}m</color>";
        }

        if (txt_EquippedWeapon != null)
        {
            txt_EquippedWeapon.text = $"Kusanilan Silah: <b>{wName}</b>";
        }
    }
}
