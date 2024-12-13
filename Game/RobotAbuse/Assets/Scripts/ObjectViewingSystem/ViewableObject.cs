using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RobotAbuse
{
    //ViewableObject implements IViewableObject to be useable by ObejctViewer.
    //ViewableObject can add interactions and features by implenting IHighlightable and ISocketable
    [DisallowMultipleComponent]
    public class ViewableObject : ViewableObjectBase, IHighlightable, ISocketable
    {
        //IHighlightable
        public bool IsHighlighted { get; private set; } = false;

        [SerializeField] AssetReferenceMaterial materialReference;
        Dictionary<GameObject,Material> dictGameObjectMaterial = new Dictionary<GameObject,Material>();
        Material highlightMaterial;
        Material originalMaterial;
        MeshRenderer meshRenderer;

        //ISockatable
        public PartSocket PartSocket { get { return partSocket; } set { partSocket = value; } }
        PartSocket partSocket;

        void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            originalMaterial = meshRenderer.material;

            //Get materials from Addressables
            Addressables.LoadAssetsAsync<Material>(materialReference, (material) => 
            {
                highlightMaterial = material;   
            });

            //Get original materials from game objects
            foreach(GameObject go in AdditonalGameObjects)
            {
                dictGameObjectMaterial.Add(go, go.GetComponent<MeshRenderer>().material);
            }

            partSocket = GetComponentInChildren<PartSocket>();
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
}
