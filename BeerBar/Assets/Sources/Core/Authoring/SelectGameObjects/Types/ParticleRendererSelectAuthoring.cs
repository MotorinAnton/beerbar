using UnityEngine;

namespace Core.Authoring.SelectGameObjects.Types
{
    public class ParticleRendererSelectAuthoring : SelectObjectAuthoring
    {
        [SerializeField] private ParticleSystemRenderer _particle;
        
        public ParticleSystemRenderer Particle => _particle;
        
        public override SelectObjectType SelectType => SelectObjectType.Particle;
    }
}