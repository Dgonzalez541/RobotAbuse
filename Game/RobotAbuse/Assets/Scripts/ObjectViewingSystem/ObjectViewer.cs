using System;
using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;
using System.Linq;
namespace RobotAbuse
{
    //ObjectViewer is the class that handles the selection and manipulation of objects that implement IViewableObject
    [System.Serializable]
    public class ObjectViewer : MonoBehaviour
    {
        public GameObject SelectedGameObject { get; private set; }

        public IViewableObject SelectedViewableObject { get; private set; }

        public bool IsDragging { get; private set; } = false;

        public bool IsConnectingSocket { get; private set; } = false;

        public event EventHandler OnSocketDetach;
        public event EventHandler OnSocketAttach;
        public event EventHandler OnHideAllSockets;
        public event EventHandler OnShowAllSockets;
        public event EventHandler OnCheckSocketConnection;

        [Header("UI Text Label")]
        [SerializeField] public TextMeshProUGUI textLabel;

        [Header("Socket Click Sound")]
        [SerializeField] AssetReferenceAudioClip assetReferenceAudioClip;
        AudioSource clickAudioSource;
        AudioClip clickAudioClip;

        PlayerController playerController;
        
        void Awake()
        {
            //Init Part Sockets
            var partSockets = FindObjectsOfType<PartSocket>();
            foreach (var partSocket in partSockets) 
            {
                partSocket.ObjectViewer = this;
            }

            //Get audio clip from Addressables
            Addressables.LoadAssetsAsync<AudioClip>(assetReferenceAudioClip, (audioClip) =>
            {
                clickAudioClip = audioClip;
                clickAudioSource = GetComponent<AudioSource>();
                clickAudioSource.clip = clickAudioClip;
            });

            playerController = GetComponent<PlayerController>();
            playerController.OnFireCanceledEvent += PlayerController_OnFireCanceledEvent;
        }

        public bool DetectObject(Ray ray)
        { 
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))//Detect if mouse hovered object is IViewableObject
            {
                if(SelectedGameObject != hit.transform.gameObject)
                {
                    ClearDetectedObject();
                }

                SelectedGameObject = hit.transform.gameObject;
                if (SelectedGameObject.GetComponent<IViewableObject>() != null)
                {
                    SetSelectedObject(SelectedGameObject.transform);
                    return true;
                }
                else //Hit an IViewableObject's Additional Mesh, but not the IViewableObject itself.
                {   
                    SelectedGameObject = hit.transform.gameObject;
                    //Find parent with IViewableObject
                    foreach (var go in SelectedGameObject.GetComponentsInParent<Transform>())
                    {
                        if(go.GetComponent<IViewableObject>() != null)
                        {
                            SetSelectedObject(go);
                            return true;
                        }
                    }
                }
            }

            ClearDetectedObject();
            return false;
        }

        private void SetSelectedObject(Transform detectedTransform)
        {
            //Set detected objects
            SelectedGameObject = detectedTransform.gameObject;
            SelectedViewableObject = detectedTransform.GetComponent<IViewableObject>();

            var highlightableObject = SelectedGameObject.GetComponent<IHighlightable>();
            if (highlightableObject != null)
            {
                highlightableObject.Highlight();
            }
        }

        public void OnObjectSelection()
        {
            //Start dragging object
            IsDragging = true;

            //Handle Highlighting
            var detectedVO = SelectedViewableObject as ViewableObject;
            detectedVO.GetComponent<IHighlightable>().Highlight();

            //Handle Sockets
            var socketObject = detectedVO.gameObject.GetComponent<ISocketable>();
            if (socketObject != null && socketObject.PartSocket != null)
            {
                socketObject.PartSocket.OnSocketPartsConnected += PartSocket_OnSocketsConnected;
            }

            PartSocket otherPartSocket = null;
            if(socketObject.PartSocket != null)
            {
                otherPartSocket = socketObject.PartSocket.AttachedPartSocket;
            }

            //Disconnect if other socket is connected and selected object is not the root.
            if (otherPartSocket != null && otherPartSocket.IsConnected && SelectedGameObject.GetComponentsInParent<IViewableObject>().Count() > 1) 
            {
                textLabel.text = "Disconnected!";
                clickAudioSource.Play(0);
                OnSocketDetach?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = socketObject.PartSocket, OtherPartSocket = otherPartSocket });
            }

            OnShowAllSockets?.Invoke(this, EventArgs.Empty);
        }

        void PartSocket_OnSocketsConnected(object sender, System.EventArgs e)
        {
            if (!IsConnectingSocket)//Prevent Multiple connections
            {
                IsConnectingSocket = true;
                textLabel.text = "Connected!";

                clickAudioSource.Play(0);
            }
        }

        void Update()
        {
            HandleSocketConnectionSnap();
        }

        //Snaps sockets in place
        void HandleSocketConnectionSnap()
        {
            if (IsConnectingSocket && SelectedGameObject != null && SelectedViewableObject != null)
            {
                StopDragging();

                var detectedVo = SelectedViewableObject as ViewableObject;
                var currentGrabbedPartSocketPosition = detectedVo.gameObject.transform.position;
                var connectingSocketPartTargetPosition = SelectedGameObject.GetComponentInParent<ISocketable>().PartSocket.AttachedPartSocket.transform.position;

                if (SelectedGameObject.GetComponentsInParent<IViewableObject>().Count() > 1) //Check if not root object
                {
                    //Move Sockets to each other
                    detectedVo.transform.position = Vector3.Lerp(currentGrabbedPartSocketPosition, connectingSocketPartTargetPosition, 1000f * Time.deltaTime);
                }
                //Attach sockets 
                if (detectedVo.transform.position == currentGrabbedPartSocketPosition)
                {
                    IsConnectingSocket = false;
                    var sockatableVo = SelectedViewableObject as ISocketable;

                    OnSocketAttach?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = sockatableVo.PartSocket, OtherPartSocket = sockatableVo.PartSocket.AttachedPartSocket });
                }
            }
        }

        void ClearDetectedObject()
        {
            if(IsDragging) 
            {
                StopDragging();
            }
            
            if (SelectedGameObject != null && SelectedGameObject.GetComponent<IHighlightable>() != null)
            {
                SelectedGameObject.GetComponent<IHighlightable>().Unhighlight();
            }

            OnHideAllSockets?.Invoke(this, EventArgs.Empty);
            SelectedGameObject = null;
            SelectedViewableObject = null;
        }

        public void DragObject(Vector3 currentMousePosition)
        {
            SelectedGameObject.transform.position = currentMousePosition;
        }

        void PlayerController_OnFireCanceledEvent(object sender, EventArgs e)
        {
            OnCheckSocketConnection?.Invoke(this, EventArgs.Empty);
            StopDragging();
        }

        void StopDragging()
        {
            IsDragging = false;
        }

        void OnDisable()
        {
            if (SelectedGameObject != null && SelectedGameObject.GetComponent<ISocketable>() != null)
            {
                var socketObject = SelectedGameObject.GetComponent<ISocketable>();
                socketObject.PartSocket.OnSocketPartsConnected -= PartSocket_OnSocketsConnected;
            }

            playerController.OnFireCanceledEvent -= PlayerController_OnFireCanceledEvent;
        }
    }

    public class OnSocketPartsInteractionEventArgs : EventArgs
    {
        public PartSocket GrabbedPartSocket;
        public PartSocket OtherPartSocket;
    }
}