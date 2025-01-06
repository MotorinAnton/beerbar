using UnityEngine;

namespace Core.Authoring.SelectGameObjects.Types
{
    public class RendererAndParticleSelectAuthoring : SelectObjectAuthoring
    {
        [SerializeField] private MeshRenderer[] _renderers;
        
        public MeshRenderer[] Renderers => _renderers;
        
        [SerializeField] private ParticleSystemRenderer _paticle;
        
        public ParticleSystemRenderer Particle => _paticle;
        
        public override SelectObjectType SelectType => SelectObjectType.RendererAndParticle;
    }
}