
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RobotAbuse
{
    [System.Serializable]
    public class AssetReferenceMaterial : AssetReferenceT<Material>
    {
        public AssetReferenceMaterial(string guid) : base(guid) {}
    }

    [DisallowMultipleComponent]
    public class ViewableObject : MonoBehaviour, IViewableObject, IHighlightable, ISocketable
    {
        [field: SerializeField] public GameObject[] AdditonalGameObjects { get; set; }
        Dictionary<GameObject,Material> dictGameObjectMaterial = new Dictionary<GameObject,Material>();

        [SerializeField] AssetReferenceMaterial materialReference;
        public bool IsHighlighted { get; private set; } = false;

        public PartSocket PartSocket { get { return partSocket; } set { partSocket = value; } }
        PartSocket partSocket;

        Material highlightMaterial;
        Material originalMaterial;
        MeshRenderer meshRenderer;

        void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            originalMaterial = meshRenderer.material;
            Addressables.LoadAssetsAsync<Material>(materialReference, (material) => 
            {
                highlightMaterial = material;   
            });

            foreach(GameObject go in AdditonalGameObjects)
            {
                dictGameObjectMaterial.Add(go, go.GetComponent<MeshRenderer>().material);
            }

            partSocket = GetComponentInChildren<PartSocket>();
            if(partSocket != null)
            {
                partSocket.OnSocketPartsConnected += PartSocket_OnTriggerEntered;
            }

        }

        private void PartSocket_OnTriggerEntered(object sender, EventArgs e)
        {
        }

        public void Highlight()
        {
            meshRenderer.material = highlightMaterial;
            IsHighlighted = true;

            foreach(var go in AdditonalGameObjects) 
            {
                go.GetComponent<MeshRenderer>().material = highlightMaterial;
            }
        }

        public void Unhighlight()
        {
            meshRenderer.material = originalMaterial;
            IsHighlighted = false;

            foreach (var go in AdditonalGameObjects)
            {
                Material addtionalGoOriginalMat;
                if(dictGameObjectMaterial.TryGetValue(go, out addtionalGoOriginalMat))
                {
                    go.GetComponent<MeshRenderer>().material = addtionalGoOriginalMat;
                }
                
            }
        }

        private void OnDisable()
        {
            if (partSocket != null)
            {
                partSocket.OnSocketPartsConnected -= PartSocket_OnTriggerEntered;
            }
        }
    }
}
