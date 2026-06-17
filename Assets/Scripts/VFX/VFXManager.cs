using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [Header("Element Partikül Prefabları")]
    public GameObject blazeVFXPrefab;
    public GameObject poisonVFXPrefab;
    public GameObject freezeVFXPrefab;
    [Header("Instant VFX (Dark)")]
    public GameObject darkTouchVFXPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Düşmanın içine VFX ekler ve referansını döndürür 
    public GameObject PlayDoTVFX(ElementType type, Transform target)
    {
        GameObject prefab = GetPrefabForElement(type);
        if (prefab == null || target == null) return null;

        Vector3 spawnPos = target.position;
        Collider col = target.GetComponent<Collider>();
        if (col != null)
        {
            // Partiküller (göbek/bel hizası) çıksın
            spawnPos.y = Mathf.Lerp(col.bounds.min.y, col.bounds.max.y, 0.50f);
        }

        GameObject vfx = ObjectPooler.Spawn(prefab, spawnPos, Quaternion.identity);
        if (vfx != null) vfx.transform.SetParent(target);
        return vfx;
    }

    // Anlık efektler 
    public void PlayInstantVFX(ElementType type, Vector3 position)
    {
        GameObject prefab = GetPrefabForElement(type);
        if (prefab == null) return;

        GameObject vfx = ObjectPooler.Spawn(prefab, position, Quaternion.identity);
        if (vfx != null) StartCoroutine(ReturnVFXAfter(vfx, 2f));
    }

    System.Collections.IEnumerator ReturnVFXAfter(GameObject vfx, float duration)
    {
        yield return new WaitForSeconds(duration);
        ObjectPooler.ReturnToPool(vfx);
    }

    private GameObject GetPrefabForElement(ElementType type)
    {
        switch (type)
        {
            case ElementType.Blaze: return blazeVFXPrefab;
            case ElementType.Poison: return poisonVFXPrefab;
            case ElementType.Freeze: return freezeVFXPrefab;
            case ElementType.Dark: return darkTouchVFXPrefab;
        }
        return null;
    }
}
