using Core.Authoring.MovementArrows;
using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.TVs
{
    public sealed class TVAuthoring : SelectAuthoring<RendererSelectAuthoring>
    {
        [SerializeField] private Animator  _animator;
        public Animator Animator => _animator;

        [SerializeField] private Material[]  _chanel;
        public Material[] Chanel => _chanel;


        [SerializeField] private MeshRenderer _onRenderer;
        
        public MeshRenderer OnRenderer => _onRenderer;
        
        
        [SerializeField] private AudioSource _audioSourse;
        
        public AudioSource AudioSource  => _audioSourse;
        
    }
    
    public class SpawnTV : IComponentData
    {
        public TVAuthoring TVPrefab;
        public MovementArrowAuthoring RepairArrow;
    }
    
    public struct TVEntity : IComponentData { }

    public class TVView : IComponentData
    {
        public TVAuthoring Value;
        public int Chanal;
    }
}