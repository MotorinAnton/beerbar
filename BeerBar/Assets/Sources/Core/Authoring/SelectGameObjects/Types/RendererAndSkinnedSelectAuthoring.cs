using UnityEngine;

namespace Core.Authoring.SelectGameObjects.Types
{
    public class RendererAndSkinnedSelectAuthoring : SelectObjectAuthoring
    {
        [SerializeField] private MeshRenderer[] _renderers;
        
        public MeshRenderer[] Renderers => _renderers;
        
        [SerializeField] private SkinnedMeshRenderer _skinned;
        
        public SkinnedMeshRenderer Skinned => _skinned;
        
        public override SelectObjectType SelectType => SelectObjectType.RendererAndSkinned;
    }
}