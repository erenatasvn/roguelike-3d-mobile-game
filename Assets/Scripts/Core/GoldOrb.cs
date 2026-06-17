using UnityEngine;

public class GoldOrb : MonoBehaviour
{
    public int goldAmount = 10; // Kazanılacak altın miktarı

    [Header("Magnet Ayarlari")]
    public float magnetRadius = 2.5f; // Miknatisin cekim mesafesi
    public float moveSpeed = 15f;     // Karakterin ustune dogru ucuracak hiz

    private Transform player;
    private bool isFollowing = false; // Altin su an havada mı kontrolü
    private bool isCollected = false; // Cift toplama engelleyici

    void OnEnable()
    {
        isFollowing = false;
        isCollected = false;

        // Sahnede oyuncuyu bul ve kaydet
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Magnet menzilini JSON'dan cek
        SaveData save = SaveSystem.Load();
        magnetRadius = save.GetMagnetRadius();
    }

    void Update()
    {
        if (player == null) return;

        if (!isFollowing)
        {
            // Oyuncu miknatis alanina girdi mi 
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= magnetRadius)
            {
                isFollowing = true; // Ucmaya basla
            }
        }
        else
        {
            // Oyuncunun merkezine dogru uc
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            // garanti toplama mesafesi
            if (Vector3.Distance(transform.position, player.position) < 0.5f)
            {
                CollectGold();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Sadece Player tagli obje toplayabilir
        if (!other.CompareTag("Player")) return;

        CollectGold();
    }

    void CollectGold()
    {
        // Ayni frame'de cift tetiklenirse cift para vermesin
        if (isCollected) return;
        isCollected = true;

        // Bu rundaki goldlara ekle
        GameManager.sessionGold += goldAmount;

        // Lifetime kazanc istatistigi
        SaveData save = SaveSystem.Load();
        save.totalGoldEarned += goldAmount;
        QuestSystem.CheckAndUpdate(save);
        SaveSystem.Save(save);

        // Oyun icindeki arayuzu guncelle
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.RefreshGold();
        }

        ObjectPooler.ReturnToPool(gameObject); 
    }
}
