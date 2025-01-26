using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float enemySpeed = 5f; // Скорость передвижения
    [SerializeField] private GameObject patrolIntoPlayer; // закинуть сюда игрока, чтобы враг следовал за игроком
    [SerializeField] private float EnemyPatrolDistance = 6f;
    private int currentPointIndex = 0; // Индекс текущей точки

    private Transform[] patrolPoints; // Массив точек патруля

    void Start()
    {
        // Если patrolIntoPlayer не присвоен в инспекторе, пытаемся найти объект с тегом "Player"
        if (patrolIntoPlayer == null)
        {
            StartCoroutine(FindPlayerWhenAvailable());
        }

        // Находим все объекты с тегом "PatrolPoint" и сохраняем их позиции
        GameObject[] points = GameObject.FindGameObjectsWithTag("PatrolPoint");
        if (points.Length == 0) return; // Если точек нет, выход из метода

        patrolPoints = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            patrolPoints[i] = points[i].transform;
        }

        // Устанавливаем позицию врага в первую точку патруля
        transform.position = patrolPoints[0].position;
    }

    // Корутину, которая будет искать игрока, если его еще нет
    private IEnumerator FindPlayerWhenAvailable()
    {
        while (patrolIntoPlayer == null)
        {
            patrolIntoPlayer = GameObject.FindGameObjectWithTag("Player");

            if (patrolIntoPlayer != null)
            {
                break; // Если нашли игрока, выходим из цикла
            }

            yield return null; // Ждем 1 кадр и пробуем снова
        }

        if (patrolIntoPlayer == null)
        {
            Debug.LogError("Player object not found! Make sure the player has the 'Player' tag.");
        }
    }

    void Update()
    {
        // Если нет точек, выходим из метода
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Текущая целевая точка
        Transform targetPoint = patrolPoints[currentPointIndex];

        // Проверка на близость к игроку
        if (patrolIntoPlayer != null && (transform.position - patrolIntoPlayer.transform.position).sqrMagnitude < EnemyPatrolDistance * EnemyPatrolDistance)
        {
            // Двигаем врага к игроку
            transform.position = Vector3.MoveTowards(transform.position, patrolIntoPlayer.transform.position, enemySpeed * Time.deltaTime);
        }
        else
        {
            // Двигаем врага к текущей точке патруля
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, enemySpeed * Time.deltaTime);

            // Проверяем, достиг ли враг точки патруля (используем квадрат расстояния для оптимизации)
            if ((transform.position - targetPoint.position).sqrMagnitude < 0.05f * 0.05f) 
            {
                // Переход к следующей точке патруля
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            }
        }
    }
}
