using System;
using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;
namespace RobotAbuse
{
    //ObjectViewer is the class that handles the selection and manipulation of objects that implement IViewableObject
    [System.Serializable]
    public class ObjectViewer : MonoBehaviour
    {
        public GameObject DetectedGameObject { get; private set; }

        public IViewableObject DetectedViewableObject { get; private set; }

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
                if(DetectedGameObject != hit.transform.gameObject)
                {
                    ClearDetectedObject();
                }

                DetectedGameObject = hit.transform.gameObject;

                DetectedViewableObject = DetectedGameObject.GetComponent<IViewableObject>();
                if (DetectedViewableObject != null)
                {

                    var highlightableObject = DetectedGameObject.GetComponent<IHighlightable>();
                    if (highlightableObject != null)
                    {
                        highlightableObject.Highlight();
                    }

                    return true;
                }
                else //Hit an IViewableObject's Additional Mesh
                {   
                    DetectedGameObject = hit.transform.gameObject;
                    //Find parent with IViewableObject
                    foreach (var go in DetectedGameObject.GetComponentsInParent<Transform>())
                    {
                        if(go.GetComponent<IViewableObject>() != null) 
                        {
                            DetectedGameObject = go.gameObject;
                            DetectedViewableObject = go.GetComponent<IViewableObject>();

                            var highlightableObject = DetectedGameObject.GetComponent<IHighlightable>();
                            if (highlightableObject != null)
                            {
                                highlightableObject.Highlight();
                            }

                            return true;
                        }
                    }
                }
                
            }

            ClearDetectedObject();
            return false;
        }

        public void OnObjectSelection()
        {
            //Start dragging object
            IsDragging = true;

            //Handle Highlighting
            var detectedVO = DetectedViewableObject as ViewableObject;
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

            if(otherPartSocket != null && otherPartSocket.IsConnected) 
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
            if (IsConnectingSocket && DetectedGameObject != null)
            {
                StopDragging();

                var detectedVo = DetectedViewableObject as ViewableObject;
                var currentGrabbedPartSocketPosition = detectedVo.gameObject.transform.position;
                var connectingSocketPartTargetPosition = DetectedGameObject.GetComponentInParent<ISocketable>().PartSocket.AttachedPartSocket.transform.position; 
                
                //Move Sockets to each other
                detectedVo.transform.position = Vector3.Lerp(currentGrabbedPartSocketPosition, connectingSocketPartTargetPosition, 1000f * Time.deltaTime);

                //Attach sockets 
                if (detectedVo.transform.position == currentGrabbedPartSocketPosition)
                {
                    IsConnectingSocket = false;
                    var sockatableVo = DetectedViewableObject as ISocketable;

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
            
            if (DetectedGameObject != null && DetectedGameObject.GetComponent<IHighlightable>() != null)
            {
                DetectedGameObject.GetComponent<IHighlightable>().Unhighlight();
            }

            OnHideAllSockets?.Invoke(this, EventArgs.Empty);
            DetectedGameObject = null;
        }

        public void DragObject(Vector3 currentMousePosition)
        {
            DetectedGameObject.transform.position = currentMousePosition;
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
            if (DetectedGameObject != null && DetectedGameObject.GetComponent<ISocketable>() != null)
            {
                var socketObject = DetectedGameObject.GetComponent<ISocketable>();
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