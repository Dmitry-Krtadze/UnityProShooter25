using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour
{

    [SerializeField] private Camera myCamera;
    private void Awake()
    {
        myCamera = GetComponentInParent<Camera>();
    }

    private void LateUpdate()
    {
        transform.LookAt(myCamera.transform.position);
        transform.Rotate(Vector3.up * 180);
    }
}
