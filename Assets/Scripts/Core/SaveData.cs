using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // Karakter 
    public string playerName = "";
    public bool isFirstLaunch = true;

    //  Ekonomi 
    public int totalGold = 0;

    //  Silahlar 
    //  ilk başta her silahtan 1 tane açık gelsin
    public List<string> ownedWeapons = new List<string> { "sword_1", "bow_1", "axe_1" };
    public string equippedWeapon = "sword_1";

    // Kalıcı Geliştirmeler 
    public int permDamageLevel    = 0; // Max 20 — her seviye +%5 hasar
    public int permHealthLevel    = 0; // Max 20 — her seviye +%5 can
    public int permGoldChanceLevel = 0; // Max 20 — her seviye +%2 altın düşme şansı
    public int permMagnetLevel    = 0; // Max 10 — her seviye +0.5m mıknatıs menzili

    // İlerleme 
    public int dungeonsCleared   = 0; // Kaç zindan temizlendi
    public int currentDungeon    = 1; // Şu an hangi zindandayız
    public List<int> maxRoomReachedPerDungeon = new List<int> { 1 }; // Her zindan için ulaşılan en yüksek oda

    // Görev İstatistikleri 
    public long totalEnemiesKilled = 0;  // Toplam öldürülen düşman
    public long totalGoldEarned    = 0;  // Toplam kazanılan altın 
    public long totalRoomsCleared  = 0;  // Toplam temizlenen oda
    public long totalBossesKilled  = 0;  // Toplam öldürülen boss
    public long totalDamageDealt   = 0;  // Toplam verilen hasar 

    // Tamamlanan Görevler 
    // Format: "KILL_B1" = Kill kategori Bronze 1 tamamlandı
    public System.Collections.Generic.List<string> completedQuests = new System.Collections.Generic.List<string>();
    public System.Collections.Generic.List<string> claimedQuests   = new System.Collections.Generic.List<string>();

    // Hesaplanan Değerler 
    public float GetDamageMultiplier()    => 1f + permDamageLevel    * 0.05f;
    public float GetHealthMultiplier()    => 1f + permHealthLevel    * 0.05f;
    public float GetGoldDropChance()      => 0.5f + permGoldChanceLevel * 0.02f;
    public float GetMagnetRadius()        => 2.5f + permMagnetLevel  * 0.5f;

    // Silah Yardımcıları
    public bool OwnsWeapon(string weaponID) => ownedWeapons.Contains(weaponID);

    public string GetEquippedWeaponType()
    {
        if (equippedWeapon.StartsWith("sword")) return "sword";
        if (equippedWeapon.StartsWith("bow"))   return "bow";
        if (equippedWeapon.StartsWith("axe"))   return "axe";
        return "sword";
    }

    public int GetEquippedWeaponTier()
    {
        if (equippedWeapon.Length == 0) return 1;
        char last = equippedWeapon[equippedWeapon.Length - 1];
        if (int.TryParse(last.ToString(), out int tier)) return tier;
        return 1;
    }

    // Geliştirme Fiyatları
    public int GetUpgradeCost(int currentLevel, int baseCost = 100)
    {
        // Her seviyede fiyat %20 artar
        return (int)System.Math.Round(baseCost * System.Math.Pow(1.2, currentLevel));
    }
}
