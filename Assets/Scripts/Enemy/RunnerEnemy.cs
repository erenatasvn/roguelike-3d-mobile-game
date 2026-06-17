using UnityEngine;
using UnityEngine.AI;


public class RunnerEnemy : MonoBehaviour
{
    [Header("Hareket")]
    public float speed = 6f;              // Normal düşmanın 2 katı
    public float zigzagInterval = 0.8f;   // Zigzag süresi
    public float zigzagRange = 3.5f;      // Yana ne kadar kayar 

    private Transform player;
    private NavMeshAgent agent;
    private Animator anim;
    private float zigzagTimer;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            int room = GameManager.currentRoom;
            // odaya göre hız artsın
            float scaledSpeed = Mathf.Min(speed * (1f + (room - 1) * 0.03f), 9f);
            agent.speed = scaledSpeed;
            agent.updateRotation = true;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        zigzagTimer = 0f; // Hemen ilk hamleyi yapsın
    }

    void Update()
    {
        if (player == null || agent == null || !agent.isActiveAndEnabled || agent.isStopped) return;

        zigzagTimer -= Time.deltaTime;
        if (zigzagTimer <= 0f)
        {
            UpdateDestination();
            zigzagTimer = zigzagInterval;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    void UpdateDestination()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;

        // XZ düzleminde dik vektör (zigzag yönü)
        Vector3 perp = new Vector3(-toPlayer.z, 0f, toPlayer.x);
        float sideOffset = Random.Range(-zigzagRange, zigzagRange);

        Vector3 destination = player.position + perp * sideOffset;
        agent.SetDestination(destination);
    }
}
