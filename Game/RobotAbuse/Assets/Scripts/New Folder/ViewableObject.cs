using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotAbuse
{
    public class ViewableObject : MonoBehaviour, IViewableObject
    {
        [SerializeField] public Mesh[] Meshs { get; private set; }

    }
}
