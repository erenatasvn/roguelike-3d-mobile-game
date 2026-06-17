using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Temel Stat'lar
    public static float damageMultiplier = 1f;
    public static float attackSpeedMultiplier = 1f;

    // Kalıcı Geliştirmeler JSON'dan okunur
    public static float permDamageMultiplier = 1f; // Menu'den kalıcı hasar bonusu

    // Kritik Vuruş 
    public static float critChance = 0f;       // 0–1 arası oran
    public static float critMultiplier = 2f;   // kritik çarpanı (2 = 2 kat hasar)

    // XP / Seviye 
    public static float xpMultiplier = 1f;     // Smart yeteneği bu değeri artırır

    // Hayatta Kalma
    public static float dodgeChance = 0f;      // hasar kaçınma şansı (0–0.6)
    public static float lifeStealPerKill = 0f; // Bloodthirst: her öldürüşte iyileşme
    public static bool hasExtraLife = false;   // Extra Life var mı 
    public static bool extraLifeUsed = false;  // Extra Life kullanıldı mı

    // Ölüm ve yeniden deneme anında her şeyi sıfırla
    public static void ResetStats()
    {
        damageMultiplier = 1f;
        attackSpeedMultiplier = 1f;
        critChance = 0f;
        critMultiplier = 2f;
        xpMultiplier = 1f;
        dodgeChance = 0f;
        lifeStealPerKill = 0f;
        hasExtraLife = false;
        extraLifeUsed = false;
    }
}