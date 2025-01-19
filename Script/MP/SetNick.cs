using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class SetNick : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text NickField;
    private PhotonView pnView;
    private void Awake()
    {
        pnView = GetComponentInParent<PhotonView>();
    }
    private void Start()
    {
        NickField = GetComponent<TMP_Text>();
        if (pnView.IsMine)
        {
        NickField.text = PlayerPrefs.GetString("NickName");
        }
        
    }
}
