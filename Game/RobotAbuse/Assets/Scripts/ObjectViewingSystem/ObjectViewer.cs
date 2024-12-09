using System;
using UnityEngine;
using TMPro;
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

        [Header("UI Text Label")]
        [SerializeField] public TextMeshProUGUI textLabel;

        private void Awake()
        {
            //Init Part Sockets
            var partSockets = FindObjectsOfType<PartSocket>();
            foreach (var partSocket in partSockets) 
            {
                partSocket.ObjectViewer = this;
            }
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

        public void OnObjectDetected()
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

            OnSocketDetach?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = socketObject.PartSocket, OtherPartSocket = otherPartSocket});
            textLabel.text = "Disconnected!";
        }

        private void PartSocket_OnSocketsConnected(object sender, System.EventArgs e)
        {
            if (!IsConnectingSocket)//Prevent Multiple connections
            {
                IsConnectingSocket = true;
                textLabel.text = "Connected!";
            }
            
        }

        void Update()
        {
            HandleSocketConnectionSnap();
        }

        //Snaps sockets in place
        private void HandleSocketConnectionSnap()
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

        public void StopDragging()
        {
            IsDragging = false;
        }

        public void OnDisable()
        {
            if (DetectedGameObject != null && DetectedGameObject.GetComponent<ISocketable>() != null)
            {
                var socketObject = DetectedGameObject.GetComponent<ISocketable>();
                socketObject.PartSocket.OnSocketPartsConnected -= PartSocket_OnSocketsConnected;
            }
        }
    }

    public class OnSocketPartsInteractionEventArgs : EventArgs
    {
        public PartSocket GrabbedPartSocket;
        public PartSocket OtherPartSocket;
    }
}