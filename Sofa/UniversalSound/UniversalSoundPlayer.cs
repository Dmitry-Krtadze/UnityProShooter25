using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class UniversalSoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private PhotonView photonView;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        photonView = GetComponent<PhotonView>();
    }

    public void PlaySound(bool sync)
    {
        if (sync && photonView != null && photonView.IsMine)
        {
            photonView.RPC("RPC_PlaySound", RpcTarget.All);
        }
        else
        {
            audioSource.Play();
        }
    }

    [PunRPC]
    private void RPC_PlaySound()
    {
        audioSource.Play();
    }
}
