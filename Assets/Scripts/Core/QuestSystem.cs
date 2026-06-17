using System.Collections.Generic;
using UnityEngine;

public enum QuestCategory { Kill, Gold, Rooms, Boss, Damage }
public enum QuestTier     { Bronze = 0, Silver = 1, Gold = 2 }

[System.Serializable]
public class QuestStep
{
    public string    id;
    public string    title;
    public string    description;
    public long      targetValue;
    public int       rewardGold;
    public QuestTier tier;
    public int       stepInTier; // 1, 2, 3
}

public static class QuestSystem
{
    public static readonly List<QuestStep> AllSteps = new List<QuestStep>
    {
        // KILL
        new QuestStep { id="KILL_B1", title="Savascinin Baslangici",  description="50 dusmaní yenilgiye ugrat.",    targetValue=50,   rewardGold=100,  tier=QuestTier.Bronze, stepInTier=1 },
        new QuestStep { id="KILL_B2", title="Deneyimli Savascı",      description="100 dusmanı yenilgiye ugrat.",   targetValue=100,  rewardGold=200,  tier=QuestTier.Bronze, stepInTier=2 },
        new QuestStep { id="KILL_B3", title="Buyume Belirtisi",        description="200 dusmanı yenilgiye ugrat.",   targetValue=200,  rewardGold=400,  tier=QuestTier.Bronze, stepInTier=3 },
        new QuestStep { id="KILL_S1", title="Sefer Savasci",           description="400 dusmanı yenilgiye ugrat.",   targetValue=400,  rewardGold=800,  tier=QuestTier.Silver, stepInTier=1 },
        new QuestStep { id="KILL_S2", title="Meydan Okuyan",           description="600 dusmanı yenilgiye ugrat.",   targetValue=600,  rewardGold=1200, tier=QuestTier.Silver, stepInTier=2 },
        new QuestStep { id="KILL_S3", title="Zindan Efsanesi",         description="800 dusmanı yenilgiye ugrat.",   targetValue=800,  rewardGold=1800, tier=QuestTier.Silver, stepInTier=3 },
        new QuestStep { id="KILL_G1", title="Altin Savascisi",         description="1000 dusmanı yenilgiye ugrat.",  targetValue=1000, rewardGold=2500, tier=QuestTier.Gold,   stepInTier=1 },
        new QuestStep { id="KILL_G2", title="Zindanin Efendisi",       description="1500 dusmanı yenilgiye ugrat.",  targetValue=1500, rewardGold=4000, tier=QuestTier.Gold,   stepInTier=2 },
        new QuestStep { id="KILL_G3", title="Yok Edici",               description="2000 dusmanı yenilgiye ugrat.",  targetValue=2000, rewardGold=6000, tier=QuestTier.Gold,   stepInTier=3 },

        // GOLD 
        new QuestStep { id="GOLD_B1", title="Ilk Servet",              description="Toplamda 50 altin kazan.",       targetValue=50,   rewardGold=50,   tier=QuestTier.Bronze, stepInTier=1 },
        new QuestStep { id="GOLD_B2", title="Kazanc Yolu",             description="Toplamda 100 altin kazan.",      targetValue=100,  rewardGold=100,  tier=QuestTier.Bronze, stepInTier=2 },
        new QuestStep { id="GOLD_B3", title="Amatör Tüccar",           description="Toplamda 200 altin kazan.",      targetValue=200,  rewardGold=200,  tier=QuestTier.Bronze, stepInTier=3 },
        new QuestStep { id="GOLD_S1", title="Ticaret Uzmani",          description="Toplamda 400 altin kazan.",      targetValue=400,  rewardGold=400,  tier=QuestTier.Silver, stepInTier=1 },
        new QuestStep { id="GOLD_S2", title="Zengin Kahraman",         description="Toplamda 600 altin kazan.",      targetValue=600,  rewardGold=600,  tier=QuestTier.Silver, stepInTier=2 },
        new QuestStep { id="GOLD_S3", title="Altin Biriktirici",       description="Toplamda 800 altin kazan.",      targetValue=800,  rewardGold=800,  tier=QuestTier.Silver, stepInTier=3 },
        new QuestStep { id="GOLD_G1", title="Altin Ustasi",            description="Toplamda 1000 altin kazan.",     targetValue=1000, rewardGold=1000, tier=QuestTier.Gold,   stepInTier=1 },
        new QuestStep { id="GOLD_G2", title="Hazine Efendisi",         description="Toplamda 1500 altin kazan.",     targetValue=1500, rewardGold=1500, tier=QuestTier.Gold,   stepInTier=2 },
        new QuestStep { id="GOLD_G3", title="Sonsuz Servet",           description="Toplamda 2000 altin kazan.",     targetValue=2000, rewardGold=2000, tier=QuestTier.Gold,   stepInTier=3 },

        // ROOMS  
        new QuestStep { id="ROOM_B1", title="Ilk Adim",                description="50 oda temizle.",                targetValue=50,   rewardGold=100,  tier=QuestTier.Bronze, stepInTier=1 },
        new QuestStep { id="ROOM_B2", title="Kalici Kahraman",         description="100 oda temizle.",               targetValue=100,  rewardGold=200,  tier=QuestTier.Bronze, stepInTier=2 },
        new QuestStep { id="ROOM_B3", title="Zindan Gezgini",          description="200 oda temizle.",               targetValue=200,  rewardGold=400,  tier=QuestTier.Bronze, stepInTier=3 },
        new QuestStep { id="ROOM_S1", title="Sefer Katilimcisi",       description="400 oda temizle.",               targetValue=400,  rewardGold=800,  tier=QuestTier.Silver, stepInTier=1 },
        new QuestStep { id="ROOM_S2", title="Zindan Uzmani",           description="600 oda temizle.",               targetValue=600,  rewardGold=1200, tier=QuestTier.Silver, stepInTier=2 },
        new QuestStep { id="ROOM_S3", title="Meslek Sahibi",           description="800 oda temizle.",               targetValue=800,  rewardGold=1800, tier=QuestTier.Silver, stepInTier=3 },
        new QuestStep { id="ROOM_G1", title="Zindan Hükümdarı",        description="1000 oda temizle.",              targetValue=1000, rewardGold=2500, tier=QuestTier.Gold,   stepInTier=1 },
        new QuestStep { id="ROOM_G2", title="Ölümsüz Kahraman",        description="1500 oda temizle.",              targetValue=1500, rewardGold=4000, tier=QuestTier.Gold,   stepInTier=2 },
        new QuestStep { id="ROOM_G3", title="Efsane",                  description="2000 oda temizle.",              targetValue=2000, rewardGold=6000, tier=QuestTier.Gold,   stepInTier=3 },

        // BOSS  
        new QuestStep { id="BOSS_B1", title="Ilk Büyük Zafer",         description="1 Boss'u yenilgiye ugrat.",      targetValue=1,    rewardGold=300,  tier=QuestTier.Bronze, stepInTier=1 },
        new QuestStep { id="BOSS_B2", title="Boss Avcisi",             description="3 Boss'u yenilgiye ugrat.",      targetValue=3,    rewardGold=600,  tier=QuestTier.Bronze, stepInTier=2 },
        new QuestStep { id="BOSS_B3", title="Korku Salmayan",          description="5 Boss'u yenilgiye ugrat.",      targetValue=5,    rewardGold=1000, tier=QuestTier.Bronze, stepInTier=3 },
        new QuestStep { id="BOSS_S1", title="Dev Avcisi",              description="10 Boss'u yenilgiye ugrat.",     targetValue=10,   rewardGold=2000, tier=QuestTier.Silver, stepInTier=1 },
        new QuestStep { id="BOSS_S2", title="Kabus Katili",            description="15 Boss'u yenilgiye ugrat.",     targetValue=15,   rewardGold=3000, tier=QuestTier.Silver, stepInTier=2 },
        new QuestStep { id="BOSS_S3", title="Korku Salan",             description="20 Boss'u yenilgiye ugrat.",     targetValue=20,   rewardGold=4500, tier=QuestTier.Silver, stepInTier=3 },
        new QuestStep { id="BOSS_G1", title="Efsanevi Katil",          description="30 Boss'u yenilgiye ugrat.",     targetValue=30,   rewardGold=7000, tier=QuestTier.Gold,   stepInTier=1 },
        new QuestStep { id="BOSS_G2", title="Tanri Katili",            description="50 Boss'u yenilgiye ugrat.",     targetValue=50,   rewardGold=12000,tier=QuestTier.Gold,   stepInTier=2 },
        new QuestStep { id="BOSS_G3", title="Bosslarin Basi",          description="100 Boss'u yenilgiye ugrat.",    targetValue=100,  rewardGold=20000,tier=QuestTier.Gold,   stepInTier=3 },

        // DAMAGE
        new QuestStep { id="DMG_B1",  title="Güç Gosterisi",           description="Toplamda 50 hasar ver.",         targetValue=50,   rewardGold=100,  tier=QuestTier.Bronze, stepInTier=1 },
        new QuestStep { id="DMG_B2",  title="Yikim Ustasi",            description="Toplamda 100 hasar ver.",        targetValue=100,  rewardGold=200,  tier=QuestTier.Bronze, stepInTier=2 },
        new QuestStep { id="DMG_B3",  title="Cehennem Silahı",         description="Toplamda 200 hasar ver.",        targetValue=200,  rewardGold=400,  tier=QuestTier.Bronze, stepInTier=3 },
        new QuestStep { id="DMG_S1",  title="Yikim Makinası",          description="Toplamda 400 hasar ver.",        targetValue=400,  rewardGold=800,  tier=QuestTier.Silver, stepInTier=1 },
        new QuestStep { id="DMG_S2",  title="Hasar Tanrısı",           description="Toplamda 600 hasar ver.",        targetValue=600,  rewardGold=1200, tier=QuestTier.Silver, stepInTier=2 },
        new QuestStep { id="DMG_S3",  title="Her Sey Yikilir",         description="Toplamda 800 hasar ver.",        targetValue=800,  rewardGold=1800, tier=QuestTier.Silver, stepInTier=3 },
        new QuestStep { id="DMG_G1",  title="Altin Yikici",            description="Toplamda 1000 hasar ver.",       targetValue=1000, rewardGold=2500, tier=QuestTier.Gold,   stepInTier=1 },
        new QuestStep { id="DMG_G2",  title="Dünya Yikici",            description="Toplamda 1500 hasar ver.",       targetValue=1500, rewardGold=4000, tier=QuestTier.Gold,   stepInTier=2 },
        new QuestStep { id="DMG_G3",  title="Silinmez Güç",            description="Toplamda 2000 hasar ver.",       targetValue=2000, rewardGold=6000, tier=QuestTier.Gold,   stepInTier=3 },
    };

    public static List<QuestStep> GetCategory(string prefix)
        => AllSteps.FindAll(s => s.id.StartsWith(prefix));

    public static float GetProgress(QuestStep step, SaveData save)
        => Mathf.Clamp01((float)GetCurrentValue(step, save) / step.targetValue);

    public static long GetCurrentValue(QuestStep step, SaveData save)
    {
        if (step.id.StartsWith("KILL")) return save.totalEnemiesKilled;
        if (step.id.StartsWith("GOLD")) return save.totalGoldEarned;
        if (step.id.StartsWith("ROOM")) return save.totalRoomsCleared;
        if (step.id.StartsWith("BOSS")) return save.totalBossesKilled;
        if (step.id.StartsWith("DMG"))  return save.totalDamageDealt;
        return 0;
    }

    public static bool IsCompleted(QuestStep step, SaveData save)
        => save.completedQuests.Contains(step.id);

    public static bool IsClaimed(QuestStep step, SaveData save)
        => save.claimedQuests.Contains(step.id);

    public static bool HasUnclaimed(SaveData save)
    {
        foreach (var step in AllSteps)
            if (IsCompleted(step, save) && !IsClaimed(step, save))
                return true;
        return false;
    }

    public static void CheckAndUpdate(SaveData save)
    {
        foreach (var step in AllSteps)
        {
            if (save.completedQuests.Contains(step.id)) continue;
            if (GetCurrentValue(step, save) >= step.targetValue)
                save.completedQuests.Add(step.id);
        }
    }
}
