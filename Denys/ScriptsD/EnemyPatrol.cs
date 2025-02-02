using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float enemySpeed = 5f;
    [SerializeField] private GameObject patrolIntoPlayer;
    [SerializeField] private float enemyPatrolDistance = 6f; 
    [SerializeField] private LayerMask playerLayerMask; // Слой, на котором находится игрок

    private int currentPointIndex = 0;
    private Transform[] patrolPoints;

    void Start()
    {
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
            yield return null;
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
        transform.position = Vector3.MoveTowards(
            transform.position,
            patrolIntoPlayer.transform.position,
            enemySpeed * Time.deltaTime
        );

        // Дополнительно: поворот в сторону игрока
        transform.LookAt(patrolIntoPlayer.transform);
    }

    void Patrol()
    {
        Transform targetPoint = patrolPoints[currentPointIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            enemySpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }
}