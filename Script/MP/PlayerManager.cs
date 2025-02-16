using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
public class PlayerManager : MonoBehaviourPunCallbacks
{
    private PhotonView ALLO;
    private GameObject controller;

    public SpawnerManager SpawnerM;
    private void Awake()
    {
        ALLO = GetComponent<PhotonView>();
    }
    private void Start()
    {
        SpawnerM = GameObject.Find("SpawnerManager").GetComponent<SpawnerManager>();
        if (ALLO.IsMine)
        {
            CreateController();
        }
    }
    private void CreateController()
    {
        int randSPPos = Random.Range(0, SpawnerM.spawnPoints.Length);
        Vector3 randSpawn = SpawnerM.spawnPoints[randSPPos].position;


        controller = PhotonNetwork.Instantiate(Path.Combine(
             "PlayerController"), randSpawn,
             Quaternion.identity, 0, new object[] {ALLO.ViewID });
    }
    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();
    }
}
