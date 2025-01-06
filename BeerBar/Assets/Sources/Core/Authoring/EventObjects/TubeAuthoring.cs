using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.EventObjects
{
    public class TubeAuthoring : SelectAuthoring<RendererAndParticleSelectAuthoring>
    {
        [SerializeField] private MeshRenderer[] _progressMeshRenderers;
        
        public MeshRenderer[] ProgressMeshRenderers => _progressMeshRenderers;
        
        [SerializeField] private ParticleSystem[] _particles;
        
        public ParticleSystem[] Particles => _particles;
    }
    public struct Tube : IComponentData { }

    public class TubeView : IComponentData
    {
        public TubeAuthoring Value;
    }
}