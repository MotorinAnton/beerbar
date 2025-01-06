using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Bartenders.AddBarmanFX
{
    public sealed class AddBarmanFXAuthoring : SelectAuthoring<RendererSelectAuthoring>
    {
        // [SerializeField] private ParticleSystem[] _particles;
        //
        // public ParticleSystem[] Particles => _particles;
    }

    public class AddBarmanFXView : IComponentData
    {
        public AddBarmanFXAuthoring Value;
    }
    
    public struct AddBarmanFX : IComponentData { }
}