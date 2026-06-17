using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponID; // Dükkanla eşleşecek ID 

    public enum WeaponType { Sword, Staff, Axe }
    public WeaponType type; // Silahın tipi

    public ElementType elementType = ElementType.None;

    // Temel Ayarlar
    public string weaponName; 
    public float damage; 
    public float attackRate; 
    public float range; 

    // Görüntüler
    public GameObject weaponModel; 
    public GameObject projectilePrefab; 

    // Menzilli Ayarlar
    public float projectileSpeed; 
}
