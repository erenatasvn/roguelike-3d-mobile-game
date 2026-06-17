using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    // Dusmanin yurume hizi
    public float speed = 3f;

    // Dusmanin takip edecegi hedef
    private Transform target;
    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            int room = GameManager.currentRoom;
            // Oda basina %3 hiz artis hesabi, maks 6 m/s
            float currentSpeed = speed * (1f + (room - 1) * 0.03f);
            agent.speed = Mathf.Min(currentSpeed, 6f);
            
            agent.updateRotation = true;
        }

        // Player objesini bul ve hedefe kitlen
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        // Agent yoksa veya knockback sirasinda duruyorsa hedefe gitme
        if (agent == null || !agent.isActiveAndEnabled || agent.isStopped)
            return;

        if (target != null)
        {
            agent.SetDestination(target.position);
        }

        // Animasyon hizi
        if (anim != null && agent != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
}