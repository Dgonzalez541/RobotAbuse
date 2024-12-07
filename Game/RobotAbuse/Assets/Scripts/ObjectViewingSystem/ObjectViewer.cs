using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Linq;

namespace RobotAbuse
{
    [System.Serializable]
    public class ObjectViewer 
    {
        public GameObject DetectedGameObject { get; private set; }

        public bool IsDragging { get; private set; }

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
                    Debug.Log("Hit" + DetectedGameObject.name);
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
            Debug.Log("Trigger: " + sender);
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
            IsDragging = false;
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