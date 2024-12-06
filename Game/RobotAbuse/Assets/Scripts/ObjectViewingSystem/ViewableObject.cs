using UnityEngine;

namespace RobotAbuse
{
    public class ViewableObject : MonoBehaviour, IViewableObject, IHighlightable
    {
        [SerializeField] Material HighlightMaterial;
        Material originalMaterial;
        
        [SerializeField] Color HighlightColor = Color.white;
        
        MeshRenderer meshRenderer;


        void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            originalMaterial = meshRenderer.material;

            HighlightMaterial.color = HighlightColor;
        }

        public void Highlight()
        {
            meshRenderer.material = HighlightMaterial;
        }

        public void Unhighlight()
        {
            meshRenderer.material= originalMaterial;

        }
    }
}
