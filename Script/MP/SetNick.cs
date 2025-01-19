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
            // Устанавливаем никнейм в свойствах игрока
            string nickName = PlayerPrefs.GetString("NickName", "Player");
            PhotonNetwork.LocalPlayer.NickName = nickName;
            SetNickOnField(nickName);
        }
        else
        {
            // Для других игроков отображаем синхронизированный ник
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
        // Проверяем, если обновился никнейм владельца
        if (pnView.Owner == targetPlayer)
        {
            UpdateNick();
        }
    }
}
