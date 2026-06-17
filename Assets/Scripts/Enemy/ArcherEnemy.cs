using UnityEngine;
using UnityEngine.AI;


public class ArcherEnemy : MonoBehaviour
{
    [Header("Mesafe Ayarları")]
    public float preferredDistance = 8f;
    public float minDistance = 0f;        
    public float moveSpeed = 2.5f;

    [Header("Atış Ayarları")]
    public GameObject projectilePrefab;   // prefabı al
    public float shootInterval = 2.5f;   // Kaç saniyede bir atar
    public float projectileDamage = 15f;
    public float projectileSpeed = 10f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator anim;
    private float shootTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.updateRotation = true;
        }

        minDistance = 1.5f; 


        anim = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // İlk atışı erkene al
        shootTimer = shootInterval * 0.5f;

        // Hasar odaya göre artsın (%4)
        int room = GameManager.currentRoom;
        projectileDamage = projectileDamage * Mathf.Pow(1.04f, room - 1);
        if (room % 10 == 0) projectileDamage *= 1.5f; // Boss odalarında elit olurlar
    }

    void Update()
    {
        if (player == null || agent == null || !agent.isActiveAndEnabled) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);

        if (dist < minDistance)
        {
            Vector3 fleeDir = (transform.position - player.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDir * 0.2f;
            agent.SetDestination(fleeTarget);
        }
        else if (dist > preferredDistance)
        {
            // Oyuncu çok uzak yaklaş
            agent.SetDestination(player.position);
        }
        else
        {
            // Doğru mesafede dur, yüzü çevir, ateş et
            agent.ResetPath();
            FacePlayer();

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootInterval;
            }
        }
    }

    void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    void Shoot()
    {
        if (anim != null) anim.SetTrigger("Attack");

        if (projectilePrefab == null)
        {
            Debug.LogWarning("ArcherEnemy: projectilePrefab atanmamış", this);
            return;
        }

        StartCoroutine(SpawnProjectileDelayed());
    }

    System.Collections.IEnumerator SpawnProjectileDelayed()
    {
        // Atış animasyonunda merminin fırlatılmasını bekle
        yield return new WaitForSeconds(0.8f);

        if (player == null || projectilePrefab == null) yield break;

        // Mermiyi gövdeden çıkar 
        Vector3 spawnPos = transform.position + Vector3.up * 1.2f + transform.forward * 0.5f;

        // Oyuncunun tam pozisyonuna bak
        Vector3 targetPos = player.position + Vector3.up * 1f;
        Vector3 dir = (targetPos - spawnPos).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject proj = ObjectPooler.Spawn(projectilePrefab, spawnPos, rot);
        if (proj != null)
        {
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null)
            {
                ep.damage = projectileDamage;
                ep.speed = projectileSpeed;
            }
        }
    }

    // Gizmos: atış mesafesini editörde göster
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, preferredDistance);
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, minDistance);
    }
}
