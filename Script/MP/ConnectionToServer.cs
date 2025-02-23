using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class ConnectionToServer : MonoBehaviourPunCallbacks
{
    public static ConnectionToServer Instance;

    [SerializeField] private TMP_InputField inputRoomName;
    [SerializeField] private TMP_Text roomName;

    [SerializeField] private Transform transformRoomList;
    [SerializeField] private GameObject roomItemPrefab;



    [SerializeField] private GameObject playerListItem;
    [SerializeField] private Transform transformPlaterList;
    [SerializeField] private GameObject startGameButton;

    public override void OnJoinedRoom()
    {
        WindowManager.Layout.OpenLayout("GameRoom");
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        if (PhotonNetwork.IsMasterClient)
            startGameButton.SetActive(true);
        else startGameButton.SetActive(false);
  

        Player[] players = PhotonNetwork.PlayerList;
        foreach (Transform trns in transformPlaterList)
        {
            Destroy(trns.gameObject);
        }
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItem, transformPlaterList).
                GetComponent<PlayerListItem>().
                SetUp(players[i]);
        }
    }


    public override void OnMasterClientSwitched(
        Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient) startGameButton.
                 SetActive(true);
        else startGameButton.SetActive(false);
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(Transform trns in transformRoomList)
        {
            Destroy(trns.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            Instantiate(roomItemPrefab, transformRoomList).
                GetComponent<RoomItem>().SetUp(roomList[i]);
        }
    }
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
    }



    public void CreateNewRoom()
    {
        if (string.IsNullOrEmpty(inputRoomName.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(inputRoomName.text);
    }

   

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        WindowManager.Layout.OpenLayout("MainMenu");
    }

    
    private void Awake()
    {
        Instance = this;
        PhotonNetwork.ConnectUsingSettings();   
    }



    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); // ALLO
        string randomNickname = "Player" + Random.Range(0, 1000);
        PhotonNetwork.NickName = PlayerPrefs.GetString("NickName", randomNickname); 
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    [SerializeField] TMP_InputField PlayerNickNameField;
    [SerializeField] TMP_Text PrikolNaOtrabotke;
    public void SetNickName()
    {
        PhotonNetwork.NickName = PlayerNickNameField.GetComponent<TMP_InputField>().text;
        PrikolNaOtrabotke.text = PhotonNetwork.NickName;
        PlayerPrefs.SetString("NickName", PhotonNetwork.NickName);

    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItem, transformPlaterList).
            GetComponent<PlayerListItem>().
            SetUp(newPlayer);
    }



    public override void OnJoinedLobby()
    {
        Debug.Log("My abobiki, My na servere !!!");
        //
        WindowManager.Layout.OpenLayout("MainMenu");
        //
    }

    public void ConnectToRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }







    public void StartGameLevel(int levelIndex)
    {
        PhotonNetwork.LoadLevel(levelIndex);
    }
}
