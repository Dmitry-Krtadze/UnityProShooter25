using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float enemySpeed = 5f;
    [SerializeField] private GameObject patrolIntoPlayer;
    [SerializeField] private float enemyPatrolDistance = 6f; 
    [SerializeField] private LayerMask playerLayerMask;

    private int currentPointIndex = 0;
    private Transform[] patrolPoints;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Запрещаем физическое вращение

        InitializePatrolPoints();
        StartCoroutine(FindPlayerWhenAvailable());
    }

    void InitializePatrolPoints()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("PatrolPoint");
        if (points.Length == 0)
        {
            Debug.LogError("No patrol points found!");
            return;
        }

        patrolPoints = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            patrolPoints[i] = points[i].transform;
        }

        transform.position = patrolPoints[0].position;
    }

    private IEnumerator FindPlayerWhenAvailable()
    {
        while (patrolIntoPlayer == null)
        {
            patrolIntoPlayer = GameObject.FindGameObjectWithTag("Player");
            yield return new WaitForSeconds(0.1f); // Делаем паузу перед следующей попыткой поиска
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyPatrolDistance);
    }

    void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        CheckForPlayer();

        if (ShouldChasePlayer())
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position, 
            enemyPatrolDistance, 
            playerLayerMask
        );

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                patrolIntoPlayer = collider.gameObject;
                return;
            }
        }
        patrolIntoPlayer = null;
    }

    bool ShouldChasePlayer()
    {
        return patrolIntoPlayer != null 
            && Vector3.Distance(transform.position, patrolIntoPlayer.transform.position) <= enemyPatrolDistance;
    }

    void ChasePlayer()
    {
        Vector3 targetPosition = patrolIntoPlayer.transform.position;
        targetPosition.y = transform.position.y; // Фиксируем Y, чтобы враг не поднимался и не опускался

        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * enemySpeed; // Двигаем врага через физику

        LookAtTarget(targetPosition);
    }

    void Patrol()
    {
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector3 targetPosition = targetPoint.position;
        targetPosition.y = transform.position.y; // Оставляем текущую высоту

        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * enemySpeed; // Двигаем врага через физику

        LookAtTarget(targetPosition);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // Убираем наклон по оси Y

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}
