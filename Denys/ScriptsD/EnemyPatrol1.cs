using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol1 : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] goals;
    public float distanceToChangeGoal = 1f;
    private int currentGoal = 0;
    public float chaseDistance = 5f; // Радиус обнаружения игрока
    public float stoppingDistance = 1.5f; // Расстояние остановки от игрока
    public LayerMask playerLayer;

    private Transform player;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        SetClosestGoal();
    }

    void Update()
    {
        DetectPlayer();

        if (isChasing && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > stoppingDistance)
            {
                agent.destination = player.position;
            }
            else
            {
                agent.ResetPath(); // Останавливаем врага перед игроком
            }
        }
        else
        {
            Patrol();
        }
    }

    void DetectPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, chaseDistance, playerLayer);
        if (colliders.Length > 0)
        {
            player = colliders[0].transform;
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
    }

    void Patrol()
    {
        if (agent.remainingDistance < distanceToChangeGoal || !agent.hasPath)
        {
            currentGoal = (currentGoal + 1) % goals.Length;
            agent.destination = goals[currentGoal].position;
        }
    }

    void SetClosestGoal()
    {
        float minDistance = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < goals.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, goals[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        currentGoal = closestIndex;
        agent.destination = goals[currentGoal].position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}
