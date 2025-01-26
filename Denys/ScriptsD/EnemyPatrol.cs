using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float enemySpeed = 10f; // Скорость передвижения
    [SerializeField] private GameObject patrolIntoPlayer; // закинуть сюда игрока, чтобы враг следовал за игроком
    private int currentPointIndex = 0; // Индекс текущей точки

    private Transform[] patrolPoints; // Массив точек патруля

    void Start()
    {
        // Если patrolIntoPlayer не присвоен в инспекторе, ищем объект с тегом "Player"
        if (patrolIntoPlayer == null)
        {
            patrolIntoPlayer = GameObject.FindGameObjectWithTag("Player");
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

    void Update()
    {
        // Если нет точек, выходим из метода
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Текущая целевая точка
        Transform targetPoint = patrolPoints[currentPointIndex];

        // Проверка на близость к игроку
        if (patrolIntoPlayer != null && (transform.position - patrolIntoPlayer.transform.position).sqrMagnitude < 5 * 5)
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
