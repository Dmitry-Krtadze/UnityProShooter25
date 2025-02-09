using System.Collections;
using UnityEngine;
using Photon.Pun;

public class UniversalSoundPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    private AudioSource audioSource;
    private PhotonView photonView;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        photonView = GetComponent<PhotonView>();

        if (audioSource == null)
        {
            Debug.LogError("[UniversalSoundPlayer] AudioSource не найден!", gameObject);
        }
        if (photonView == null)
        {
            Debug.LogError("[UniversalSoundPlayer] PhotonView не найден!", gameObject);
        }
    }

    public void PlaySound(bool sync)
    {
        Debug.Log($"[UniversalSoundPlayer] PlaySound вызван. Sync: {sync}, IsMine: {photonView.IsMine}");

        if (sync && photonView != null && photonView.IsMine)
        {
            photonView.RPC("RPC_PlaySound", RpcTarget.All);
        }
        else
        {
            PlayLocalSound();
        }
    }

    private void PlayLocalSound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogError("[UniversalSoundPlayer] Попытка воспроизвести звук, но AudioSource == null!", gameObject);
        }
    }

    [PunRPC]
    private void RPC_PlaySound()
    {
        Debug.Log($"[UniversalSoundPlayer] RPC_PlaySound вызван на {PhotonNetwork.NickName}");
        PlayLocalSound();
    }

    // Интерфейс IPunObservable нужен для корректной работы с PhotonView
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Здесь ничего не передаем, так как звук вызывается через RPC
    }
}
