using System;
using UnityEngine;

namespace RobotAbuse
{
    //PartSocket is the class that IViewableObjects can socket into if they implement ISocketable
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PartSocket : MonoBehaviour
    {
        public bool IsConnected { get; private set; } = true;
        public PartSocket AttachedPartSocket { get; private set; }
      
        public IViewableObject SocketOwner { get; private set; }

        public ObjectViewer ObjectViewer;

        public event EventHandler OnSocketPartsConnected;

        private void Awake()
        {
            //Ensure collider and Rigidbody work properly
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
                    OnSocketPartsConnected?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = this, OtherPartSocket = AttachedPartSocket });
                }
            }
        }
        private void Start()
        {
            ObjectViewer.OnSocketDetach += ObjectViewer_OnSocketDetach;
            ObjectViewer.OnHideAllSockets += ObjectViewer_OnHideAllSockets;
        }

        //Attach Sockets
        private void OnTriggerEnter(Collider other)
        {
            var vo = ObjectViewer.DetectedViewableObject as ViewableObject;
            if (other.gameObject.GetComponent<PartSocket>() != null && !other.gameObject.GetComponent<PartSocket>().IsConnected && !IsConnected)
            {
                IsConnected = true;
                AttachedPartSocket = other.gameObject.GetComponent<PartSocket>();
                AttachedPartSocket.IsConnected = true;
                OnSocketPartsConnected?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = this, OtherPartSocket = other.gameObject.GetComponent<PartSocket>() });
                HideSocket();
            }
        }

        //Detach Sockets
        private void ObjectViewer_OnSocketDetach(object sender, EventArgs e)
        {
            var eventArgs = e as OnSocketPartsInteractionEventArgs;
            if (IsConnected && eventArgs.GrabbedPartSocket == this)
            {
                IsConnected = false;
                eventArgs.OtherPartSocket.IsConnected = false;
            }
            ShowSocket();
        }

        private void ShowSocket()
        {
            var renderer = GetComponent<Renderer>();
            renderer.material.SetFloat("_Opacity", .5f);
        }

        private void HideSocket()
        {
            var renderer = GetComponent<Renderer>();
            renderer.material.SetFloat("_Opacity", 0f);
        }

        private void ObjectViewer_OnHideAllSockets(object sender, EventArgs e)
        {
            HideSocket();
        }

        private void OnDisable()
        {
            ObjectViewer.OnSocketDetach -= ObjectViewer_OnSocketDetach;
            ObjectViewer.OnHideAllSockets -= ObjectViewer_OnHideAllSockets;
        }
    }
}