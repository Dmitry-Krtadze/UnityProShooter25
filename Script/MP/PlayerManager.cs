using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
public class PlayerManager : MonoBehaviourPunCallbacks
{
    private PhotonView ALLO;
    private GameObject controller;
    private void Awake()
    {
        ALLO = GetComponent<PhotonView>();
    }
    private void Start()
    {
        if (ALLO.IsMine)
        {
            CreateController();
        }
    }
    private void CreateController()
    {
        controller = PhotonNetwork.Instantiate(Path.Combine(
             "PlayerController"), Vector3.zero,
             Quaternion.identity, 0, new object[] {ALLO.ViewID });
    }
    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();
    }
}
