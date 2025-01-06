using Core.Authoring.Bartenders;
using Core.Authoring.Cleaners;
using Core.Authoring.Customers;
using Core.Authoring.ProductKeepers;
using Core.Authoring.Repairmans;
using Core.Components;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.Characters.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class MoveCharacterSystem : SystemBase
    {
        private EntityQuery _moveCharacterQuery;

        protected override void OnCreate()
        {
            using var moveCharactersBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveCharacterQuery = moveCharactersBuilder.WithAll<MoveCharacter>().WithNone<MoveCharacterCompleted>()
                .Build(this);
        }

        protected override void OnUpdate()
        {
            var moveCharacters = _moveCharacterQuery.ToEntityArray(Allocator.Temp);

            foreach (var characterEntity in moveCharacters)
            {
                var transform = EntityManager.GetComponentObject<TransformView>(characterEntity).Value;
                var animator = EntityManager.GetComponentObject<AnimatorView>(characterEntity).Value;
                var agent = EntityManager.GetComponentObject<NavMeshAgentView>(characterEntity);
                var config = EntityUtilities.GetGameConfig();
                var targetPoint = EntityManager.GetComponentData<MoveCharacter>(characterEntity).TargetPoint;
               
                if (agent.Obstacle != default)
                {
                    agent.Obstacle.enabled = false;
                }

                agent.Agent.enabled = true;
                
                animator.SetBool(CustomerAnimationConstants.Walk, true);
                
                agent.Agent.SetDestination(targetPoint);
                
                if (EntityManager.HasComponent<ProductKeeper>(characterEntity))
                {
                    if (EntityManager.HasComponent<MoveContainerProductKeeper>(characterEntity))
                    {
                        animator.SetBool(ProductKeeperAnimationConstants.Walk, false);
                        animator.SetBool(ProductKeeperAnimationConstants.WalkWithBox, true);
                    }
                    var productKeeperData = EntityManager.GetComponentObject<ProductKeeperDataComponent>(characterEntity);
                    animator.SetFloat(ProductKeeperAnimationConstants.WalkSpeed.Value, productKeeperData.Value.WalkAnimatorSpeed);
                    agent.Agent.speed = config.ProductKeeperConfig.MoveSpeed;
                }

                if (EntityManager.HasComponent<Cleaner>(characterEntity))
                {
                    if (EntityManager.HasComponent<MopInHands>(characterEntity))
                    {
                        animator.SetBool(CleanerAnimationConstants.Walk, false);
                        animator.SetBool(CleanerAnimationConstants.WalkMop, true);
                    }
                    
                    var cleanerData = EntityManager.GetComponentObject<CleanerDataComponent>(characterEntity);
                    
                    animator.SetFloat(CleanerAnimationConstants.WalkSpeed.Value, cleanerData.Value.WalkAnimatorSpeed);
                    agent.Agent.speed = config.CleanerConfig.MoveSpeed;
                }


                if (EntityManager.HasComponent<Barman>(characterEntity))
                {
                    var barmanView = EntityManager.GetComponentObject<BarmanView>(characterEntity);

                    if (barmanView.Value.PivotHand[0].gameObject.activeInHierarchy)
                    {
                        animator.SetBool(BarmanAnimationConstants.Walk, false);
                        animator.SetBool(BarmanAnimationConstants.WalkBottle, true);
                    }
                    
                    var barmanData = EntityManager.GetComponentObject<BarmanDataComponent>(characterEntity);
                    
                    animator.SetFloat(BarmanAnimationConstants.WalkSpeed.Value, barmanData.Value.WalkAnimatorSpeed);
                    agent.Agent.speed = config.BarmanConfig.MoveSpeed;
                }
                
                if (EntityManager.HasComponent<Customer>(characterEntity))
                {
                    var customerData = EntityManager.GetComponentObject<CustomerConfigDataComponent>(characterEntity);
                    animator.SetFloat(CustomerAnimationConstants.WalkSpeed.Value,  customerData.Value.Visual.WalkAnimatorSpeed);
                    agent.Agent.speed = customerData.Value.Speed;
                }
                
                if (EntityManager.HasComponent<Repairman>(characterEntity))
                {
                    var repairmanData = EntityManager.GetComponentObject<RepairmanDataComponent>(characterEntity);
                    animator.SetFloat(RepairmanAnimationConstants.WalkSpeed.Value,  repairmanData.Value.WalkAnimatorSpeed);
                    agent.Agent.speed = config.RepairmanConfig.MoveSpeed;
                }


                if (!EntityUtilities.CheckItInPosition(transform, targetPoint))
                {
                    continue;
                }
                
                
                if (agent.Agent.hasPath)
                {
                    agent.Agent.ResetPath();
                }
                

                if (EntityManager.HasComponent<ProductKeeper>(characterEntity))
                {
                    if (EntityManager.HasComponent<MoveContainerProductKeeper>(characterEntity))
                    {
                        animator.SetBool(ProductKeeperAnimationConstants.WalkWithBox, false);
                    }
                }

                if (EntityManager.HasComponent<Cleaner>(characterEntity))
                {
                    if (EntityManager.HasComponent<MopInHands>(characterEntity))
                    {
                        animator.SetBool(CleanerAnimationConstants.WalkMop, false);
                    }
                }

                if (EntityManager.HasComponent<Barman>(characterEntity))
                {
                    var barmanView = EntityManager.GetComponentObject<BarmanView>(characterEntity);

                    if (barmanView.Value.PivotHand[0].gameObject.activeInHierarchy)
                    {
                        animator.SetBool(BarmanAnimationConstants.WalkBottle, false);
                    }
                }
                
                animator.SetBool(CustomerAnimationConstants.Walk, false);
                
                EntityManager.AddComponent<MoveCharacterCompleted>(characterEntity);
                EntityManager.RemoveComponent<MoveCharacter>(characterEntity);
                
            }
        }
    }
}