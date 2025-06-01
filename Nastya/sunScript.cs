using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sunScript : MonoBehaviour
{
    public GameObject missle;
    private Transform target;
    public float speed = 5f;
    public void SpawnPrikol()
    {
        Instantiate(missle, transform.position, Quaternion.identity);
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
            // ������� �������� � ����
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // ��������� ������ � ������� ���� (�� �������)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
    }
}
