using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [HideInInspector] public float speed = 10f;
    [HideInInspector] public float damage = 15f;
    [HideInInspector] public bool isFireball = false; // Ateş topu kontrolü
    public float lifetime = 5f; // Kaç saniye destroy

    void Awake()
    {
        // merminin fiziksel olarak oyuncuyu ittirmesini (duvardan geçirmesini) engelle
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnEnable()
    {
        Invoke(nameof(Deactivate), lifetime);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Deactivate()
    {
        ObjectPooler.ReturnToPool(gameObject);
    }

    void Update()
    {
        // İleri doğru hareket et
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }

        // Oyuncuya çarparsa hasar ver
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                
                if (isFireball)
                {
                    // Oyuncu 5 saniye boyunca saniyede 5 hasar alır
                    ph.ApplyDoT(5f, 5f, 1f);
                }
            }
            Deactivate();
            return;
        }

        // Düşmana ve Trigger collider'lara çarpınca yok olma
        if (other.CompareTag("Enemy") || other.isTrigger) return;

        // Başka her şeye çarparsa yok ol
        Deactivate();
    }
}
