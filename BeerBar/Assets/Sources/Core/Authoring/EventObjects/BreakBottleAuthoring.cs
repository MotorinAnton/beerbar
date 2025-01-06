using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.EventObjects
{
    public class BreakBottleAuthoring : SelectAuthoring<ParticleRendererSelectAuthoring>
    {
        [SerializeField] private ParticleSystem[] _particles;
        
        public ParticleSystem[] Particles => _particles;
    }

    public struct BreakBottleEntity : IComponentData { }
    
    public class BreakBottleView : IComponentData
    {
        public BreakBottleAuthoring Value;
    }
    
    public struct OrderCleanBreakBottle : IBufferElementData
    {
        public Entity BreakBottle;
    }
    
    public struct RandomEventEntity : IComponentData { }
    
}