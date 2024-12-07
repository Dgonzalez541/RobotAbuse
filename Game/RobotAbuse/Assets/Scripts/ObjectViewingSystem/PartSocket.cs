using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RobotAbuse
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PartSocket : MonoBehaviour
    {
        public event EventHandler OnTriggerEntered;

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.GetComponent<PartSocket>() != null)
            {
                OnTriggerEntered?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}