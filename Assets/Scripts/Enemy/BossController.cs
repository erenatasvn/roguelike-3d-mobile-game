using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum BossType
{
    Jumper,      // 1. Zıplayan 
    Summoner,    // 2. Minyon Çağıran
    Sprinter,    // 3. Hızlı Koşan
    Mutant,      // 4. Dev Dövüşçü 
    Spider,      // 5. Örümcek 
    Dragon       // 6. Ejderha 
}

public class BossController : MonoBehaviour
{
    [Header("Boss Ayarları")]
    public BossType bossType;
    public string bossName = "GÖLGE KRAL";
    
    [Header("Temel Hareket")]
    public float moveSpeed = 4f;
    public float stopDistance = 2f; 
    [Header("Mutant Özel Ayarları")]
    public float mutantAttack1Delay = 0.5f; // Birinci vuruş gecikmesi
    public float mutantAttack2Delay = 1.2f; // İkinci (yavaş) vuruş gecikmesi
    public float mutantDamage = 40f;        // Mutant'ın vuracağı hasar

    [Header("Yetenek Ayarları")]
    public float skillCooldown = 5f; 
    
    [Header("Özel Ayarlar (Türe Göre)")]
    public GameObject minionPrefab;
    public GameObject projectilePrefab;
    public Transform shootPoint;

    private NavMeshAgent agent;
    private Transform player;
    private EnemyHealth healthComponent;
    private Animator anim;
    private float skillTimer;
    private bool isUsingSkill = false;

    // Özel Yetenek Değişkenleri
    public float jumpAoERadius = 4f;
    public float jumpAoEDamage = 25f;
    public float jumpHeight = 4f;
    public float jumpDuration = 0.8f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        healthComponent = GetComponent<EnemyHealth>();
        anim = GetComponentInChildren<Animator>();
        
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) 
        {
            player = pObj.transform;
            
            // Doğduğu an yüzünü oyuncuya dön
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
        }

        if (agent != null) agent.speed = moveSpeed;

        if (bossType == BossType.Sprinter)
        {
            skillCooldown = 2.5f; // sprinter boss daha sık koşsun atsın
        }
        else if (bossType == BossType.Mutant)
        {
            skillCooldown = 2.0f; // Mutant'ın yumruk atma hızı
        }
        else if (bossType == BossType.Dragon)
        {
            skillCooldown = 1.25f; // Ejderha sık ateş etsin
        }

        skillTimer = skillCooldown;

        // Boss doğduğunda UI Level barı Boss barı olur
        PlayerLevel pl = FindAnyObjectByType<PlayerLevel>();
        if (pl != null && healthComponent != null)
        {
            pl.SetBossMode(true, healthComponent.maxHealth, bossName);
        }
    }

    void Update()
    {
        // Eğer agent kapalıysa (ölmüşse) veya NavMesh üzerinde değilse dur
        if (player == null || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        if (healthComponent == null || healthComponent.maxHealth <= 0) return;

        // Canı UI'a bildir
        PlayerLevel pl = FindAnyObjectByType<PlayerLevel>();
        if (pl != null)
        {
            
        }

        // Ejderhanın sürekli olarak oyuncuya döndür
        if (bossType == BossType.Dragon)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 6f);
            }
        }

        if (isUsingSkill) return;

        // Normal Takip
        float dist = Vector3.Distance(transform.position, player.position);
        
        // Animasyon Hızı
        if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);

        
        if (dist > stopDistance) 
        {
            agent.SetDestination(player.position);
        }
        else 
        {
            agent.ResetPath(); // Yeterince yakındaysa veya Stop Distance büyükse sadece dön!
        }

        skillTimer -= Time.deltaTime;
        if (skillTimer <= 0f)
        {
            skillTimer = skillCooldown;
            StartCoroutine(UseSkill());
        }
    }

    IEnumerator UseSkill()
    {
        isUsingSkill = true;
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        if (anim != null) anim.SetFloat("Speed", 0f); // Ayakların kaymasını önlemek için hızı 0

        switch (bossType)
        {
            case BossType.Jumper:
                yield return StartCoroutine(Skill_Jumper());
                break;
            case BossType.Summoner:
                yield return StartCoroutine(Skill_Summoner());
                break;
            case BossType.Sprinter:
                yield return StartCoroutine(Skill_Sprinter());
                break;
            case BossType.Mutant:
                yield return StartCoroutine(Skill_Mutant());
                break;
            case BossType.Spider:
                yield return StartCoroutine(Skill_Spider());
                break;
            case BossType.Dragon:
                yield return StartCoroutine(Skill_Dragon());
                break;
        }

        if (agent != null && agent.isActiveAndEnabled) agent.isStopped = false;
        isUsingSkill = false;
    }

    // --- YETENEKLER --- //

    IEnumerator Skill_Jumper()
    {
        if (anim != null) anim.SetTrigger("Skill"); // Sıçrama/Yere Vurma animasyonu

        Vector3 startPos = transform.position;
        Vector3 targetPos = player != null ? player.position : startPos;
        
        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            Vector3 flat = Vector3.Lerp(startPos, targetPos, t);
            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = flat + Vector3.up * arc;
            yield return null;
        }

        if (player != null)
        {
            // Y eksenindeki yükseklik farkı yüzünden ıska geçmesini engellemek için X ve Z eksenine bak
            Vector3 bossPosFlat = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 playerPosFlat = new Vector3(player.position.x, 0, player.position.z);
            
            if (Vector3.Distance(bossPosFlat, playerPosFlat) <= jumpAoERadius)
            {
                player.GetComponent<PlayerHealth>()?.TakeDamage(jumpAoEDamage);
            }
        }

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.8f);

        if (agent != null && agent.isActiveAndEnabled) agent.Warp(transform.position);
    }

    IEnumerator Skill_Summoner()
    {
        if (anim != null) anim.SetTrigger("Skill"); // Roar animasyonu

        // Animasyonu 2x hızlı bekleme süresini 0.5'e düşsün
        yield return new WaitForSeconds(0.5f);

        if (minionPrefab != null)
        {
            GameManager gm = FindAnyObjectByType<GameManager>();
            EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();

            for (int i = 0; i < 3; i++)
            {
                // Varsayılan olarak bossun etrafı
                Vector3 spawnPos = transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                
                // KORİDORDA DOĞSUNLAR
                if (spawner != null && spawner.spawnPoints != null && spawner.spawnPoints.Length > 0)
                {
                    Transform randomPoint = spawner.spawnPoints[Random.Range(0, spawner.spawnPoints.Length)];
                    spawnPos = randomPoint.position + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
                }

                Instantiate(minionPrefab, spawnPos, Quaternion.identity);
                if (gm != null) gm.enemyCount++;
            }
        }
    }

    IEnumerator Skill_Sprinter()
    {
        if (anim != null) anim.SetTrigger("Skill"); // Dash animasyonu
        
        yield return new WaitForSeconds(0.2f); // direkt atılsın
        
        float originalSpeed = agent.speed;
        float originalAccel = agent.acceleration;

        agent.speed = 25f; // hız
        agent.acceleration = 120f;

        float sprintTime = 1.5f; // uzun süre koş
        while (sprintTime > 0)
        {
            sprintTime -= Time.deltaTime;
            agent.SetDestination(player.position); 
            yield return null;
        }

        agent.speed = originalSpeed;
        agent.acceleration = originalAccel;
        agent.ResetPath();
        yield return new WaitForSeconds(0.5f); // bekle
    }

    IEnumerator Skill_Mutant()
    {
        int attackType = 0;
        if (anim != null)
        {
            // İki ataktan birini rastgele seç (0 = Hızlı Vuruş, 1 = Yavaş Ağır Vuruş)
            attackType = Random.Range(0, 2);
            anim.SetInteger("AttackIndex", attackType);
            anim.SetTrigger("Attack");
        }

        // Seçilen animasyonun bekleme süresini kullan
        float currentDelay = (attackType == 0) ? mutantAttack1Delay : mutantAttack2Delay;
        yield return new WaitForSeconds(currentDelay);

        // Eğer oyuncu menzilindeyse hasar ver
        if (player != null && Vector3.Distance(transform.position, player.position) <= stopDistance + 1.5f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(mutantDamage); 
        }

        // Animasyonun toparlanmasını bekle
        yield return new WaitForSeconds(0.8f);
    }

    IEnumerator Skill_Spider()
    {
        // örümceğin skilli yok o yüzden attack
        if (anim != null) anim.SetTrigger("Attack"); 
        
        yield return new WaitForSeconds(0.6f); // Isırma animasyonunun hasar anı
        
        // Eğer oyuncu dibindeyse hasar ver
        if (player != null && Vector3.Distance(transform.position, player.position) <= stopDistance + 1.5f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(30f); 
        }
    }

    IEnumerator Skill_Dragon()
    {
        // Ejderhanın 'Attack' animasyonunu kullanıyoruz
        if (anim != null) anim.SetTrigger("Attack"); 
        
        yield return new WaitForSeconds(1f); // Animasyonun ağzı açma süresi bekle

        // 3 Tane peş peşe alev topu fırlat
        for (int i = 0; i < 3; i++)
        {
            if (player != null)
            {
                ShootProjectile(projectilePrefab, player.position, true);
            }
            yield return new WaitForSeconds(0.4f); // Topların arasındaki bekleme süresi
        }
    }

    void ShootProjectile(GameObject prefab, Vector3 targetPos, bool isFireball = false)
    {
        if (prefab == null) return;
        
        // Mermiyi bosstan biraz ileride spawn et
        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 1.5f + transform.forward * 1.5f;
        Vector3 dir = (targetPos + Vector3.up * 1f - spawnPos).normalized; // Oyuncunun göğsüne nişan al
        
        GameObject proj = Instantiate(prefab, spawnPos, Quaternion.LookRotation(dir));
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        
        // Eğer mermide script atanmamışsa otomatik ekle
        if (ep == null)
        {
            ep = proj.AddComponent<EnemyProjectile>();
        }

        ep.damage = isFireball ? 40f : 20f;
        ep.speed = isFireball ? 8f : 15f; // Hızları normal haline döndür
        ep.isFireball = isFireball; // Yanma efektini tetikle
    }

    void OnDestroy()
    {
        PlayerLevel pl = FindAnyObjectByType<PlayerLevel>();
        if (pl != null) pl.SetBossMode(false);
    }
}
