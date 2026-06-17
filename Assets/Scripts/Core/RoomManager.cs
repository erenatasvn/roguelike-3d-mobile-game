using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Oda Tasarımları")]
    public GameObject[] roomPrefabs;

    private GameObject currentRoomObj;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SpawnRandomRoom();
    }

    public void SpawnRandomRoom()
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0) return;

        // Eski odayı sil
        if (currentRoomObj != null)
        {
            Destroy(currentRoomObj);
        }

        int randomIndex = 0; // Varsayılan olarak 0. oda 
        
        bool isBossRoom = (GameManager.currentRoom % 10 == 0);

        // Eğer 1. veya 2. odadaysak veya Boss Odasındaysak her zaman 0. indeksi seç
        // Diğer odalarda tamamen rastgele seç
        if (GameManager.currentRoom > 2 && !isBossRoom)
        {
            randomIndex = Random.Range(0, roomPrefabs.Length);
        }

        GameObject selectedPrefab = roomPrefabs[randomIndex];

        if (selectedPrefab != null)
        {
            currentRoomObj = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);
            currentRoomObj.transform.SetParent(transform);
        }
    }
}
