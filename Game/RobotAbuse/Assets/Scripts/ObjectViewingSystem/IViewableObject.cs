using UnityEngine;

namespace RobotAbuse
{
    //Allows objects to be used by ObjectViewer
    public interface IViewableObject
    {
         public GameObject[] AdditonalGameObjects { get; }
    }
}
