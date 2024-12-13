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

        void Awake()
        {
            //Ensure collider and Rigidbody work properly
            GetComponent<SphereCollider>().isTrigger = true;
            GetComponent<Rigidbody>().useGravity = false;

            SocketOwner = GetComponentInParent<IViewableObject>();

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
        void Start()
        {
            ObjectViewer.OnSocketDetach += ObjectViewer_OnSocketDetach;
            ObjectViewer.OnHideAllSockets += ObjectViewer_OnHideAllSockets;
            ObjectViewer.OnShowAllSockets += ObjectViewer_OnShowAllSockets;
        }

        //Attach Sockets
        void OnTriggerEnter(Collider other)
        {
            var vo = ObjectViewer.DetectedViewableObject as ViewableObject;
            if (other.gameObject.GetComponent<PartSocket>() != null && !other.gameObject.GetComponent<PartSocket>().IsConnected && !IsConnected)
            {
                IsConnected = true;
                AttachedPartSocket = other.gameObject.GetComponent<PartSocket>();
                AttachedPartSocket.IsConnected = true;
                OnSocketPartsConnected?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = this, OtherPartSocket = other.gameObject.GetComponent<PartSocket>() });
                OnSocketPartsConnected?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = other.gameObject.GetComponent<PartSocket>(), OtherPartSocket = this  });
                HideSocket();
            }
        }

        //Detach Sockets
        void ObjectViewer_OnSocketDetach(object sender, EventArgs e)
        {
            var eventArgs = e as OnSocketPartsInteractionEventArgs;
            if (IsConnected && eventArgs.GrabbedPartSocket == this &&  eventArgs.OtherPartSocket.IsConnected)
            {
                IsConnected = false;
                eventArgs.OtherPartSocket.IsConnected = false;
            }
            ShowSocket();
        }

        void ShowSocket()
        {
            var renderer = GetComponent<Renderer>();
            renderer.material.SetFloat("_Opacity", .5f);
        }

        void HideSocket()
        {
            var renderer = GetComponent<Renderer>();
            renderer.material.SetFloat("_Opacity", 0f);
        }

        void ObjectViewer_OnShowAllSockets(object sender, EventArgs e)
        {
            ShowSocket();
        }

        void ObjectViewer_OnHideAllSockets(object sender, EventArgs e)
        {
            HideSocket();
        }

        void OnDisable()
        {
            ObjectViewer.OnSocketDetach -= ObjectViewer_OnSocketDetach;
            ObjectViewer.OnHideAllSockets -= ObjectViewer_OnHideAllSockets;
            ObjectViewer.OnShowAllSockets -= ObjectViewer_OnShowAllSockets;
        }
    }
}