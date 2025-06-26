using UnityEngine;
using Photon.Pun;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView view = other.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.pnView.RPC("RPC_Damage", RpcTarget.All, 999f, "MapFall", true); // 999 урона, "MapFall" Ч им€ убийцы
                }
            }
        }
    }
}
