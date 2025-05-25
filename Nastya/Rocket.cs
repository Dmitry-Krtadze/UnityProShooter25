using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 5f;
    private Transform target;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Плавное движение к цели
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Повернуть ракету в сторону цели (по желанию)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
    }
}
