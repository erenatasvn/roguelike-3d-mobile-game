using UnityEngine;

public class CharacterPreview : MonoBehaviour
{
    [Header("Silah Ayarları")]
    public Transform weaponHolder; // Karakterin elindeki boş obje
    public WeaponData[] allWeaponDatas; // Tüm silahların ScriptableObject'leri (Inspector'da)

    [Header("Animasyon")]
    public Animator animator;
    public float rotationSpeed = 20f; // Kendi etrafında yavaşça dönme hızı

    private GameObject currentVisualWeapon;

    void OnEnable()
    {
        UpdateWeaponPreview();
    }

    void Update()
    {
        // Karakteri yavaşça kendi etrafında döndür
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    public void UpdateWeaponPreview()
    {
        SaveData save = SaveSystem.Load();
        if (save == null) return;

        string equippedID = save.equippedWeapon;

        // Doğru silah verisini bul
        WeaponData equippedData = null;
        if (allWeaponDatas != null)
        {
            foreach (var data in allWeaponDatas)
            {
                if (data.weaponID == equippedID)
                {
                    equippedData = data;
                    break;
                }
            }
        }

        if (equippedData == null || equippedData.weaponModel == null) return;

        // Eski silahı sil
        if (currentVisualWeapon != null)
        {
            Destroy(currentVisualWeapon);
        }

        // Yeni silahı doğur ve ele tak
        currentVisualWeapon = Instantiate(equippedData.weaponModel, weaponHolder);
        
        // Pozisyon ve açıları sıfırla (Prefab'daki gibi kalsın)
        currentVisualWeapon.transform.localPosition = equippedData.weaponModel.transform.localPosition;
        currentVisualWeapon.transform.localRotation = equippedData.weaponModel.transform.localRotation;
        currentVisualWeapon.transform.localScale = equippedData.weaponModel.transform.localScale;
    }
}
