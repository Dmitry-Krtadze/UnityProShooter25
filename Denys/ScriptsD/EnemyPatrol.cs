using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class EnemyPatrol : MonoBehaviourPunCallbacks, IPunObservable
{
    private NavMeshAgent agent;
    public Transform[] goals;
    Animator en_Animator;
    public float distanceToChangeGoal = 1f;
    private int currentGoal = 0;
    public float chaseDistance = 5f; // Радиус обнаружения игрока
    public float stoppingDistance = 1f; // Расстояние остановки от игрока
    public LayerMask playerLayer;

    public float stopTime = 1.5f; // Публичная переменная для времени остановки, по умолчанию 1.5 секунды
    int hp = 100;
    bool isDead = false; // чтобы избежать повторного уничтожения
    private Transform player; // Оставляем приватным, но добавим публичный метод доступа
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        agent.speed = 2f;

        // Динамическое заполнение массива goals
        GameObject[] patrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");
        goals = new Transform[patrolPoints.Length];
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No objects with tag 'PatrolPoint' found!");
        }
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            goals[i] = patrolPoints[i].transform;
        }

        SetClosestGoal();
        en_Animator = gameObject.GetComponent<Animator>();
        en_Animator.SetBool("IsWalk", true);
    }

    void Update()
    {
        DetectPlayer();
        float currentSpeed = agent.velocity.magnitude;
        en_Animator.SetFloat("Speed", currentSpeed);
        if (isChasing && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
          
            if (distanceToPlayer > stoppingDistance)
            {
                agent.destination = player.position;
                agent.speed = 5f;
                en_Animator.SetBool("IsWalk", false);
                en_Animator.SetBool("IsRun", true);
            }
            else
            {
                en_Animator.SetTrigger("IsAttack");
            }
        }
        else
        {
            Patrol();
            agent.speed = 2f;
            en_Animator.SetBool("IsRun", false);
            en_Animator.SetBool("IsWalk", true);
        }
    }

    // Делаем метод публичным, чтобы DelayDamage мог его вызывать
    public void DetectPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, chaseDistance, playerLayer);
        if (colliders.Length > 0)
        {
            player = colliders[0].transform;
            isChasing = true;
        }
        else
        {
            player = null;
            isChasing = false;
        }
    }

    // Добавляем публичный метод для доступа к player
    public Transform GetPlayer()
    {
        return player;
    }

    void Patrol()
    {
        if (agent.remainingDistance < distanceToChangeGoal || !agent.hasPath)
        {
            Transform nextGoal = goals[currentGoal];
            currentGoal = (currentGoal + 1) % goals.Length;

            if (goals[currentGoal] != player)
            {
                StartCoroutine(PauseBeforeNextGoal());
            }
            else
            {
                agent.destination = player.position;
            }
        }
    }

    IEnumerator PauseBeforeNextGoal()
    {
        yield return new WaitForSeconds(stopTime);
        agent.destination = goals[currentGoal].position;
        agent.speed = 2f;
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(hp);
        }
        else
        {
            hp = (int)stream.ReceiveNext();
        }
    }

    public void OnHit()
    {
        // Только мастер-клиент обрабатывает урон
        if (!PhotonNetwork.IsMasterClient)
            return;

        hp -= 10;

        if (hp <= 0 && !isDead)
        {
            isDead = true;
            PhotonNetwork.Destroy(gameObject);
        }
    }
}

