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
            HideSocket();
            
        }
        void Start()
        {
            //Anything needing the ObjectViewer needs to be done in Start() to ensure ObjectViewer is setup before needing it.
            ObjectViewer.OnSocketDetach += ObjectViewer_OnSocketDetach;
            ObjectViewer.OnHideAllSockets += ObjectViewer_OnHideAllSockets;
            ObjectViewer.OnShowAllSockets += ObjectViewer_OnShowAllSockets;
            ObjectViewer.OnCheckSocketConnection += ObjectViewer_OnCheckSocketConnection;

            SocketOwner = GetComponentInParent<IViewableObject>();

            //Find already attached sockets at start
            var colliders = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius);
            foreach (var collider in colliders)
            {
                if (collider.gameObject.GetComponent<PartSocket>() != null && collider.gameObject.GetComponent<PartSocket>() != this)
                {
                    var attachedPartSocket = collider.gameObject.GetComponent<PartSocket>();
                    SetSocketConnection(attachedPartSocket);
                }
            }
        }

        void ObjectViewer_OnCheckSocketConnection(object sender, EventArgs e)
        {
            //Find already attached sockets at start
            var colliders = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius);
            foreach (var collider in colliders)
            {
                if (!IsConnected 
                    && collider.gameObject.GetComponent<PartSocket>() != null 
                    && collider.gameObject.GetComponent<PartSocket>() != this 
                    && !collider.gameObject.GetComponent<PartSocket>().IsConnected)
                {
                    var attachedPartSocket = collider.gameObject.GetComponent<PartSocket>();
                    SetSocketConnection(attachedPartSocket);
                }
            }
        }

        void SetSocketConnection(PartSocket newAttachedPartSocket)
        {
            IsConnected = true;
            AttachedPartSocket = newAttachedPartSocket;
            AttachedPartSocket.IsConnected = true;
            OnSocketPartsConnected?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = this, OtherPartSocket = AttachedPartSocket });
            OnSocketPartsConnected?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = AttachedPartSocket, OtherPartSocket = this });
        }

        //Attach Sockets
        void OnTriggerEnter(Collider other)
        {
            if (!IsConnected 
                && other.gameObject.GetComponent<PartSocket>() != null 
                && !other.gameObject.GetComponent<PartSocket>().IsConnected)
            {
                var attachedPartSocket = other.gameObject.GetComponent<PartSocket>();
                SetSocketConnection(attachedPartSocket);
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