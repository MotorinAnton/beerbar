using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.EventObjects
{
    public class ElectricityAuthoring : SelectAuthoring<RendererAndSkinnedSelectAuthoring>
    {
        [SerializeField] private Animation _animation;
        
        public Animation Animation => _animation;
        
        [SerializeField] private ParticleSystem[] _particles;
        
        public ParticleSystem[] Particles => _particles;
    }
    
    public struct Electricity : IComponentData { }

    public class ElectricityView : IComponentData
    {
        public ElectricityAuthoring Value;
    }
}