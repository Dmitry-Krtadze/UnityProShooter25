using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyPatrol1 : MonoBehaviour
{
    // Компонент агента
    UnityEngine.AI.NavMeshAgent agent;
    // Массив положений точек назначения
    public Transform[] goals;
    // Расстояние на которое необходимо приблизиться к точке
    public float distanceToChangeGoal;
    // Номер текущей целевой точки
    int currentGoal = 0;
    CharacterController charCtrl;
    public GameObject player;
    public float chaseDistance = 5f; // Радиус преследования
    private NavMeshAgent nMesh; 

    void Start()
    {
        // Сохранение компонента агента и направление к первой точке
        agent = GetComponent<NavMeshAgent>();
        agent.destination = goals[0].position;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        Debug.Log(agent.speed);
        if (player == null) return;

        float playerDistance = Vector3.Distance(transform.position, player.transform.position);

        // Если игрок в радиусе, меняем точку назначения на игрока
        if (playerDistance < chaseDistance)
        {
            agent.destination = player.transform.position;
            agent.speed = agent.speed + 2f;
        }
        // Если враг не преследует игрока, то патрулирует по точкам
        else if (agent.remainingDistance < distanceToChangeGoal)
        {
            // Смена точки на следующую
            currentGoal++;
            if (currentGoal >= goals.Length) currentGoal = 0;
            agent.destination = goals[currentGoal].position;
        }
    }
}
