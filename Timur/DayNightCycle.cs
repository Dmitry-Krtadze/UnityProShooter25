using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DayNightCycle : MonoBehaviourPunCallbacks
{
    public float dayLengthInSeconds = 60f;
    private float rotationSpeed;
    private float currentRotationX = 100f;

    void Start()
    {
        rotationSpeed = 360f / dayLengthInSeconds;
        currentRotationX = transform.eulerAngles.x;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            currentRotationX += rotationSpeed * Time.deltaTime;

            if (currentRotationX >= 360f)
                currentRotationX = 0f;

            transform.rotation = Quaternion.Euler(currentRotationX, 0f, 0f);
        }
    }
}
