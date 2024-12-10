using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;
namespace RobotAbuse
{
    public class OnSocketPartsInteractionEventArgs : EventArgs
    {
        public PartSocket GrabbedPartSocket;
        public PartSocket OtherPartSocket;
    }


    //ObjectViewer implements detection and manipulation interactions for IViewableObjects. 
    [System.Serializable]
    public class ObjectViewer : MonoBehaviour
    {
        public GameObject DetectedGameObject { get; private set; }
        public IViewableObject DetectedViewableObject { get; private set; }

        public bool IsDragging { get; private set; } = false;
        public bool IsConnectingSocket { get; private set; } = false;

        [SerializeField] public TextMeshProUGUI textLabel;

        public event EventHandler OnSocketDetach;
        public event EventHandler OnSocketAttach;
        public event EventHandler OnHideAllSockets;

        private void Awake()
        {
            //Init Part Sockets
            var partSockets = FindObjectsOfType<PartSocket>();
            foreach (var partSocket in partSockets) 
            {
                partSocket.ObjectViewer = this;
            }
        }
        void Update()
        {
            HandleSocketConnectionSnap();
        }

        public bool DetectObject(Ray ray)
        { 
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if(DetectedGameObject != null && hit.transform.gameObject != DetectedGameObject)
                {
                    ClearDetectedObject();
                }

                DetectedGameObject = hit.transform.gameObject;

                var hitObject = DetectedGameObject.GetComponent<IViewableObject>();

                if (hitObject != null)
                {
                    DetectedViewableObject = hitObject;

                    var highlightableObject = DetectedGameObject.GetComponent<IHighlightable>();
                    if (highlightableObject != null)
                    {
                        highlightableObject.Highlight();
                    }
                    return true;
                }
                else //Hit a game object without IViewalbe (IViewable additonal game object)
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

        void ClearDetectedObject()
        {
            if (IsDragging)
            {
                StopDragging();
            }

            if (DetectedGameObject != null && DetectedGameObject.GetComponent<IHighlightable>() != null)
            {
                DetectedGameObject.GetComponent<IHighlightable>().Unhighlight();
            }

            if (DetectedGameObject != null && DetectedGameObject.GetComponent<ISocketable>() != null && DetectedGameObject.GetComponent<ISocketable>().PartSocket != null)
            {
                var detectedPartSocket = DetectedGameObject.GetComponent<ISocketable>().PartSocket;
                detectedPartSocket.OnSocketPartsConnected -= PartSocket_OnSocketsConnected;
                OnSocketDetach?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = detectedPartSocket, OtherPartSocket = detectedPartSocket.AttachedPartSocket });
            }
            OnHideAllSockets?.Invoke(this, EventArgs.Empty);
            DetectedGameObject = null;
        }

        //Handle Socket Connections
        private void PartSocket_OnSocketsConnected(object sender, System.EventArgs e)
        {
            if (!IsConnectingSocket)//Prevent Multiple connections
            {
                IsConnectingSocket = true;
                textLabel.text = "Connected!";
            }
        }

        //Snaps sockets in place
        private void HandleSocketConnectionSnap()
        {
            if (IsConnectingSocket && DetectedGameObject != null && !IsDragging)
            {

                var detectedVo = DetectedViewableObject as ViewableObject;

                var currentGrabbedPartSocketPosition = detectedVo.gameObject.transform.position;
                ISocketable socketable = null;
                if (DetectedGameObject != null)
                {
                    socketable = DetectedGameObject.GetComponentInParent<ISocketable>();
                }
                var connectingSocketPartTargetPosition = socketable.PartSocket.AttachedPartSocket.transform.position; 
                        
                detectedVo.transform.position = Vector3.Lerp(currentGrabbedPartSocketPosition, connectingSocketPartTargetPosition, 100f * Time.deltaTime);

                if (detectedVo.transform.position == currentGrabbedPartSocketPosition)
                {
                    IsConnectingSocket = false;

                    OnSocketAttach?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = socketable.PartSocket, OtherPartSocket = socketable.PartSocket.AttachedPartSocket });
                }
            }
        }

        
        //Dragging Objects
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
}