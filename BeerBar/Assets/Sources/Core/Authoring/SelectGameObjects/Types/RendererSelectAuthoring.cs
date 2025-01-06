using UnityEngine;

namespace Core.Authoring.SelectGameObjects.Types
{
    public class RendererSelectAuthoring : SelectObjectAuthoring
    {
        [SerializeField] private MeshRenderer[] _renderers;
        
        public MeshRenderer[] Renderers => _renderers;
        
        public override SelectObjectType SelectType => SelectObjectType.Renderer;
    }
}