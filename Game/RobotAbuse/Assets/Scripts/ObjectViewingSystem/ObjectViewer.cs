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
                            DetectedGameObject = go.gameObject;
                            IsDragging=true;
                            return true;
                        }
                    }
                }
                
            }
            ClearDetectedObject();
            return false;
        }

        void ClearDetectedObject()
        {
            if(DetectedGameObject != null && DetectedGameObject.GetComponent<IHighlightable>() != null)
            {
                DetectedGameObject.GetComponent<IHighlightable>().Unhighlight();
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

    }
}