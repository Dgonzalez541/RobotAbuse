
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

    public class ViewableObject : MonoBehaviour, IViewableObject, IHighlightable
    {
        [SerializeField] AssetReferenceMaterial materialReference;

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
        }

        public void Unhighlight()
        {

            meshRenderer.material = originalMaterial;
        }
    }
}
