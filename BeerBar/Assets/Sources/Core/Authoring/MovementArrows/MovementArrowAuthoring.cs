using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.MovementArrows
{
    public class MovementArrowAuthoring : EntityBehaviour
    {
        [SerializeField] private ParticleSystem[] _particles;

        public ParticleSystem[] Particles => _particles;


        public void EnableArrow() => gameObject.SetActive(true);
        public void DisableArrow() => gameObject.SetActive(false);
    }
    
    public struct MovementArrow : IComponentData { }
    
    public class ClearMovementArrowView : IComponentData
    {
        public MovementArrowAuthoring Arrow;
    }
    public class RepairMovementArrowView : IComponentData
    {
        public MovementArrowAuthoring Arrow;
    }
}