using UnityEngine;

public class XPGem : MonoBehaviour
{
    public int xpAmount = 20; // Verecegi XP degeri

    [Header("Magnet Ayarlari")]
    public float magnetRadius = 2f; // Miknatisin cekim mesafesi
    public float moveSpeed = 15f;   // Tasi oyuncuya dogru ucuracak hiz

    private Transform player;
    private bool isFollowing = false; // Tas su an ucuyor mu?
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
    }

    void Update()
    {
        // Yerde dururken havada ucarken donme efekti 
        transform.Rotate(0, 50 * Time.deltaTime, 0);

        if (player == null) return;

        if (!isFollowing)
        {
            // Oyuncu miknatis alanina girdi mi diye kontrol et
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= magnetRadius)
            {
                isFollowing = true; // oyuncuya uc
            }
        }
        else
        {
            // oyuncuya uc
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            // garanti toplama mesafesi
            if (Vector3.Distance(transform.position, player.position) < 0.5f)
            {
                CollectGem(player.gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Sadece Player toplayabilir
        if (!other.CompareTag("Player")) return;

        CollectGem(other.gameObject);
    }

    // 
    void CollectGem(GameObject playerObj)
    {
        // Ayni frame'de hem Update hem OnTriggerEnter calisirsa cift XP vermesin
        if (isCollected) return;
        isCollected = true;

        PlayerLevel playerLevel = playerObj.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.AddXP(xpAmount);
            ObjectPooler.ReturnToPool(gameObject); // XP'yi ver tasi poola at
        }
    }
}