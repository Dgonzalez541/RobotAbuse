
using System;
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
    public class ViewableObject : MonoBehaviour, IViewableObject, IHighlightable
    {
        [field: SerializeField] public GameObject[] AdditonalGameObjects { get; set; }

        [SerializeField] AssetReferenceMaterial materialReference;
        public bool IsHighlighted { get; private set; } = false;

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
        }

        public void Highlight()
        {
            meshRenderer.material = highlightMaterial;
            IsHighlighted = true;

            foreach(var go in AdditonalGameObjects) 
            {
                var viewableObject = go.GetComponent<IViewableObject>();
                if(viewableObject != null) 
                {
                    var highlightableObject = go.GetComponent<IHighlightable>();
                    if (highlightableObject != null && highlightableObject.IsHighlighted ==false)
                    {
                        highlightableObject.Highlight();
                    }
                }

            }
        }

        public void Unhighlight()
        {
            meshRenderer.material = originalMaterial;
            IsHighlighted = false;

            foreach(var go in AdditonalGameObjects)
            {
                var viewableObject = go.GetComponent<IViewableObject>();
                if (viewableObject != null)
                {
                    var highlightableObject = go.GetComponent<IHighlightable>();
                    if (highlightableObject != null && highlightableObject.IsHighlighted == true)
                    {
                        highlightableObject.Unhighlight();
                    }
                }
            }
        }
    }
}
