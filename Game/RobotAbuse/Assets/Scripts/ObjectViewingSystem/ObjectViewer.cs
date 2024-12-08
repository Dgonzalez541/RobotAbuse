using UnityEngine;

namespace RobotAbuse
{
    [System.Serializable]
    public class ObjectViewer : MonoBehaviour
    {
        public GameObject DetectedGameObject { get; private set; }
        public IViewableObject DetectedViewableObject { get; private set; }

        public bool IsDragging { get; private set; } = false;

        public bool IsConnectingSocket { get; private set; } = false;

        PartSocket targetPartSocket;

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
                        IsDragging = true;

                        var socketObject = DetectedGameObject.GetComponent<ISocketable>();
                        if(socketObject != null && socketObject.PartSocket != null) 
                        {
                            socketObject.PartSocket.OnTriggerEntered += PartSocket_OnTriggerEntered;
                        }
                    }
                    return true;
                }
                else
                {
                    foreach(var go in DetectedGameObject.GetComponentsInParent<Transform>())
                    {
                        if(go.GetComponent<IViewableObject>() != null) 
                        {
                            
                            DetectedViewableObject = go.GetComponent<IViewableObject>();
                            go.GetComponent<IHighlightable>().Highlight();
                            IsDragging=true;

                            var socketObject = go.GetComponent<ISocketable>();
                            if (socketObject != null && socketObject.PartSocket != null)
                            {
                                socketObject.PartSocket.OnTriggerEntered += PartSocket_OnTriggerEntered;
                            }

                            DetectedGameObject = go.gameObject;
                            return true;
                        }
                    }
                }
                
            }
            ClearDetectedObject();
            return false;
        }

        private void PartSocket_OnTriggerEntered(object sender, System.EventArgs e)
        {
            if (!IsConnectingSocket)
            {
                foreach (var go in DetectedGameObject.GetComponentsInParent<Transform>())
                {
                    if (go.GetComponent<IViewableObject>() != null)
                    {
                        var onPartSocketEventArgs = e as OnPartSocketEnterEventArgs;
                        IsConnectingSocket = true;

                        targetPartSocket = onPartSocketEventArgs.OtherPartSocket;
                        break;
                    }
                }
            }

        }

        void Update()
        {
            if(IsConnectingSocket)
            {
                StopDragging();

                var detectedVo = DetectedViewableObject as ViewableObject;

                var currentGrabbedPartSocketPosition = detectedVo.gameObject.transform.position;
                var connectingSocketPartTargetPosition = targetPartSocket.gameObject.transform.position;

                detectedVo.transform.position = Vector3.Lerp(currentGrabbedPartSocketPosition, connectingSocketPartTargetPosition, 100f * Time.deltaTime);
               
                if (detectedVo.transform.position == currentGrabbedPartSocketPosition)
                {
                    IsConnectingSocket = false;
                }
            }
        }
    

    void ClearDetectedObject()
        {
            if(DetectedGameObject != null && DetectedGameObject.GetComponent<IHighlightable>() != null)
            {
                DetectedGameObject.GetComponent<IHighlightable>().Unhighlight();
            }

            if (DetectedGameObject != null && DetectedGameObject.GetComponent<ISocketable>() != null && DetectedGameObject.GetComponent<ISocketable>().PartSocket != null)
            {
                DetectedGameObject.GetComponent<ISocketable>().PartSocket.OnTriggerEntered -= PartSocket_OnTriggerEntered;
            }

            DetectedGameObject = null;
            StopDragging();
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
                socketObject.PartSocket.OnTriggerEntered -= PartSocket_OnTriggerEntered;
            }
        }
    }
}