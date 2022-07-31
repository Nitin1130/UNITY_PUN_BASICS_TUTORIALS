using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Unity.VisualScripting;
using UnityEngine;


namespace MultiplayerDemo
{
    /// <summary>
    /// Player Manager
    /// Handles fire Input and Beams
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        #region IPunObservable Implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
            }
            else
            {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion
        #region Private Fields

        [Tooltip("The Beams GameObject to control")] [SerializeField]
        private GameObject beams;
        // True, when the user is firing
        private bool IsFiring;
        
        
        #if UNITY_5_4_OR_NEWER
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
        #endif
        #endregion

        #region Public Fields
    
        
        [Tooltip("The current Health of our player")]
        public float Health = 1f;
        
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;
        
        #endregion

        #region Monobehaviour CallBacks

        /// <summary>
        /// Monobehaviour method called on GameObjet by Unity during early initialization phase
        /// </summary>
        private void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
            if (beams == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }
        }
    
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            
            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo =  Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage ("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
            
            CameraWork cameraWork = this.gameObject.GetComponent<CameraWork>();


            if (cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }
            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif
        }
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            if (photonView.IsMine)
            {
                if (photonView.IsMine)
                {
                    ProcessInputs();
                }

                if (Health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();
                }
            }
            // trigger beams active state
            if (beams != null && IsFiring != beams.activeInHierarchy)
            {
                beams.SetActive(IsFiring);
            }
        }

        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player.
        /// </summary>\
        void OnTriggerEnter(Collider other)
        {

            if (!photonView.IsMine)
            {
                return;
            }

            if (!other.name.Contains("Beam"))
            {
                return;
            }

            Health -= 0.1f;
        }

        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are touching the player
        /// </summary>
        /// <param name="other">Other.</param>
        void OnTriggerStay(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (!other.name.Contains("Beam"))
            {
                return;
            }

            Health -= 0.1f * Time.deltaTime;
        }
        
        #if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
        #endif


        void CalledOnLevelWasLoaded(int level)
        {
            GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
        }
        #endregion
        
        #if UNITY_5_4_OR_NEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable ();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endif

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!IsFiring)
                {
                    IsFiring = true;
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                if (IsFiring)
                {
                    IsFiring = false;
                }
            }
        }

        #endregion

    }
}
