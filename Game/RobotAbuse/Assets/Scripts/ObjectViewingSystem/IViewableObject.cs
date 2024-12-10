using UnityEngine;

namespace RobotAbuse
{
    //An interface for objects to be used by the ObjectViewer
    public interface IViewableObject
    {
         public GameObject[] AdditonalGameObjects { get; }
    }
}
