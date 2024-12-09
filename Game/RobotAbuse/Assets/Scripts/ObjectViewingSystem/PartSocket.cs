using System;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;

namespace RobotAbuse
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PartSocket : MonoBehaviour
    {
        public event EventHandler OnSocketPartsConnected;

        public PartSocket AttachedPartSocket { get; private set; }

        public ObjectViewer ObjectViewer;

        public bool IsConnected { get; private set; } = true;

        public IViewableObject SocketOwner { get; private set; }
        

        private void Awake()
        {
            GetComponent<SphereCollider>().isTrigger = true;
            GetComponent<Rigidbody>().useGravity = false;

            SocketOwner = GetComponentInParent<IViewableObject>();

            ObjectViewer = GameObject.FindObjectOfType<ObjectViewer>();

            HideSocket();

            //Find already attached sockets at start
            var colliders = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius);
            foreach (var collider in colliders)
            {
                if(collider.gameObject.GetComponent<PartSocket>() != null && collider.gameObject.GetComponent<PartSocket>() != this)
                {
                    IsConnected = true;
                    AttachedPartSocket = collider.gameObject.GetComponent<PartSocket>();
                }
            }
        }
        private void Start()
        {
            ObjectViewer.OnSocketDetach += ObjectViewer_OnSocketDetach;
            ObjectViewer.OnSocketAttach += ObjectViewer_OnSocketAttach;
            ObjectViewer.OnHideAllSockets += ObjectViewer_OnHideAllSockets;
        }

        private void ObjectViewer_OnHideAllSockets(object sender, EventArgs e)
        {
            HideSocket();
        }

        private void ObjectViewer_OnSocketDetach(object sender, EventArgs e)
        {
            var eventArgs = e as OnSocketPartsInteractionEventArgs;
            if (IsConnected && eventArgs.OtherPartSocket == this)
            {  
                IsConnected = false;
            }
                ShowSocket();
        }

        private void ShowSocket()
        {
            var renderer = GetComponent<Renderer>();
            renderer.material.SetFloat("_Opacity", .5f);
        }

        private void ObjectViewer_OnSocketAttach(object sender, EventArgs e)
        {
            var eventArgs = e as OnSocketPartsInteractionEventArgs;
            if (IsConnected || eventArgs.OtherPartSocket == this)
            {
                IsConnected = true;
            }

            HideSocket();
        }

        private void HideSocket()
        {
            var renderer = GetComponent<Renderer>();
            renderer.material.SetFloat("_Opacity", 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.GetComponent<PartSocket>() != null)
            {
                IsConnected = true;
                AttachedPartSocket = other.gameObject.GetComponent<PartSocket>();
                OnSocketPartsConnected?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = this, OtherPartSocket = other.gameObject.GetComponent<PartSocket>() }); ;
            }
        }

        private void OnDisable()
        {
            ObjectViewer.OnSocketDetach -= ObjectViewer_OnSocketDetach;
            ObjectViewer.OnSocketAttach -= ObjectViewer_OnSocketAttach;
        }
    }
}