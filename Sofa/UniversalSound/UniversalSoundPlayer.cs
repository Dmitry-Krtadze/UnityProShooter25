using System.Collections;
using UnityEngine;
using Photon.Pun;

public class UniversalSoundPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    private AudioSource audioSource;
    private PhotonView photonView;
    [SerializeField] private bool debugEnable = false;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        photonView = GetComponent<PhotonView>();

        if (audioSource == null)
        {
            Debug.LogError("[UniversalSoundPlayer] AudioSource �� ������!", gameObject);
        }
        if (photonView == null)
        {
            Debug.LogError("[UniversalSoundPlayer] PhotonView �� ������!", gameObject);
        }
    }

    public void PlaySound(bool sync, AudioClip clip)
    {
        if (clip == null)
        {   
            Debug.LogError("[UniversalSoundPlayer] ������� ������ AudioClip!");
            return;
        }

        if (debugEnable) Debug.Log($"[UniversalSoundPlayer] PlaySound ������. Sync: {sync}, IsMine: {photonView.IsMine}");

        if (sync && photonView.IsMine)
        {
            string clipName = clip.name;
            photonView.RPC("RPC_PlaySound", RpcTarget.All, clipName);
        }
        else
        {
            PlayLocalSound(clip);
        }
    }

    private void PlayLocalSound(AudioClip clip)
    {
        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("[UniversalSoundPlayer] AudioSource == null!", gameObject);
        }
    }

    [PunRPC]
    private void RPC_PlaySound(string clipName)
    {
        if (debugEnable)  Debug.Log($"[UniversalSoundPlayer] RPC_PlaySound ������ � {clipName}");
        AudioClip clip = Resources.Load<AudioClip>($"Sounds/{clipName}");
        if (clip != null)
        {
            PlayLocalSound(clip);
        }
        else
        {
            Debug.LogError($"[UniversalSoundPlayer] ���� '{clipName}' �� ������ � Resources/Sounds!");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // ������ �� ��������, ��� ��� ���� ���������� ����� RPC
    }
}
