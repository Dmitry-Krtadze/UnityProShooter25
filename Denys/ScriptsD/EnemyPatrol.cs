using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float enemySpeed = 3f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float patrolStoppingDistance = 0.5f;
    [SerializeField] private float patrolDetectionRange = 6f;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    
    private int currentPointIndex = 0;
    private Transform[] patrolPoints;
    private Rigidbody rb;
    private GameObject player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

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
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return new WaitForSeconds(0.1f);
        }
    }

    void FixedUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        CheckForPlayer();

        // Проверка на землю (Default Layer)
        bool isGrounded = Physics.SphereCast(transform.position, 0.3f, Vector3.down, out _, 1.1f, groundLayerMask);

        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration); // Притягиваем к земле
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // Фиксируем на земле
        }

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
        Collider[] colliders = Physics.OverlapSphere(transform.position, patrolDetectionRange, playerLayerMask);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                player = collider.gameObject;
                return;
            }
        }
        player = null;
    }

    bool ShouldChasePlayer()
    {
        return player != null && Vector3.Distance(transform.position, player.transform.position) > patrolStoppingDistance;
    }

    void ChasePlayer()
    {
        if (player == null) return;

        Vector3 targetPosition = player.transform.position;
        targetPosition.y = transform.position.y; // Фиксируем Y
        Vector3 direction = (targetPosition - transform.position).normalized;

        rb.AddForce(direction * acceleration, ForceMode.Acceleration);

        LookAtTarget(targetPosition);
    }

    void Patrol()
    {
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector3 targetPosition = targetPoint.position;
        targetPosition.y = transform.position.y; // Фиксируем Y

        MoveTowards(targetPosition);

        // Проверяем, достигли ли точки патрулирования
        if (Vector3.Distance(transform.position, targetPosition) < patrolStoppingDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length; // Переход к следующей точке
        }
    }

    void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.AddForce(direction * acceleration, ForceMode.Acceleration);
        
        // Ограничение скорости, чтобы враг не разгонялся бесконечно
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, enemySpeed);

        LookAtTarget(targetPosition);
    }

    void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}
