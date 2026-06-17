using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;
    private int dropXPAmount;
    public bool isDead { get; private set; } = false;

    [Header("Can Barı")]
    public Slider healthSlider;
    public RectTransform healthBarCanvas;
    private Vector3 originalScale;
    private Vector3 popScale;
    public float shrinkDelay = 1.5f;
    private float shrinkTimer;

    [Header("Knockback")]
    public NavMeshAgent agent;
    public float knockbackForce = 3f;
    public float stunDuration = 0.2f;

    [Header("XP ve Altın Düşürme")]
    public GameObject xpGemPrefab;
    public float gemSpawnY = 0.5f;
    private static GameObject cachedXpGemPrefab;

    public GameObject goldOrbPrefab;
    private static GameObject cachedGoldOrbPrefab;
    [HideInInspector] public float goldDropChance;
    
    [Header("Ölüm Ayarları")]
    public float deathYOffset = 0f;

    [Header("Boss")]
    public bool isBoss = false;

    [Header("Görsel")]
    public Color enemyColor = Color.white;

    private Renderer cachedRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    private GameManager gameManager;
    private PlayerHealth cachedPlayerHealth;
    private Animator cachedAnimator;

    void Start()
    {
        if (xpGemPrefab != null) cachedXpGemPrefab = xpGemPrefab;
        else if (cachedXpGemPrefab != null) xpGemPrefab = cachedXpGemPrefab;

        if (goldOrbPrefab != null) cachedGoldOrbPrefab = goldOrbPrefab;
        else if (cachedGoldOrbPrefab != null) goldOrbPrefab = cachedGoldOrbPrefab;

        int room = GameManager.currentRoom;

        // Boss prefabı "isBoss" işaretliyse çok daha yüksek can
        if (isBoss)
        {
            // Olunan odaya göre Boss'un canı otomatik ayarlanır.
            maxHealth = maxHealth * Mathf.Pow(1.08f, room - 1);
            dropXPAmount = Mathf.RoundToInt(600f + (room - 1) * 40f); // Boss: Daha fazla xp
        }
        else
        {
            maxHealth = maxHealth * Mathf.Pow(1.06f, room - 1);
            dropXPAmount = 30 + Mathf.RoundToInt((room - 1) * 2.5f); 

            // Normal boss odalarında normal düşmanlar elit olur
            if (room % 10 == 0 && !isBoss)
            {
                maxHealth *= 2.5f;
                dropXPAmount = (room != 40) ? dropXPAmount * 5 : 0;
            }
        }

        currentHealth = maxHealth;

        if (healthSlider != null) { healthSlider.maxValue = maxHealth; healthSlider.value = currentHealth; }
        
        // Eğer Boss ise küçük can barını kapat
        if (isBoss && healthBarCanvas != null) 
        {
            healthBarCanvas.gameObject.SetActive(false);
        }
        else if (healthBarCanvas != null) 
        {
            originalScale = healthBarCanvas.localScale;
            popScale = originalScale * 1.15f; // %15 büyüme
        }

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        gameManager = FindAnyObjectByType<GameManager>();
        cachedPlayerHealth = FindAnyObjectByType<PlayerHealth>();

        // Hit Flash efekti
        cachedRenderer = GetComponentInChildren<Renderer>();
        if (enemyColor != Color.white && cachedRenderer != null)
            cachedRenderer.material.color = enemyColor;
        originalColor = (cachedRenderer != null) ? cachedRenderer.material.color : Color.white;

        // Animator'u  bul ve sakla
        cachedAnimator = GetComponentInChildren<Animator>();
        if (cachedAnimator != null)
            Debug.Log($"[EnemyHealth] ✅ Animator BULUNDU: {cachedAnimator.gameObject.name} | Controller: {cachedAnimator.runtimeAnimatorController?.name}", gameObject);
        else
            Debug.LogError($"[EnemyHealth] ❌ Animator BULUNAMADI! Obje: {gameObject.name}", gameObject);

        // JSON'dan altin dusme sansini cek
        SaveData save = SaveSystem.Load();
        goldDropChance = save.GetGoldDropChance();
    }

    void Update()
    {
        if (shrinkTimer > 0)
        {
            shrinkTimer -= Time.deltaTime;
            if (shrinkTimer <= 0 && healthBarCanvas != null)
                healthBarCanvas.localScale = originalScale;
        }
    }

    public void TakeDamage(float damageAmount, Vector3 hitPoint, bool applyKnockback = true)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        // Lifetime hasar istatistigi run bazli, oda bitince kaydedilir
        GameManager.sessionDamageDealt += (long)damageAmount;

        // Hasar sayisini goster %75 yüksekliğinde kafa/göğüs çevresinde)
        Vector3 dmgPos = transform.position + Vector3.up * 1f;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            dmgPos.y = Mathf.Lerp(col.bounds.min.y, col.bounds.max.y, 0.75f);
        }
        DamageNumberSpawner.Spawn(dmgPos, damageAmount, false);

        if (healthSlider != null) healthSlider.value = currentHealth;
        if (healthBarCanvas != null) { healthBarCanvas.localScale = popScale; shrinkTimer = shrinkDelay; }

        if (isBoss)
        {
            PlayerLevel pl = FindAnyObjectByType<PlayerLevel>();
            if (pl != null) pl.UpdateBossHealth(currentHealth);
        }

        //  Hasar alınca hit flash 
        if (!isFlashing && cachedRenderer != null) StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (applyKnockback && !isBoss) // Bosslar knockback yemesin
        {
            Vector3 knockbackDir = (transform.position - hitPoint).normalized;
            knockbackDir.y = 0;
            StartCoroutine(ApplyKnockback(knockbackDir));
        }
    }

    // EnemySpawner/BossController'dan gelen overload (hitPoint yok)
    public void TakeDamage(float damageAmount)
    {
        TakeDamage(damageAmount, transform.position, true);
    }

    IEnumerator HitFlash()
    {
        isFlashing = true;
        cachedRenderer.material.color = new Color(1f, 0.15f, 0.15f); // Kırmızı
        yield return new WaitForSeconds(0.2f);
        if (cachedRenderer != null && !isDead)
            cachedRenderer.material.color = originalColor;
        isFlashing = false;
    }

    IEnumerator ApplyKnockback(Vector3 direction)
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
            agent.velocity = direction * knockbackForce;
            yield return new WaitForSeconds(stunDuration);
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
                agent.updateRotation = true;
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Lifetime istatistikleri guncelle
        SaveData save = SaveSystem.Load();
        save.totalEnemiesKilled++;
        if (isBoss) save.totalBossesKilled++;
        QuestSystem.CheckAndUpdate(save);
        SaveSystem.Save(save);

        // Bloodthirst oldürulen her düsman basina %50 ihtimalle can yenile
        if (PlayerStats.lifeStealPerKill > 0f && Random.value < 0.5f)
            cachedPlayerHealth?.HealFlat(PlayerStats.lifeStealPerKill);

        // XP gem spawn et
        if (xpGemPrefab != null && dropXPAmount > 0)
        {
            // XP orbu sola kaysin (-1.0f)
            Vector3 spawnPos = new Vector3(transform.position.x - 1.0f, gemSpawnY, transform.position.z - 0.5f);
            GameObject gemObj = ObjectPooler.Spawn(xpGemPrefab, spawnPos, Quaternion.identity);
            if (gemObj != null)
            {
                XPGem gemScript = gemObj.GetComponent<XPGem>();
                if (gemScript != null) gemScript.xpAmount = dropXPAmount;
            }
        }

        // Altın (Gold) spawn et
        if (goldOrbPrefab != null && Random.value <= goldDropChance)
        {
            // Gold orbu biraz kaysin (+1.0f)
            Vector3 spawnPos = new Vector3(transform.position.x + 1.0f, gemSpawnY, transform.position.z + 0.5f);
            GameObject goldObj = ObjectPooler.Spawn(goldOrbPrefab, spawnPos, Quaternion.identity);
            if (goldObj != null)
            {
                GoldOrb goldScript = goldObj.GetComponent<GoldOrb>();
            if (goldScript != null) 
            {
                // Rastgele altın miktarı hesaplama
                if (isBoss) 
                {
                    goldScript.goldAmount = 500; 
                }
                else 
                {
                    float r = Random.value;
                    if (r < 0.10f) goldScript.goldAmount = Random.Range(100, 201); // %10 ihtimalle 100-200 arası
                    else if (r < 0.30f) goldScript.goldAmount = Random.Range(50, 100); // %20 ihtimalle 50-99 arası
                    else goldScript.goldAmount = Random.Range(5, 50); // %70 ihtimalle 5-49 arası

                    // Elit düşmansa altın çarpı 3
                    if (dropXPAmount > 100) goldScript.goldAmount *= 3;
                }
            }
            }
        }

        gameManager?.OnEnemyDied();

        // EnemySpawner / Manager için collider kapat
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        if (agent != null) 
        {
            agent.isStopped = true;
            // Ajanı kapatmıyoruz, böylece 2.5 saniye boyunca silinene kadar NavMesh onu kusursuz bir şekilde zeminde tutacak.
        }

        // Ölüm sonrası saldırı ve takip scriptlerini durdur
        EnemyAttack ea = GetComponent<EnemyAttack>();
        if (ea != null) ea.enabled = false;
        EnemyFollow ef = GetComponent<EnemyFollow>();
        if (ef != null) ef.enabled = false;
        ArcherEnemy archer = GetComponent<ArcherEnemy>();
        if (archer != null) archer.enabled = false;
        BossController bc = GetComponent<BossController>();
        if (bc != null) bc.enabled = false;
        
        // Ölünce yere gömülmeyi engellemek için fiziği durdur
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Modele özel ölüm ofseti yere gömülme engellemek için
        if (deathYOffset > 0f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + deathYOffset, transform.position.z);
        }

        // Canvas kapat
        if (healthBarCanvas != null) healthBarCanvas.gameObject.SetActive(false);

        // Animator ile Ölüm Animasyonu
        Debug.Log($"[EnemyHealth] Die() çağrıldı. cachedAnimator null mu: {cachedAnimator == null}", gameObject);
        if (cachedAnimator != null)
        {
            // Kuyrukta bekleyen Attack temizle death her zaman öncelikli 
            cachedAnimator.ResetTrigger("Attack");
            cachedAnimator.SetTrigger("Death");
            Debug.Log($"[EnemyHealth] ✅ Death trigger gönderildi! Mevcut state: {cachedAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash}", gameObject);
            StartCoroutine(DieAnimationWait());
        }
        else
        {
            // Eğer animatör yoksa sil
            Debug.LogError("[EnemyHealth] ❌ Animator yok, direkt siliniyor!", gameObject);
            Destroy(gameObject, 0.2f);
        }
    }

    IEnumerator DieAnimationWait()
    {
        float waitTime = isBoss ? 5.5f : 2.5f; // Bosslar yerde daha uzun kalsın
        Debug.Log($"[EnemyHealth] ⏳ DieAnimationWait başladı, {waitTime}s bekleniyor...", gameObject);
        yield return new WaitForSeconds(waitTime);
        Debug.Log($"[EnemyHealth] 💀 {waitTime}s doldu, obje siliniyor.", gameObject);
        Destroy(gameObject);
    }
}