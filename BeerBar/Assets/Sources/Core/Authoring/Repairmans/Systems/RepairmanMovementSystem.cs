using Core.Authoring.Banks;
using Core.Authoring.Characters;
using Core.Authoring.EventObjects;
using Core.Authoring.MovementArrows;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.ProfitUi;
using Core.Authoring.Tables;
using Core.Authoring.TVs;
using Core.Components;
using Core.Components.Wait;
using Core.Constants;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Repairmans.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class RepairmanMovementSystem : SystemBase
    {
        private EntityQuery _freeRepairmanQuery;
        private EntityQuery _moveRepairsRepairmanQuery;
        private EntityQuery _repairsRepairmanQuery;
        private EntityQuery _afterRepairsRepairmanQuery;
        private EntityQuery _moveExitRepairmanQuery;
        private EntityQuery _repairPointsQuery;
        private EntityQuery _spawnPointRepairmanQuery;
        private EntityQuery _bankQuery;

        protected override void OnCreate()
        {
            using var freeRepairmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _freeRepairmanQuery = freeRepairmanBuilder.WithAll<Repairman, FreeRepairman>().Build(this);
            
            using var moveRepairsRepairmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveRepairsRepairmanQuery = moveRepairsRepairmanBuilder.WithAll<Repairman, MoveRepairsRepairman>().WithNone<MoveCharacter>().Build(this);
            
            using var repairsRepairmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _repairsRepairmanQuery = repairsRepairmanBuilder.WithAll<Repairman, RepairsRepairman, WaitTime>().Build(this);
            
            using var afterRepairsRepairmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterRepairsRepairmanQuery = afterRepairsRepairmanBuilder.WithAll<Repairman, RepairsRepairman>().WithNone<WaitTime>().Build(this);
            
            using var moveExitRepairmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveExitRepairmanQuery = moveExitRepairmanBuilder.WithAll<Repairman, MoveExitRepairman>().WithNone<MoveCharacter>().Build(this);
            
            using var repairPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _repairPointsQuery = repairPointsBuilder.WithAll<RepairPoints>().Build(this);
            
            using var spawnPointRepairmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointRepairmanQuery = spawnPointRepairmanBuilder.WithAll<SpawnPointRepairman>().Build(this);
            
            using var bankBuilder = new EntityQueryBuilder(Allocator.Temp);
            _bankQuery = bankBuilder.WithAllRW<Bank>().Build(this);
        }

        protected override void OnUpdate()
        {
            MoveRepairsRepairman();
            RepairsRepairman();
            MoveExitRepairman();
            FreeRepairman();
            AfterRepairsRepairman();
        }
        
        private void FreeRepairman()
        {
            var freeRepairmanArray = _freeRepairmanQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var repairmanEntity in freeRepairmanArray)
            {
                var orders = EntityManager.GetComponentObject<OrderRepairman>(repairmanEntity).RepairObjectList;
                var animator = EntityManager.GetComponentObject<AnimatorView>(repairmanEntity).Value;
                animator.SetBool(RepairmanAnimationConstants.Walk, false);
                
                if (orders.Count == 0)
                {
                    return;
                }
                
                var bank = _bankQuery.GetSingleton<Bank>();

                if (bank.Coins < 10) 
                {
                    return;
                }
                
                EntityManager.AddComponent<MoveRepairsRepairman>(repairmanEntity);
                EntityManager.RemoveComponent<FreeRepairman>(repairmanEntity);
            }
        }

        private void MoveRepairsRepairman()
        {
            var moveRepairsRepairmanArray = _moveRepairsRepairmanQuery.ToEntityArray(Allocator.Temp);

            foreach (var repairmanEntity in moveRepairsRepairmanArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(repairmanEntity).Value;
                var orders = EntityManager.GetComponentObject<OrderRepairman>(repairmanEntity).RepairObjectList;
                var repairPoints = _repairPointsQuery.ToComponentDataArray<RepairPoints>(Allocator.Temp)[0];
                var orderPoint = new Vector3();
                var animationName = "";

                if (EntityManager.HasComponent<TableView>(orders[0]))
                {
                    orderPoint = EntityManager.GetComponentObject<TableView>(orders[0]).CleanTablePoint.Position;
                    animationName = RepairmanAnimationConstants.WorkerWaterRepair.Value;
                }

                if (EntityManager.HasComponent<Electricity>(orders[0]))
                {
                    orderPoint = repairPoints.Electricity;
                    animationName = RepairmanAnimationConstants.RepairElectric.Value;
                }

                if (EntityManager.HasComponent<Tube>(orders[0]))
                {
                    orderPoint = repairPoints.Tube;
                    animationName = RepairmanAnimationConstants.WorkerWaterRepair.Value;
                }

                if (EntityManager.HasComponent<TVView>(orders[0]))
                {
                    orderPoint = repairPoints.TV;
                    animationName = RepairmanAnimationConstants.WorkerWaterRepair.Value;

                }

                if (EntityManager.HasComponent<MoveCharacterCompleted>(repairmanEntity))
                {
                    EntityManager.GetComponentObject<RepairmanView>(repairmanEntity).Value.PivotHand[0].gameObject
                        .SetActive(true);
                    EntityManager.AddComponent<RepairsRepairman>(repairmanEntity);
                    
                    EntityManager.AddComponentData(repairmanEntity,
                        new WaitTime { Current = AnimationUtilities.AnimationLength(animator, animationName) });
                    EntityManager.RemoveComponent<MoveRepairsRepairman>(repairmanEntity);
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(repairmanEntity);
                }
                else
                {
                    EntityManager.AddComponentData(repairmanEntity, new MoveCharacter { TargetPoint = orderPoint });
                }
            }
        }
        
        private void MoveExitRepairman()
        {
            var moveExitRepairmanArray = _moveExitRepairmanQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var repairmanEntity in moveExitRepairmanArray)
            {
                if (EntityManager.HasComponent<MoveCharacterCompleted>(repairmanEntity))
                {
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(repairmanEntity);
                    EntityManager.RemoveComponent<MoveExitRepairman>(repairmanEntity);
                    EntityManager.AddComponent<FreeRepairman>(repairmanEntity);
                }
                else
                {
                    var spawnPointRepairman =
                        _spawnPointRepairmanQuery.ToComponentDataArray<SpawnPointRepairman>(Allocator.Temp)[0];
                    EntityManager.AddComponentData(repairmanEntity,
                        new MoveCharacter { TargetPoint = spawnPointRepairman.Position });
                }
            }
        }
        
        private void AfterRepairsRepairman()
        {
            var repairmanEntityArray = _afterRepairsRepairmanQuery.ToEntityArray(Allocator.Temp);

            foreach (var repairmanEntity in repairmanEntityArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(repairmanEntity).Value;
                animator.SetBool(RepairmanAnimationConstants.TubeRepair, false);
                
                EntityManager.GetComponentObject<RepairmanView>(repairmanEntity).Value.PivotHand[0].gameObject.SetActive(false);
                
                var repairmanPosition = EntityManager.GetComponentObject<TransformView>(repairmanEntity).Value.position;
                
                SpawnProfitUi(repairmanPosition);

                var orders = EntityManager.GetComponentObject<OrderRepairman>(repairmanEntity).RepairObjectList;
                
                if (EntityManager.HasComponent<TableView>(orders[0]))
                {
                    var tableView = EntityManager.GetComponentObject<TableView>(orders[0]);

                    foreach (var pointEntity in tableView.AtTablePointsEntity)
                    {
                        if (EntityManager.HasComponent<PointDirtTable>(pointEntity))
                        {
                            EntityManager.RemoveComponent<PointDirtTable>(pointEntity);
                        }
                    }
                }
                
                if (EntityManager.HasComponent<Tube>(orders[0]))
                {
                    var tubeView = EntityManager.GetComponentObject<TubeView>(orders[0]).Value;
                    
                    foreach (var particle in tubeView.Particles)
                    {
                        particle.Stop(); ;
                    }
                    
                    if (EntityManager.HasComponent<WaitTime>(orders[0]))
                    {
                        var progress = tubeView.ProgressMeshRenderers[0].material.GetFloat(BreakdownObjectConstants.PipeLeak);
                        var waitTime = EntityManager.GetComponentData<WaitTime>(orders[0]);
                        var startWaitTime = EntityManager.GetComponentData<StartWaitTime>(orders[0]).Start;
                        waitTime.Current = progress * startWaitTime;
                        EntityManager.SetComponentData(orders[0], waitTime);
                    }
                    else
                    {
                        EntityManager.AddComponentData(orders[0], new WaitTime { Current = BreakdownObjectConstants.FlowTime });
                    }
                }
                
                if (EntityManager.HasComponent<Electricity>(orders[0]))
                {
                    animator.SetBool(RepairmanAnimationConstants.ElectricityRepair, false);

                    var electricityView = EntityManager.GetComponentObject<ElectricityView>(orders[0]).Value;
                    
                    foreach (var particle in electricityView.Particles)
                    {
                        particle.Stop(); ;
                    }
                    
                    var closeDoorAnimation = electricityView.Animation.GetClip(BreakdownObjectConstants.ElectricalDoorClose.Value);
                    electricityView.Animation.clip = closeDoorAnimation;
                    electricityView.Animation.Play();
                    electricityView.TweenFinished();
                }
                
                if (EntityManager.HasComponent<TVView>(orders[0]))
                {
                    var tvView = EntityManager.GetComponentObject<TVView>(orders[0]).Value.OnRenderer;
                    tvView.gameObject.SetActive(true);
                }

                var repairArrow = EntityManager.GetComponentObject<RepairMovementArrowView>(orders[0]).Arrow;
                repairArrow.DisableArrow();
                
                EntityManager.RemoveComponent<Breakdown>(orders[0]);
                
                orders.Remove(orders[0]);

                if (orders.Count > 0)
                {
                    EntityManager.AddComponent<MoveRepairsRepairman>(repairmanEntity);
                }
                else
                {
                    EntityManager.AddComponent<MoveExitRepairman>(repairmanEntity);
                }

                EntityManager.RemoveComponent<RepairsRepairman>(repairmanEntity);
            }
        }
        
        private void RepairsRepairman()
        {
            var repairsRepairmanArray = _repairsRepairmanQuery.ToEntityArray(Allocator.Temp);

            foreach (var repairmanEntity in repairsRepairmanArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(repairmanEntity).Value;
                var repairmanView = EntityManager.GetComponentObject<RepairmanView>(repairmanEntity).Value;
                var orders = EntityManager.GetComponentObject<OrderRepairman>(repairmanEntity);
                var orderEntity = orders.RepairObjectList[0];
                var targetRotationPoint = new Vector3();
                
                if (EntityManager.HasComponent<TVView>(orderEntity))
                {
                    targetRotationPoint = EntityManager.GetComponentObject<TVView>(orderEntity).Value
                        .transform.position;
                    animator.SetBool(RepairmanAnimationConstants.TubeRepair, true);
                }

                if (EntityManager.HasComponent<TableView>(orderEntity))
                {
                    targetRotationPoint = EntityManager.GetComponentObject<TableView>(orderEntity).Value
                        .transform.position;
                    animator.SetBool(RepairmanAnimationConstants.TubeRepair, true);
                }

                if (EntityManager.HasComponent<TubeView>(orderEntity))
                {
                    targetRotationPoint =
                        EntityManager.GetComponentObject<TubeView>(orderEntity).Value.transform.position;
                    animator.SetBool(RepairmanAnimationConstants.TubeRepair, true);
                }

                if (EntityManager.HasComponent<ElectricityView>(orderEntity))
                {
                    targetRotationPoint =
                        EntityManager.GetComponentObject<ElectricityView>(orderEntity).Value.transform.position;
                    
                    animator.SetBool(RepairmanAnimationConstants.ElectricityRepair, true);

                    if (!EntityManager.HasComponent<TweenProcessing>(orderEntity))
                    {
                        var flashLightSequence = DOTween.Sequence();
                        var intensityTween = RenderSettings.sun.DOIntensity(BreakdownObjectConstants.IntensityEndValue,
                                BreakdownObjectConstants.IntensityTweenDuration).SetEase(Ease.InOutElastic)
                            .SetLoops(BreakdownObjectConstants.IntensityTweenLoops)
                            .SetDelay(BreakdownObjectConstants.IntensityEndValue);
                        var intensityToNormalTween = RenderSettings.sun
                            .DOIntensity(1f, BreakdownObjectConstants.IntensityTweenToNormalDuration)
                            .SetEase(Ease.InOutElastic)
                            .SetLoops(BreakdownObjectConstants.IntensityTweenToNormalLoops);
                        flashLightSequence.Append(intensityTween);
                        flashLightSequence.Append(intensityToNormalTween);
                        EntityManager.AddComponent<TweenProcessing>(orderEntity);
                    }
                }
                
                repairmanView.TurningCharacterToPoint(targetRotationPoint);
            }
        }

        private void SpawnProfitUi(Vector3 repairmanPosition)
        {
            var profitUiPosition = repairmanPosition;

            profitUiPosition.y += RepairmanAnimationConstants.ProfitOffsetY;

            var spawnProfitUiEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(spawnProfitUiEntity,
                new SpawnProfitUi
                {
                    Profit = false,
                    Point = profitUiPosition,
                    Text = "-" + 10
                });

            var bank = _bankQuery.GetSingleton<Bank>();
            bank.Coins -= 10;
            _bankQuery.SetSingleton(bank);
        }
    }
}