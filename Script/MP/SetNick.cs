using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
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
            // ������������� ������� � ��������� ������
            string randomNickname = "Player" + Random.Range(0, 1000);
            string nickName = PlayerPrefs.GetString("NickName", randomNickname);
            PhotonNetwork.LocalPlayer.NickName = nickName;
            SetNickOnField(nickName);
        }
        else
        {
            // ��� ������ ������� ���������� ������������������ ���
            UpdateNick();
        }
    }

    private void UpdateNick()
    {
        if (pnView.Owner != null)
        {
            SetNickOnField(pnView.Owner.NickName);
        }
    }

    private void SetNickOnField(string nickName)
    {
        if (NickField != null)
        {
            NickField.text = nickName;
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // ���������, ���� ��������� ������� ���������
        if (pnView.Owner == targetPlayer)
        {
            UpdateNick();
        }
    }
}
