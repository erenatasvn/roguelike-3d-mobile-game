using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StageSpawnData
{
    public int roomNumber;
    public int normalCount;
    public int tankCount;
    public int runnerCount;
    public int archerCount;
    public bool isBossRoom;
}

[System.Serializable]
public class DungeonSpawnConfig
{
    public StageSpawnData[] stages;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Düşman Prefabları")]
    public GameObject enemyPrefab;        // Normal düşman
    public GameObject tankEnemyPrefab;    // Yavaş, yüksek canlı
    public GameObject runnerEnemyPrefab;  // Hızlı, zigzag
    public GameObject archerEnemyPrefab;  // Menzilli, ok atar

    [Header("Spawn Noktaları")]
    public Transform[] spawnPoints;

    [Header("Boss")]
    public GameObject[] bossPrefabs;
    public Transform bossSpawnPoint;

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        int currentRoom = GameManager.currentRoom;
        
        // JSON'dan odanın verisini okur
        StageSpawnData currentStageData = GetStageData(currentRoom);

        if (currentStageData.isBossRoom)
        {
            SpawnBoss();
            return;
        }

        // Toplam düşman sayısı
        int totalEnemies = currentStageData.normalCount + currentStageData.tankCount + currentStageData.runnerCount + currentStageData.archerCount;
        
        if (totalEnemies == 0)
        {
            // Eger JSON'da bir sey yazilmamissa default 2 
            totalEnemies = 2;
            currentStageData.normalCount = 2;
        }

        GameManager gm = FindAnyObjectByType<GameManager>();
        gm?.InitializeEnemyCount(totalEnemies);

        StartCoroutine(SpawnEnemies(currentStageData));
    }

    StageSpawnData GetStageData(int roomNum)
    {
        // Fail-safe default
        StageSpawnData fallbackData = new StageSpawnData { roomNumber = roomNum, normalCount = 2, isBossRoom = (roomNum > 0 && roomNum % 10 == 0) };

        TextAsset jsonFile = Resources.Load<TextAsset>("Stages");
        if (jsonFile == null)
        {
            Debug.LogError("EnemySpawner: Resources/Stages.json bulunamadi Default degerler kullaniliyor.");
            return fallbackData;
        }

        DungeonSpawnConfig config = JsonUtility.FromJson<DungeonSpawnConfig>(jsonFile.text);
        if (config == null || config.stages == null || config.stages.Length == 0)
        {
            Debug.LogError("EnemySpawner: Stages.json okunamadi veya bos");
            return fallbackData;
        }

        foreach (var stage in config.stages)
        {
            if (stage.roomNumber == roomNum) return stage;
        }

        // Oda bulunamadiysa default dondur.
        return fallbackData;
    }

    void SpawnBoss()
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: bossPrefabs atanmamış");
            return;
        }

        int bossIndex = 0;
        if (GameManager.currentRoom > 10)
        {
            bossIndex = Random.Range(0, bossPrefabs.Length);
        }

        GameObject selectedBoss = bossPrefabs[bossIndex];
        Vector3 spawnPosition = Vector3.zero;

        Instantiate(selectedBoss, spawnPosition, Quaternion.identity);

        GameManager gm = FindAnyObjectByType<GameManager>();
        gm?.InitializeEnemyCount(1);

        Debug.Log($"BOSS ODASI! Oda {GameManager.currentRoom} — Boss spawn edildi.");
    }

    IEnumerator SpawnEnemies(StageSpawnData data)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawner: spawnPoints boş", this);
            yield break;
        }

        // Basilacak tum dusmanlari bir listeye koy
        List<GameObject> enemiesToSpawnList = new List<GameObject>();
        for (int i = 0; i < data.normalCount; i++) if (enemyPrefab != null) enemiesToSpawnList.Add(enemyPrefab);
        for (int i = 0; i < data.tankCount; i++) if (tankEnemyPrefab != null) enemiesToSpawnList.Add(tankEnemyPrefab);
        for (int i = 0; i < data.runnerCount; i++) if (runnerEnemyPrefab != null) enemiesToSpawnList.Add(runnerEnemyPrefab);
        for (int i = 0; i < data.archerCount; i++) if (archerEnemyPrefab != null) enemiesToSpawnList.Add(archerEnemyPrefab);

        // Liste karissin hepsi ayni sirada cikmasin
        for (int i = 0; i < enemiesToSpawnList.Count; i++)
        {
            GameObject temp = enemiesToSpawnList[i];
            int randomIndex = Random.Range(i, enemiesToSpawnList.Count);
            enemiesToSpawnList[i] = enemiesToSpawnList[randomIndex];
            enemiesToSpawnList[randomIndex] = temp;
        }

        for (int i = 0; i < enemiesToSpawnList.Count; i++)
        {
            Transform point = spawnPoints[i % spawnPoints.Length];
            Quaternion spawnRotation = point.rotation;

            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            Vector3 finalSpawnPos = point.position + randomOffset;

            Instantiate(enemiesToSpawnList[i], finalSpawnPos, spawnRotation);

            yield return new WaitForSeconds(0.6f);
        }
    }
}