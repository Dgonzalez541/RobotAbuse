using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RobotAbuse
{
    public class OnSocketPartsInteractionEventArgs : EventArgs
    {
        public PartSocket GrabbedPartSocket;
        public PartSocket OtherPartSocket;
    }

    [System.Serializable]
    public class ObjectViewer : MonoBehaviour
    {
        public GameObject DetectedGameObject { get; private set; }

        public IViewableObject DetectedViewableObject { get; private set; }

        PartSocket otherPartSocket; //Cached here until functionality moved out of Update()
        public bool IsDragging { get; private set; } = false;

        public bool IsConnectingSocket { get; private set; } = false;

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

        public bool DetectObject(Ray ray)
        { 
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if(DetectedGameObject != null && hit.transform.gameObject != DetectedGameObject)
                {
                    OnHideAllSockets?.Invoke(this, EventArgs.Empty);
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
                        //OnObjectDetected();
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
                            //OnObjectDetected();
                            var highlightableObject = DetectedGameObject.GetComponent<IHighlightable>();
                            if (highlightableObject != null)
                            {
                                //OnObjectDetected();
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
        }

        private void PartSocket_OnSocketsConnected(object sender, System.EventArgs e)
        {
            if (!IsConnectingSocket)//Prevent Multiple connections
            {
                IsConnectingSocket = true;
                var onPartSocketEventArgs = e as OnSocketPartsInteractionEventArgs;
                otherPartSocket = onPartSocketEventArgs.OtherPartSocket;
            }

        }

        void Update()
        {
            HandleSocketConnectionSnap();
        }

        //Snaps sockets in place
        private void HandleSocketConnectionSnap()
        {
            if (IsConnectingSocket)
            {
                StopDragging();

                var detectedVo = DetectedViewableObject as ViewableObject;

                var currentGrabbedPartSocketPosition = detectedVo.gameObject.transform.position;
                var connectingSocketPartTargetPosition = otherPartSocket.gameObject.transform.position;

                detectedVo.transform.position = Vector3.Lerp(currentGrabbedPartSocketPosition, connectingSocketPartTargetPosition, 100f * Time.deltaTime);

                if (detectedVo.transform.position == currentGrabbedPartSocketPosition)
                {
                    IsConnectingSocket = false;
                    var sockatableVo = DetectedViewableObject as ISocketable;

                    OnSocketAttach?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = sockatableVo.PartSocket, OtherPartSocket = otherPartSocket });
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

            if (DetectedGameObject != null && DetectedGameObject.GetComponent<ISocketable>() != null && DetectedGameObject.GetComponent<ISocketable>().PartSocket != null)
            {
                var detectedPartSocket = DetectedGameObject.GetComponent<ISocketable>().PartSocket;
                detectedPartSocket.OnSocketPartsConnected -= PartSocket_OnSocketsConnected;
                OnSocketDetach?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = detectedPartSocket, OtherPartSocket = detectedPartSocket.AttachedPartSocket });
            }

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
            var socketObject = DetectedGameObject.GetComponent<ISocketable>();
            if (socketObject != null)
            {
                socketObject.PartSocket.OnSocketPartsConnected -= PartSocket_OnSocketsConnected;
            }
        }
    }
}