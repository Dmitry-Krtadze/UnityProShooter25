using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveMove : MonoBehaviour
{
    [SerializeField] public GameObject[] points; // ������ �����, ����� �������� ����� ��������� ������
    public float speed = 3f; // �������� ��������

    private int currentPointIndex = 0; // ������ ������� �����

    void Update()
    {
        if (points.Length == 0) return; // ���� ����� ���, ������ �� ������

        // ������� ������ � ������� �����
        transform.position = Vector3.MoveTowards(transform.position, points[currentPointIndex].transform.position, speed * Time.deltaTime);

        // ���� ������ ������ ������� �����, ��������� � ���������
        if (currentPointIndex <= points.Length)
        {
            if (transform.position == points[currentPointIndex].transform.position)
            {
                currentPointIndex = currentPointIndex + 1; // ������� � ��������� �����
            }
        }
        else
        {
            currentPointIndex = 0;
        }
    }
}

