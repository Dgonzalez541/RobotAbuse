using UnityEngine;

namespace RobotAbuse
{
    //Base class for ViewableObjects that are to be extended with interfaces.
    public abstract class ViewableObjectBase : MonoBehaviour, IViewableObject
    {
        [field: SerializeField] public GameObject[] AdditonalGameObjects { get; set; }
    }
}
