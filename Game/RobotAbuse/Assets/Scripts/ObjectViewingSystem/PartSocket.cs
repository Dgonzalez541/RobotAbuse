using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RobotAbuse
{
    public class OnPartSocketEnterEventArgs : EventArgs
    {
        public Collider Other;
        public PartSocket OtherPartSocket;
    }

    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PartSocket : MonoBehaviour
    {
        public event EventHandler OnTriggerEntered;
        public SphereCollider SphereCollider { get { return sphereCollider; }  }
        [SerializeField] SphereCollider sphereCollider;

        public IViewableObject SocketOwner { get; private set; }

        private void Awake()
        {
            GetComponent<SphereCollider>().isTrigger = true;
            GetComponent<Rigidbody>().useGravity = false;

            SocketOwner = GetComponentInParent<IViewableObject>();

        }

        private void OnTriggerEnter(Collider other)
        {
            
            if(other.gameObject.GetComponent<PartSocket>() != null)
            {
                OnTriggerEntered?.Invoke(this, new OnPartSocketEnterEventArgs { Other = other, OtherPartSocket = other.gameObject.GetComponent<PartSocket>() }); ;
            }
        }

    }
}