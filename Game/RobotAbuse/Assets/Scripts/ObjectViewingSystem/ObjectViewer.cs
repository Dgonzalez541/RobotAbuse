using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace RobotAbuse
{
    [System.Serializable]
    public class ObjectViewer 
    {
        public GameObject DetectedGameObject { get; private set; }
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
                    }
                    return true;
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
                DetectedGameObject = null;
            }   
        }

    }
}