using Core.Authoring.SelectGameObjects;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace Core.Authoring.Characters
{
    public sealed class CharacterAuthoring : EntityBehaviour , IPointerClickHandler
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private NavMeshObstacle _agentObstacle;
        [SerializeField] private Transform[] _pivotsHand;
        [SerializeField] private AudioSource _audioSourse;
        
        public Animator Animator => _animator;
        public NavMeshAgent NavMeshAgent => _agent;
        public NavMeshObstacle NavMeshObstacle  => _agentObstacle;
        public Transform[] PivotHand  => _pivotsHand;
        public AudioSource AudioSource  => _audioSourse;

        private const float _duration = 2f;

        public void OnPointerClick(PointerEventData eventData)
        {
            EntityManager.AddComponent<Clicked>(Entity);
        }

        public void TurningCharacterToPoint(Vector3 targetPoint)
        {
            var direction = targetPoint - Transform.position;
            var rotation = Quaternion.LookRotation(direction);
            Transform.rotation = Quaternion.Lerp(Transform.rotation, rotation, _duration * Time.deltaTime);
        }
    }
    
    public struct MoveCharacter : IComponentData
    {
        public float3 TargetPoint;
    }
    
    public struct MoveCharacterCompleted : IComponentData { }
}