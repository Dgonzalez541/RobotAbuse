using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace RobotAbuse
{
    public class ObjectViewer 
    {
        public bool DetectObject(Ray ray)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var hitObject = hit.transform.gameObject.GetComponent<IViewableObject>();
                if (hitObject != null)
                {
                    Debug.Log("Hit" + hit.transform.gameObject.name);
                    return true;
                }
                else
                {
                    Debug.Log("Hit Invalid Object");
                }
            }
            else
            {
                Debug.Log("Did not Hit");
            }
            return false;
        }
    }
}