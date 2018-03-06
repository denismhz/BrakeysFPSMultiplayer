using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class JoinGame : MonoBehaviour {

    List<GameObject> roomlist = new List<GameObject>();

    private NetworkManager networkManager;

    [SerializeField]
    private GameObject roomListItemPrefab;

    [SerializeField]
    Text status;

    [SerializeField]
    private Transform roomListParent;

    private void Start()
    {
        networkManager = NetworkManager.singleton;
        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        RefreshRoomList();
    }

    public void RefreshRoomList()
    {
        ClearRoomList();
        networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
        status.text = "Loading...";
    }

    public void OnMatchList(bool success, string extendInfo, List<MatchInfoSnapshot> matchList)
    {
        status.text = "";

        if(matchList == null)
        {
            status.text = "Couldn't get room list";
            return;
        }

        ClearRoomList();

        foreach(MatchInfoSnapshot match in matchList)
        {
            GameObject _roomListItemGO = Instantiate(roomListItemPrefab);
            _roomListItemGO.transform.SetParent(roomListParent);

            RoomListItem _roomListItem = _roomListItemGO.GetComponent<RoomListItem>();

            if(_roomListItem != null)
            {
                _roomListItem.Setup(match, JoinRoom);
            }

            roomlist.Add(_roomListItemGO);
        }

        if(roomlist.Count == 0)
        {
            status.text = "No rooms available";
        }
    }

    void ClearRoomList()
    {
        for (int i = 0; i < roomlist.Count; i++)
        {
            Destroy(roomlist[i]);
        }

        roomlist.Clear();
    }

    public void JoinRoom(MatchInfoSnapshot _match)
    {
        networkManager.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        ClearRoomList();
        status.text = "joining...";
    }

}
