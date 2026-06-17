using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    [Header("Mevcut Silah (Otomatik Seçilir)")]
    public WeaponData currentWeapon;

    [Header("Görsel Silah (Kozmetik)")]
    public Transform weaponHolder; // Karakterin elindeki o tutma noktası
    [HideInInspector] public GameObject visualWeaponInHand; // kod otomatik dolduracak

    [Header("Tüm Silah Veritabanı")]
    public WeaponData[] allWeaponDatas;

    void Awake()
    {
        // JSON kayıt dosyasından silah seçimini oku
        SaveData save = SaveSystem.Load();
        string equipped = save.equippedWeapon;

        // Silah ID'sine göre doğru WeaponData'yı bul
        if (allWeaponDatas != null)
        {
            foreach (WeaponData data in allWeaponDatas)
            {
                if (data.weaponID == equipped)
                {
                    currentWeapon = data;
                    break;
                }
            }
        }

        // Bulunamazsa dizideki ilk silahı varsayılan yap
        if (currentWeapon == null && allWeaponDatas != null && allWeaponDatas.Length > 0)
        {
            currentWeapon = allWeaponDatas[0];
        }

        // Kalıcı hasar geliştirmesini PlayerStats'a uygula
        PlayerStats.permDamageMultiplier = save.GetDamageMultiplier();
        PlayerStats.damageMultiplier = save.GetDamageMultiplier();

        if (currentWeapon == null)
            Debug.LogWarning("WeaponManager: Silah bulunamadı");
        else
            SpawnVisualWeapon();
    }

    // Seçilen silahın 3D Prefab'ını elinde doğurur
    void SpawnVisualWeapon()
    {
        if (weaponHolder == null || currentWeapon.weaponModel == null) return;

        // Varsa eskisini sil
        if (visualWeaponInHand != null) Destroy(visualWeaponInHand);

        // Yenisini WeaponHolder'ın içine 
        visualWeaponInHand = Instantiate(currentWeapon.weaponModel, weaponHolder);

        // Pozisyon ve açıları sıfırla 
        visualWeaponInHand.transform.localPosition = currentWeapon.weaponModel.transform.localPosition;
        visualWeaponInHand.transform.localRotation = currentWeapon.weaponModel.transform.localRotation;
    }

    private float nextAttackTime = 0f;
    public Transform attackPoint;

    private GameObject activeAxe = null;
    private GameObject activeAxe2 = null; // Multishot için ikinci balta

    public void ClearActiveAxe()  
    { 
        activeAxe  = null; 
        CheckAndShowVisualWeapon();
    }
    
    public void ClearActiveAxe2() 
    { 
        activeAxe2 = null; 
        CheckAndShowVisualWeapon();
    }

    void CheckAndShowVisualWeapon()
    {
        if (currentWeapon != null && currentWeapon.type == WeaponData.WeaponType.Axe)
        {
            if (activeAxe == null && activeAxe2 == null && visualWeaponInHand != null)
            {
                visualWeaponInHand.SetActive(true);
            }
        }
    }

    private Coroutine attackCoroutine;

    public bool TryAttack()
    {
        if (Time.time >= nextAttackTime && currentWeapon != null)
        {
            // Balta sahadayken vurma animasyonu yapmasını engelle
            if (!CanAttack()) return false;

            attackCoroutine = StartCoroutine(DelayedAttack());
            float finalRate = currentWeapon.attackRate / PlayerStats.attackSpeedMultiplier;
            nextAttackTime = Time.time + finalRate;
            return true;
        }
        return false;
    }

    void Attack()
    {
        switch (currentWeapon.type)
        {
            case WeaponData.WeaponType.Sword: SwordAttack(); break;
            case WeaponData.WeaponType.Staff: StaffAttack(); break;
            case WeaponData.WeaponType.Axe: AxeAttack(); break;
        }
    }

    public void CancelAttack()
    {
        // Karakter mermi çıkmadan önce hareket ederse saldırıyı iptal et
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            // İptal edildiği için bekleme süresini sıfırlıyoruz
            nextAttackTime = Time.time; 
        }
    }

    // Animasyonla merminin çıkışını sağlamak için ufak gecikme
    IEnumerator DelayedAttack()
    {
        yield return new WaitForSeconds(0.15f); // 150 milisaniye bekle (Kolu havaya kaldırma süresi)
        Attack();
        attackCoroutine = null; // Saldırı başarılı
    }

    bool CanAttack()
    {
        if (currentWeapon.type == WeaponData.WeaponType.Axe)
        {
            bool hasMultishot = AbilityManager.Instance?.HasAbility(AbilityID.Multishot) ?? false;
            if (activeAxe != null && !hasMultishot) return false;
            if (activeAxe != null && activeAxe2 != null) return false;
        }
        return true;
    }

    void SwordAttack()
    {
        if (currentWeapon.projectilePrefab == null || attackPoint == null) return;
        GameObject swordObj = ObjectPooler.Spawn(currentWeapon.projectilePrefab, attackPoint.position, transform.rotation);
        if (swordObj == null) return;
        Sword s = swordObj.GetComponent<Sword>();
        if (s != null) 
        { 
            s.damage = currentWeapon.damage; 
            s.range = currentWeapon.range; 
            s.elementType = GetEquippedElement();
        }
    }

    // ── Asa (Staff) Saldırısı ────────────────────────────────────────────────
    void StaffAttack()
    {
        if (currentWeapon.projectilePrefab == null || attackPoint == null) return;
        StartCoroutine(DelayedStaffShot());
    }

    IEnumerator DelayedStaffShot()
    {
        // Asaya özel atış öncesi bekleme
        yield return new WaitForSeconds(0.5f);

        bool hasMultishot     = AbilityManager.Instance?.HasAbility(AbilityID.Multishot) ?? false;
        bool hasFrontPlus1    = AbilityManager.Instance?.HasAbility(AbilityID.FrontArrowPlus1) ?? false;

        if (hasFrontPlus1)
        {
            // Yan yana 2 büyü: karakterin sağına ve soluna küçük offset
            SpawnStaffProjectile(transform.rotation, Vector3.right * 0.3f, hasFrontPlus1 ? 0.75f : 1f);
            SpawnStaffProjectile(transform.rotation, Vector3.left  * 0.3f, hasFrontPlus1 ? 0.75f : 1f);
            if (hasMultishot) StartCoroutine(DelayedDouble(transform.rotation, 0.75f));
        }
        else
        {
            // Tek büyü (Düz ateş)
            SpawnStaffProjectile(transform.rotation, Vector3.zero, 1f);
            if (hasMultishot) StartCoroutine(DelayedDouble(transform.rotation, 0.75f));
        }
    }

    IEnumerator DelayedDouble(Quaternion rotation, float damageMult)
    {
        // İkinci atış için bekle
        yield return new WaitForSeconds(0.15f);

        bool hasFrontPlus1 = AbilityManager.Instance?.HasAbility(AbilityID.FrontArrowPlus1) ?? false;
        if (hasFrontPlus1)
        {
            SpawnStaffProjectile(rotation, Vector3.right * 0.3f, damageMult);
            SpawnStaffProjectile(rotation, Vector3.left  * 0.3f, damageMult);
        }
        else
        {
            SpawnStaffProjectile(rotation, Vector3.zero, damageMult);
        }
    }

    // Büyü Topu spawn eder
    void SpawnStaffProjectile(Quaternion rotation, Vector3 localOffset, float damageMult)
    {
        Vector3 spawnPos = attackPoint.position + transform.TransformDirection(localOffset);
        GameObject projObj = ObjectPooler.Spawn(currentWeapon.projectilePrefab, spawnPos, rotation);
        if (projObj == null) return;
        StaffProjectile p = projObj.GetComponent<StaffProjectile>();
        if (p == null) return;

        p.damage    = currentWeapon.damage * damageMult;
        p.speed     = currentWeapon.projectileSpeed;

        // Element yeteneği varsa 
        p.elementType = GetEquippedElement();
    }

    // Oyuncunun en son aldığı element yeteneğini döndürür
    ElementType GetEquippedElement()
    {
        if (AbilityManager.Instance == null) return ElementType.None;
        if (AbilityManager.Instance.HasAbility(AbilityID.DarkTouch)) return ElementType.Dark;
        if (AbilityManager.Instance.HasAbility(AbilityID.Freeze))    return ElementType.Freeze;
        if (AbilityManager.Instance.HasAbility(AbilityID.Blaze))     return ElementType.Blaze;
        if (AbilityManager.Instance.HasAbility(AbilityID.Poison))    return ElementType.Poison;
        return ElementType.None;
    }

    // Balta Saldırısı 
    void AxeAttack()
    {
        if (currentWeapon.projectilePrefab == null || attackPoint == null) return;

        bool hasMultishot = AbilityManager.Instance?.HasAbility(AbilityID.Multishot) ?? false;
        
        if (activeAxe == null)
        {
            activeAxe = SpawnAxe(Vector3.zero);
            if (visualWeaponInHand != null) visualWeaponInHand.SetActive(false); // Elindekini sakla
        }
        else if (hasMultishot && activeAxe2 == null)
        {
            activeAxe2 = SpawnAxe(Vector3.zero);
            activeAxe2.transform.Rotate(0, 15f, 0); // İkinci balta biraz kavisli 
        }
    }

    GameObject SpawnAxe(Vector3 localOffset)
    {
        Vector3 spawnPos = attackPoint.position + transform.TransformDirection(localOffset);
        GameObject axeObj = ObjectPooler.Spawn(currentWeapon.projectilePrefab, spawnPos, transform.rotation);
        if (axeObj == null) return null;
        Axe axeScript = axeObj.GetComponent<Axe>();
        if (axeScript != null)
        {
            axeScript.damage       = currentWeapon.damage;
            axeScript.speed        = currentWeapon.projectileSpeed;
            axeScript.maxDistance  = currentWeapon.range;
            axeScript.halfDistance = currentWeapon.range / 2f;
            axeScript.player       = transform;
            axeScript.elementType  = GetEquippedElement();
            bool isSecond = (activeAxe != null);
            axeScript.weaponManager = this;
            axeScript.isSecondAxe   = isSecond;
        }
        return axeObj;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null || currentWeapon == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, currentWeapon.range);
    }
}