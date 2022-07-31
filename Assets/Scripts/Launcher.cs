using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;


namespace MultiplayerDemo
{
     public class Launcher : MonoBehaviourPunCallbacks
     {
          #region Private Serializable Fields

          [Tooltip(
               "The maximum number of players per room, When a room is full, it can't be joined by new players, and so new room will be created")]
          [SerializeField]
          private byte maxPlayersPerRoom = 4;

          [Tooltip("The Ui Panel to let the user enter name, connect and play")] 
          [SerializeField] private GameObject controlPanel;

          [Tooltip("The Ui Label to inform the user that the connection is in progress")] 
          [SerializeField] private GameObject progressLabel;
          #endregion

          #region Private Fields

          /// <summary>
          ///  Keep track of the current process. Since connection is asynchronous ans is based on several callbacks from Photon,
          ///  we need to keep track of this to properly adjust the behaviour when we receive call back by Photon.
          /// Typically this is used for the OnConnectedToMaster() callback.
          /// </summary>
          private bool isConnecting;
          
          /// <summary>
          /// This client's version number. Users are separated from each other by gameVersion(which allows you to make breaking changes).
          /// </summary>
          private string gameVersion = "1";

          #endregion

          #region MonoBehaviour CallBacks

          /// <summary>
          /// MonoBehaviour method called on GameObject by Unity during early initilization phase
          /// </summary>
          private void Awake()
          {
               // #Critical
               // this makes sure we can use PhotonNetwork.LevelLoad() on the master client and all clients in the same room sync their level automatically
               PhotonNetwork.AutomaticallySyncScene = true;
          }

          /// <summary>
          /// Monobehaviour method called on GameObject by Unity during initialization phase.
          /// </summary>
          private void Start()
          {
               progressLabel.SetActive(false);
               controlPanel.SetActive(true);
          }

          #endregion

          #region Public Methods

          /// <summary>
          /// Start the connection process.
          /// If already connected, we attempt joining a random room
          /// If not yet connected, Connect this applicaton instance to Photon Cloud Network
          /// </summary>
          public void Connect()
          {
               
               progressLabel.SetActive(true);
               controlPanel.SetActive(false);
               if (PhotonNetwork.IsConnected)
               {
                    // #Critical we need at this point to attempt joining a Random room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                    PhotonNetwork.JoinRandomRoom();
               }
               else
               {
                    // #Critical, we must first and foremost connect to Photon online server.
                    isConnecting = PhotonNetwork.ConnectUsingSettings();
                    PhotonNetwork.GameVersion = gameVersion;
               }
          }

          #endregion
          
          #region MonoBehaviourPunCallbacks Callbacks

          public override void OnConnectedToMaster()
          {
               Debug.Log("Pun Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
               // #Critical: The first we try to do is to join a potential existing room. If there  is, good ,else, we'll be called back with OnJoinRandoomFailed();
               if (isConnecting)
               {
                    PhotonNetwork.JoinRandomRoom();
                    isConnecting = false;
               }
          }
     
          public override void OnDisconnected(DisconnectCause cause)
          {
               progressLabel.SetActive(false);
               controlPanel.SetActive(true);
               isConnecting = false;
               Debug.Log("Pun basics tutorial/launcher: OnDisconnected() was called by PUN");
          }

          public override void OnJoinRandomFailed(short returnCode, string message)
          {
               Debug.Log(
                    "PUN Basics Tutorial/Launcher: OnJoinRandomFailed() was called by PUN. No random room available, " +
                    "so we create one .\nCalling: PhotonNetwork.CreateRoom");
               
               // # Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new Room
               PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = maxPlayersPerRoom});
          }

          public override void OnJoinedRoom()
          { 
               Debug.Log("PUN Basicws Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
               // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene
               if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
               {
                    Debug.Log("We load the 'Room for 1' ");
                    
                    // #Critical
                    // Load the Room Level.
                    PhotonNetwork.LoadLevel("Room for 1");
               }
          }
          
          

          #endregion
          
          
     }

     
}