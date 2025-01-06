using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Core.Components
{
    public class AnimatorView : IComponentData
    {
        public Animator Value;
    }
    
    public struct RandomAnimation : IComponentData
    {
        public bool TransitionNextAnimation;
        public float NumberAnimation;
        public float NextTimeTransition;
    }
    
    public class NavMeshAgentView : IComponentData
    {
        public NavMeshAgent Agent;
        public NavMeshObstacle Obstacle;
    }

    public class TransformView : IComponentData
    {
        public Transform Value;
    }
    public class EntityBehaviourView  : ICleanupComponentData
    {
        public EntityBehaviour Value;
    }
    
    public class AudioSourceView  : IComponentData
    {
        public AudioSource Value;
    }
    
    public struct ApplyRootMotion : IComponentData { }
}