using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DayNightCycle : MonoBehaviourPun
{
    [Header("Настройки")]
    public float dayLengthInSeconds = 60f;

    private float rotationSpeed;
    private PhotonView view;

    void Start()
    {
        rotationSpeed = 360f / dayLengthInSeconds;
        view = GetComponent<PhotonView>(); // получаем компонент PhotonView
    }

    void Update()
    {
        if (view != null && view.IsMine)
        {
            transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
        }
    }
}
