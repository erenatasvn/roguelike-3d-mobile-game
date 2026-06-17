using UnityEngine;

// Her defasında Inspector'dan ayarlamamak için veya her yerden ulaşabilmek için Singleton mantığı
public class DamageNumberSpawner : MonoBehaviour
{
    public static DamageNumberSpawner Instance { get; private set; }
    
    [Header("DamageNumber prefabı")]
    public GameObject damageNumberPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public static void Spawn(Vector3 position, float amount, bool isPlayerDamage = false, Color? customColor = null)
    {
        if (Instance == null || Instance.damageNumberPrefab == null) return;
        
        GameObject go = ObjectPooler.Spawn(Instance.damageNumberPrefab, position, Quaternion.identity);
        if (go == null) return;
        DamageNumber dn = go.GetComponent<DamageNumber>();
        if (dn != null) dn.Setup(amount, isPlayerDamage, customColor);
    }
}
