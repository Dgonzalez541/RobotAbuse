using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RobotAbuse
{
    /*ViewableObject is the class that represents parts that can be used by the ObjectViewer using IViewableObject. 
     *Behaviours such as IHighlightable and ISocketable can be added to ViewableObjects.
     */
    [DisallowMultipleComponent]
    public class ViewableObject : MonoBehaviour, IViewableObject, IHighlightable, ISocketable
    {
        //IViewableObject
        [field: SerializeField] public GameObject[] AdditonalGameObjects { get; set; }
        Dictionary<GameObject,Material> dictGameObjectMaterial = new Dictionary<GameObject,Material>();

        //IHighlightable
        public bool IsHighlighted { get; private set; } = false;

        //Implementation details for IHighlightable
        [SerializeField] AssetReferenceMaterial materialReference;
        Material highlightMaterial;
        Material originalMaterial;
        MeshRenderer meshRenderer;

        //ISocketable
        public PartSocket PartSocket { get { return partSocket; } set { partSocket = value; } }
        PartSocket partSocket;

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
    }

    //Class to get Materials from Addressbles
    [System.Serializable]
    public class AssetReferenceMaterial : AssetReferenceT<Material>
    {
        public AssetReferenceMaterial(string guid) : base(guid) { }
    }
}
