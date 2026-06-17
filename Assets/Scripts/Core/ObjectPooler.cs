using System.Collections.Generic;
using UnityEngine;

// Her prefab'ın orjinal referansını tut
public class PooledObject : MonoBehaviour
{
    public GameObject originalPrefab;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    // Prefab referansını anahtar olarak alıp queue tutan havuz.
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    // hierarchyde düzenli durması için havuzlanan objelerin ebeveyni.
    private Transform poolParent;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameObject parentObj = new GameObject("--- OBJECT POOL ---");
        poolParent = parentObj.transform;
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (Instance == null)
        {
            GameObject managerObj = new GameObject("ObjectPooler");
            Instance = managerObj.AddComponent<ObjectPooler>();
        }

        if (prefab == null) return null;

        if (!Instance.poolDictionary.ContainsKey(prefab))
        {
            Instance.poolDictionary[prefab] = new Queue<GameObject>();
        }

        GameObject objectToSpawn = null;
        Queue<GameObject> queue = Instance.poolDictionary[prefab];

        // Kuyrukta pasif obje var mı diye kontrol et
        while (queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            if (obj != null) // Obje Unity tarafından silinmemişse
            {
                objectToSpawn = obj;
                break;
            }
        }

        // Havuzda obje yoksa yeni oluştur
        if (objectToSpawn == null)
        {
            objectToSpawn = Instantiate(prefab, position, rotation, Instance.poolParent);
            PooledObject pooledComp = objectToSpawn.AddComponent<PooledObject>();
            pooledComp.originalPrefab = prefab;
        }
        else
        {
            // Var olanı konumlandır ve görünür yap
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);
        }

        return objectToSpawn;
    }

    public static void ReturnToPool(GameObject obj)
    {
        if (Instance == null)
        {
            GameObject managerObj = new GameObject("ObjectPooler");
            Instance = managerObj.AddComponent<ObjectPooler>();
        }

        if (obj == null) return;

        PooledObject pooledComp = obj.GetComponent<PooledObject>();
        if (pooledComp == null || pooledComp.originalPrefab == null)
        {
            // Bu obje havuz sistemiyle oluşmamışsa sil
            Destroy(obj);
            return;
        }

        // Zaten pasifse bir şey yapma
        if (!obj.activeSelf) return;

        obj.SetActive(false);
        obj.transform.SetParent(Instance.poolParent);

        if (Instance != null)
        {
            GameObject prefab = pooledComp.originalPrefab;
            if (!Instance.poolDictionary.ContainsKey(prefab))
                Instance.poolDictionary[prefab] = new Queue<GameObject>();

            Instance.poolDictionary[prefab].Enqueue(obj);
        }
        else
        {
            // Oyun kapanırken vb. Instance yok olmuşsa direkt sil
            Destroy(obj);
        }
    }
}
