using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour
{
    [SerializeField] private Camera myCamera;

    private void Awake()
    {
        // Если камера не задана в инспекторе, ищем её среди детей PlayerController
        if (myCamera == null)
        {
            myCamera = GetComponentInParent<Camera>();
        }

        // В качестве альтернативы можно указать главную камеру, если она фиксирована
        if (myCamera == null)
        {
            myCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (myCamera != null)
        {
            // Ориентируем объект на камеру
            Vector3 direction = transform.position - myCamera.transform.position;
            transform.LookAt(transform.position + direction);

            // Если требуется, добавляем поворот на 180 градусов
            transform.Rotate(Vector3.up * 180);
        }
    }
}
