using System;
using System.Diagnostics.Tracing;
using UnityEngine;
using static Codice.Client.Common.EventTracking.TrackFeatureUseEvent.Features.DesktopGUI.Filters;

namespace RobotAbuse
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PartSocket : MonoBehaviour
    {
        public PartSocket AttachedPartSocket { get; private set; }
        public bool IsConnected { get; private set; } = true;

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
            ObjectViewer.OnSocketAttach += ObjectViewer_OnSocketAttach;
            ObjectViewer.OnHideAllSockets += ObjectViewer_OnHideAllSockets;
        }

        private void OnTriggerEnter(Collider other)
        {
            var vo = ObjectViewer.DetectedViewableObject as ViewableObject;
            if (other.gameObject.GetComponent<PartSocket>() != null && !other.gameObject.GetComponent<PartSocket>().IsConnected && !IsConnected)
            {
                Debug.Log(gameObject + " is CONN");
                IsConnected = true;
                AttachedPartSocket = other.gameObject.GetComponent<PartSocket>();
                AttachedPartSocket.IsConnected = true;
                OnSocketPartsConnected?.Invoke(this, new OnSocketPartsInteractionEventArgs { GrabbedPartSocket = this, OtherPartSocket = other.gameObject.GetComponent<PartSocket>() });
                HideSocket();
            }
        }

        private void ObjectViewer_OnSocketAttach(object sender, EventArgs e)
        {
            //HideSocket();
        }

        private void ObjectViewer_OnSocketDetach(object sender, EventArgs e)
        {
            var eventArgs = e as OnSocketPartsInteractionEventArgs;
            if (IsConnected && eventArgs.GrabbedPartSocket == this)
            {
                Debug.Log(gameObject + " is DIScon");
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
            ObjectViewer.OnSocketAttach -= ObjectViewer_OnSocketAttach;
            ObjectViewer.OnHideAllSockets -= ObjectViewer_OnHideAllSockets;
        }
    }
}