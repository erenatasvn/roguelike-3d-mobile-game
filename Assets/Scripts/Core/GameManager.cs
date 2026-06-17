using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Başlangıç Ayarları (Inspector'dan Ayarla)")]
    public int startingRoom = 1;

    // Kacinci odada oldugumuz
    public static int currentRoom = 1;

    // Bir onceki odanin dusman sayisini tutar (hic azalmasin diye)
    public static int lastRoomEnemyCount = 0;

    // Sadece bu run'da toplanan altin
    public static int sessionGold = 0;

    // Sadece bu run'da verilen hasar 
    public static long sessionDamageDealt = 0;

    // Pause ve Death screen istatistikleri
    public static float sessionTimeSpent = 0f;
    public static int sessionEnemiesKilled = 0;
    public static int sessionBossesKilled = 0;

    public int enemyCount;
    public GameObject nextStagePortal;

    // EnemySpawner dusman sayisini set etmeden once gelen sinyalleri bloklar
    private bool isInitialized = false;

    void Awake()
    {
        // Start calismadan önce değerler hazırlanır
        if (currentRoom < startingRoom)
        {
            currentRoom = startingRoom;
            sessionGold = 0; 
            sessionDamageDealt = 0;
            sessionTimeSpent = 0f;
            sessionEnemiesKilled = 0;
            sessionBossesKilled = 0;
        }
    }

    void Start()
    {
        // Eğer portal Inspector'dan atanmamışsa sahneden otomatik bul
        if (nextStagePortal == null)
        {
            PortalController pc = FindAnyObjectByType<PortalController>();
            if (pc != null) nextStagePortal = pc.gameObject;
        }

        // Oyunun basinda portali kapat
        if (nextStagePortal != null)
        {
            nextStagePortal.SetActive(false);
        }
    }

    void Update()
    {
        // Zamanı sadece oyun devam ediyorsa (TimeScale > 0) say
        sessionTimeSpent += Time.deltaTime;
    }

    // EnemySpawner bu metodu cagirarak dusman sayisini verir ve sistemi hazir isaret eder
    public void InitializeEnemyCount(int count)
    {
        enemyCount = count;
        isInitialized = true;
    }

    public void OnEnemyDied()
    {
        // EnemySpawner Start'i calismadiys) yoksay
        if (!isInitialized) return;

        enemyCount--;
        sessionEnemiesKilled++; // Istatistigi artir

        if (enemyCount <= 0)
        {
            StageCleared();
        }
    }

    void StageCleared()
    {
        Debug.Log("TEBRIKLER! ODA TEMIZLENDI! Portal Aciliyor...");

        // Lifetime istatistiklerini guncelle
        SaveData save = SaveSystem.Load();
        save.totalRoomsCleared++;
        save.totalDamageDealt += sessionDamageDealt;
        sessionDamageDealt = 0;
        QuestSystem.CheckAndUpdate(save);
        SaveSystem.Save(save);

        if (currentRoom >= 40)
        {
            DungeonCleared(save);
        }
        else
        {
            // Eğer portal bulunamadıysa bir kez daha ara
            if (nextStagePortal == null)
            {
                PortalController pc = FindAnyObjectByType<PortalController>(FindObjectsInactive.Include);
                if (pc != null) nextStagePortal = pc.gameObject;
            }

            // Portal objesini aktif hale getir
            if (nextStagePortal != null)
            {
                nextStagePortal.SetActive(true);
            }
            else
            {
                Debug.LogError("GameManager: nextStagePortal bulunamadı! Oda tamamlandı ama portal açılamıyor.");
            }
        }
    }

    void DungeonCleared(SaveData save)
    {
        Debug.Log("ZINDAN TAMAMLANDI!");
        
        // Eğer bu oynadığımız zindan en yüksek seviyeli zindansa, limiti artır
        if (save.currentDungeon > save.dungeonsCleared)
        {
            save.dungeonsCleared = save.currentDungeon;
            SaveSystem.Save(save);
        }

        EndRunAndSaveGold();
        Invoke(nameof(ReturnToMainMenu), 3f);
    }

    void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public static void EndRunAndSaveGold()
    {
        SaveData save = SaveSystem.Load();
        if (sessionGold > 0)
        {
            save.totalGold += sessionGold;
            sessionGold = 0;
        }
        // Kalan hasar birikimini de kaydet
        if (sessionDamageDealt > 0)
        {
            save.totalDamageDealt += sessionDamageDealt;
            sessionDamageDealt = 0;
        }

        // İlerlenen maksimum odayı kaydet
        int dIndex = save.currentDungeon - 1;
        if (dIndex >= 0)
        {
            while (save.maxRoomReachedPerDungeon.Count <= dIndex)
            {
                save.maxRoomReachedPerDungeon.Add(1);
            }
            if (currentRoom > save.maxRoomReachedPerDungeon[dIndex])
            {
                save.maxRoomReachedPerDungeon[dIndex] = currentRoom;
            }
        }

        QuestSystem.CheckAndUpdate(save);
        SaveSystem.Save(save);
    }
}