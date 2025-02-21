using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private PlayerController player;
    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player.gameObject)
            return;
        player.GroundState(true);


    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player.gameObject)
            return;
        player.GroundState(false);

    }
}
