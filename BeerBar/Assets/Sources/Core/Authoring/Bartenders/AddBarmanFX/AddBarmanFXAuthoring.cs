using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Core.Configs;
using Unity.Entities;

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
        public Up UpData;
    }
    
    public struct AddBarmanFX : IComponentData { }
    
}