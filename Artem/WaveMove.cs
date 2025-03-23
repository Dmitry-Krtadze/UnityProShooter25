using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveMove : MonoBehaviour
{
    [SerializeField] public GameObject[] points; // Массив точек, между которыми будет двигаться объект
    public float speed = 3f; // Скорость движения

    private int currentPointIndex = 0; // Индекс текущей точки

    void Update()
    {
        if (points.Length == 0) return; // Если точек нет, ничего не делаем

        // Двигаем объект к текущей точке
        transform.position = Vector3.MoveTowards(transform.position, points[currentPointIndex].transform.position, speed * Time.deltaTime);

        // Если объект достиг текущей точки, переходим к следующей
        if (currentPointIndex <= points.Length)
        {
            if (transform.position == points[currentPointIndex].transform.position)
            {
                currentPointIndex = currentPointIndex + 1; // Переход к следующей точке
            }
        }
        else
        {
            currentPointIndex = 0;
        }
    }
}

