using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class killzonescript : MonoBehaviourPunCallbacks
{


private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок вошёл в KillZone");


            string attacker = "KillZone"; // или можно взять из другого источника
            float damage = 999f; // или любое значение, если нужно

            // Вызов RPC на игроке, который вошёл в триггер
            other.GetComponent<PhotonView>().RPC("RPC_KillZone", RpcTarget.All, damage, attacker);
        }
    }
}
