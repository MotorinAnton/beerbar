using Core.Components;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimator :  EntityComponentBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        if (Manager.HasComponent<ApplyRootMotion>(SelfEntity))
        {
            _navMeshAgent.Move(_animator.deltaPosition);
        }
    }
}