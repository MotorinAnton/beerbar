using System.Linq;
using Core.Authoring.Characters;
using Core.Authoring.CleanerDoor;
using Core.Authoring.EventObjects;
using Core.Authoring.MovementArrows;
using Core.Authoring.Tables;
using Core.Components;
using Core.Components.Destroyed;
using Core.Components.Wait;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Cleaners.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class CleanerMovementSystem : SystemBase
    {
        private EntityQuery _freeCleanerQuery;
        private EntityQuery _moveCleaningTableQuery;
        private EntityQuery _moveCleaningBreakBottleQuery;
        private EntityQuery _cleaningTableCleaner;
        private EntityQuery _afterCleaningTableCleaner;
        private EntityQuery _afterCleaningBreakBottleCleaner;
        private EntityQuery _cleaningCompletedCleaner;
        private EntityQuery _moveExitCleanerQuery;
        private EntityQuery _spawnPointCleanerQuery;
        private EntityQuery _tablePointQuery;
        private EntityQuery _cleanerMopPointQuery;
        private EntityQuery _cleanerDoorQuery;

        protected override void OnCreate()
        {
            using var freeCleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _freeCleanerQuery = freeCleanerBuilder.WithAll<Cleaner, FreeCleaner>().Build(this);

            using var moveClearTableBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveCleaningTableQuery = moveClearTableBuilder.WithAll<Cleaner, CleanTableCleaner>()
                .WithNone<WaitTime, MoveCharacter, CleaningTableCleaner>().Build(this);

            using var moveCleaningBreakBottleBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveCleaningBreakBottleQuery = moveCleaningBreakBottleBuilder
                .WithAll<Cleaner, CleanBreakBottleCleaner>()
                .WithNone<WaitTime, MoveCharacter, CleaningBreakBottleCleaner>().Build(this);

            using var cleaningTableCleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _cleaningTableCleaner = cleaningTableCleanerBuilder
                .WithAll<Cleaner, CleanTableCleaner, WaitTime>().Build(this);

            using var afterCleaningTableBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterCleaningTableCleaner = afterCleaningTableBuilder
                .WithAll<Cleaner, CleaningTableCleaner>().WithNone<WaitTime>().Build(this);

            using var afterCleaningBreakBottleBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterCleaningBreakBottleCleaner = afterCleaningBreakBottleBuilder
                .WithAll<Cleaner, CleaningBreakBottleCleaner>().WithNone<WaitTime>().Build(this);

            using var cleanCompletedCleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _cleaningCompletedCleaner = cleanCompletedCleanerBuilder.WithAll<Cleaner, CleaningCompletedCleaner>()
                .Build(this);

            using var moveExitCleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveExitCleanerQuery = moveExitCleanerBuilder.WithAll<Cleaner, MoveExitCleaner>()
                .WithNone<MoveCharacter>().Build(this);

            using var spawnPointCleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointCleanerQuery = spawnPointCleanerBuilder.WithAll<SpawnPointCleaner>().Build(this);

            using var tableBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tablePointQuery = tableBuilder.WithAll<AtTablePoint>()
                .Build(this);

            using var cleanerMopPointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _cleanerMopPointQuery = cleanerMopPointBuilder.WithAll<CleanerMopPoint>()
                .Build(this);
            
            using var cleanerDoorBuilder = new EntityQueryBuilder(Allocator.Temp);
            _cleanerDoorQuery = cleanerDoorBuilder.WithAll<DoorCleaner, CleanerDoorView>().WithNone<CleanerDoorIsOpen>()
                .Build(this);
        }

        protected override void OnUpdate()
        {
            FreeCleaner();
            MoveCleaningTable();
            CleaningTableCleaner();
            AfterCleaningTableCleaner();
            MoveCleaningBreakBottle();
            AfterCleaningBreakBottleCleaner();
            CleanCompletedCleaner();
            MoveExitCleaner();
        }

        private void FreeCleaner()
        {
            var freeCleaners = _freeCleanerQuery.ToEntityArray(Allocator.Temp);

            foreach (var cleanerEntity in freeCleaners)
            {
                if (EntityManager.HasBuffer<OrderCleanTable>(cleanerEntity) &&
                    EntityManager.GetBuffer<OrderCleanTable>(cleanerEntity).Length > 0 &&
                    !EntityManager.HasComponent<CleanBreakBottleCleaner>(cleanerEntity))
                {
                    var bufferOrdersTable = EntityManager.GetBuffer<OrderCleanTable>(cleanerEntity);
                    var ordersTableHashSet = bufferOrdersTable.ToNativeArray(Allocator.Temp).ToHashSet();

                    bufferOrdersTable.Clear();

                    foreach (var order in ordersTableHashSet)
                    {
                        bufferOrdersTable.Add(order);
                    }

                    var tableEntity = EntityManager.GetBuffer<OrderCleanTable>(cleanerEntity)[0].Table;
                    var cleanTablePoint = EntityManager.GetComponentObject<TableView>(tableEntity).CleanTablePoint;
                    EntityManager.AddComponentData(cleanerEntity,
                        new CleanTableCleaner { ClearTablePoint = cleanTablePoint, Table = tableEntity });
                    EntityManager.RemoveComponent<FreeCleaner>(cleanerEntity);

                    CleanerDoor(CleanerAnimationConstants.OpenDoor);
                }

                if (!EntityManager.HasBuffer<OrderCleanBreakBottle>(cleanerEntity) ||
                    EntityManager.GetBuffer<OrderCleanBreakBottle>(cleanerEntity).Length <= 0)
                {
                    continue;
                }


                var bufferOrdersBreakBottle = EntityManager.GetBuffer<OrderCleanBreakBottle>(cleanerEntity);
                var ordersBreakBottleHashSet = bufferOrdersBreakBottle.ToNativeArray(Allocator.Temp).ToHashSet();

                bufferOrdersBreakBottle.Clear();

                foreach (var order in ordersBreakBottleHashSet)
                {
                    bufferOrdersBreakBottle.Add(order);
                }

                var breakBottleEntity = EntityManager.GetBuffer<OrderCleanBreakBottle>(cleanerEntity)[0].BreakBottle;
                var breakBottleTransform = EntityManager.GetComponentObject<BreakBottleView>(breakBottleEntity)
                    .Value.transform;

                EntityManager.AddComponentData(cleanerEntity,
                    new CleanBreakBottleCleaner { BreakBottlePosition = breakBottleTransform.position });
                EntityManager.RemoveComponent<FreeCleaner>(cleanerEntity);
                CleanerDoor(CleanerAnimationConstants.OpenDoor);
            }
        }

        private void CleanerDoor(FixedString64Bytes animationName)
        {
            var animation = _cleanerDoorQuery.GetSingleton<CleanerDoorView>().Value.DoorAnimation;
            var doorAnimation =
                animation.GetClip(animationName.Value);
            animation.clip = doorAnimation;
            animation.Play();
        }

        private void MoveCleaningTable()
        {
            var moveCleaningTable = _moveCleaningTableQuery.ToEntityArray(Allocator.Temp);

            foreach (var cleanerEntity in moveCleaningTable)
            {
                if (EntityManager.HasComponent<MoveCharacterCompleted>(cleanerEntity))
                {
                    var animator = EntityManager.GetComponentObject<AnimatorView>(cleanerEntity).Value;

                    EntityManager.AddComponent<CleaningTableCleaner>(cleanerEntity);
                    EntityManager.AddComponentData(cleanerEntity,
                        new WaitTime
                        {
                            Current = AnimationUtilities.AnimationLength(animator, CleanerAnimationConstants.TableWash)
                        });
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(cleanerEntity);
                }
                else
                {
                    var clearTablePoint =
                        EntityManager.GetComponentData<CleanTableCleaner>(cleanerEntity).ClearTablePoint;
                    EntityManager.AddComponentData(cleanerEntity,
                        new MoveCharacter { TargetPoint = clearTablePoint.Position });
                }
            }
        }

        private void CleaningTableCleaner()
        {
            var cleaningTableCleaner = _cleaningTableCleaner.ToEntityArray(Allocator.Temp);

            foreach (var cleanerEntity in cleaningTableCleaner)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(cleanerEntity).Value;
                var cleanerView = EntityManager.GetComponentObject<CleanerView>(cleanerEntity);
                var tableEntity = EntityManager.GetComponentData<CleanTableCleaner>(cleanerEntity).Table;
                var targetRotationPoint =
                    EntityManager.GetComponentObject<TableView>(tableEntity).Value.transform.position;

                cleanerView.Value.TurningCharacterToPoint(targetRotationPoint);
                animator.SetBool(CleanerAnimationConstants.WashTable, true);
            }
        }

        private void AfterCleaningTableCleaner()
        {
            var afterCleaningTableCleanerArray = _afterCleaningTableCleaner.ToEntityArray(Allocator.Temp);
            var tablePointEntities = _tablePointQuery.ToEntityArray(Allocator.Temp);

            foreach (var cleanerEntity in afterCleaningTableCleanerArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(cleanerEntity).Value;
                var ordersClearTable = EntityManager.GetBuffer<OrderCleanTable>(cleanerEntity);

                animator.SetBool(CleanerAnimationConstants.WashTable, false);

                if (ordersClearTable.Length > 0)
                {
                    ordersClearTable.RemoveAt(0);
                }

                EntityManager.AddComponent<CleaningCompletedCleaner>(cleanerEntity);

                var tableEntity = EntityManager.GetComponentData<CleanTableCleaner>(cleanerEntity).Table;

                foreach (var pointEntity in tablePointEntities)
                {
                    var tablePointEntity = EntityManager.GetComponentData<AtTablePoint>(pointEntity);

                    if (tablePointEntity.Table != tableEntity)
                    {
                        continue;
                    }

                    if (EntityManager.HasComponent<PointDirtTable>(pointEntity))
                    {
                        EntityManager.RemoveComponent<PointDirtTable>(pointEntity);
                    }
                }

                var table = EntityManager.GetComponentData<Table>(tableEntity);

                table.DirtValue = 0;

                var clearArrow = EntityManager.GetComponentObject<ClearMovementArrowView>(tableEntity).Arrow;
                clearArrow.DisableArrow();
                EntityManager.SetComponentData(tableEntity, table);
                EntityManager.RemoveComponent<CleaningTableCleaner>(cleanerEntity);
                EntityManager.RemoveComponent<CleanTableCleaner>(cleanerEntity);
            }
        }

        private void MoveCleaningBreakBottle()
        {
            var moveCleaningCleaners = _moveCleaningBreakBottleQuery.ToEntityArray(Allocator.Temp);
            var cleanerMopPoint =
                _cleanerMopPointQuery.ToComponentDataArray<CleanerMopPoint>(Allocator.Temp)[0].Position;

            foreach (var cleanerEntity in moveCleaningCleaners)
            {
                if (!EntityManager.HasComponent<MoveCharacterCompleted>(cleanerEntity))
                {
                    if (EntityManager.HasComponent<MopInHands>(cleanerEntity))
                    {
                        var cleanBreakBottlePoint = EntityManager
                            .GetComponentData<CleanBreakBottleCleaner>(cleanerEntity)
                            .BreakBottlePosition;
                        EntityManager.AddComponentData(cleanerEntity,
                            new MoveCharacter { TargetPoint = cleanBreakBottlePoint });
                    }
                    else
                    {
                        EntityManager.AddComponentData(cleanerEntity,
                            new MoveCharacter { TargetPoint = cleanerMopPoint });
                    }
                }

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(cleanerEntity))
                {
                    continue;
                }

                if (EntityManager.HasComponent<MopInHands>(cleanerEntity))
                {
                    var animator = EntityManager.GetComponentObject<AnimatorView>(cleanerEntity).Value;

                    animator.SetBool(CleanerAnimationConstants.WalkMop, true);
                    animator.SetBool(CleanerAnimationConstants.Mop, true);

                    EntityManager.AddComponent<CleaningBreakBottleCleaner>(cleanerEntity);
                    EntityManager.AddComponentData(cleanerEntity,
                        new WaitTime
                        {
                            Current = AnimationUtilities.AnimationLength(animator,
                                CleanerAnimationConstants.MopAnimation)
                        });
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(cleanerEntity);
                }
                else
                {
                    var cleanerView = EntityManager.GetComponentObject<CleanerView>(cleanerEntity);
                    cleanerView.EnableMop();
                    EntityManager.AddComponent<MopInHands>(cleanerEntity);
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(cleanerEntity);
                }
            }
        }

        private void AfterCleaningBreakBottleCleaner()
        {
            var afterCleaningBreakBottleCleaner = _afterCleaningBreakBottleCleaner.ToEntityArray(Allocator.Temp);

            foreach (var cleanerEntity in afterCleaningBreakBottleCleaner)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(cleanerEntity).Value;
                var ordersClearBreakBottle = EntityManager.GetBuffer<OrderCleanBreakBottle>(cleanerEntity);

                var breakBottleEntity = ordersClearBreakBottle[0].BreakBottle;

                if (ordersClearBreakBottle.Length > 0)
                {
                    ordersClearBreakBottle.RemoveAt(0);
                }

                EntityManager.AddComponent<Destroyed>(breakBottleEntity);

                animator.SetBool(CleanerAnimationConstants.Mop, false);
                animator.SetBool(CleanerAnimationConstants.WalkMop, false);

                EntityManager.AddComponent<CleaningCompletedCleaner>(cleanerEntity);
                EntityManager.RemoveComponent<CleanBreakBottleCleaner>(cleanerEntity);
                EntityManager.RemoveComponent<CleaningBreakBottleCleaner>(cleanerEntity);
            }
        }

        private void CleanCompletedCleaner()
        {
            var cleanCompletedCleaner = _cleaningCompletedCleaner.ToEntityArray(Allocator.Temp);
            var cleanerMopPoint = _cleanerMopPointQuery.ToComponentDataArray<CleanerMopPoint>(Allocator.Temp)[0];

            foreach (var cleanerEntity in cleanCompletedCleaner)
            {
                var freeCleaner = true;

                if (EntityManager.HasBuffer<OrderCleanTable>(cleanerEntity) &&
                    EntityManager.GetBuffer<OrderCleanTable>(cleanerEntity).Length > 0)
                {
                    EntityManager.AddComponent<FreeCleaner>(cleanerEntity);
                    EntityManager.RemoveComponent<CleaningCompletedCleaner>(cleanerEntity);
                    freeCleaner = false;
                }

                if (EntityManager.HasBuffer<OrderCleanBreakBottle>(cleanerEntity) &&
                    EntityManager.GetBuffer<OrderCleanBreakBottle>(cleanerEntity).Length > 0)
                {
                    EntityManager.AddComponent<FreeCleaner>(cleanerEntity);
                    EntityManager.RemoveComponent<CleaningCompletedCleaner>(cleanerEntity);
                    freeCleaner = false;
                }

                if (!freeCleaner)
                {
                    continue;
                }

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(cleanerEntity) &&
                    EntityManager.HasComponent<MopInHands>(cleanerEntity))
                {
                    EntityManager.AddComponentData(cleanerEntity,
                        new MoveCharacter { TargetPoint = cleanerMopPoint.Position });
                }

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(cleanerEntity) &&
                    !EntityManager.HasComponent<MopInHands>(cleanerEntity))
                {
                    EntityManager.AddComponent<MoveExitCleaner>(cleanerEntity);
                }

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(cleanerEntity) ||
                    !EntityManager.HasComponent<MopInHands>(cleanerEntity))
                {
                    continue;
                }

                var cleanerView = EntityManager.GetComponentObject<CleanerView>(cleanerEntity);
                cleanerView.DisableMop();

                EntityManager.RemoveComponent<MopInHands>(cleanerEntity);
                EntityManager.RemoveComponent<MoveCharacterCompleted>(cleanerEntity);
                EntityManager.AddComponent<MoveExitCleaner>(cleanerEntity);
            }
        }

        private void MoveExitCleaner()
        {
            var moveExitCleaners = _moveExitCleanerQuery.ToEntityArray(Allocator.Temp);
            var exitPoint =
                _spawnPointCleanerQuery.ToComponentDataArray<SpawnPointCleaner>(Allocator.Temp)[0];


            foreach (var cleanerEntity in moveExitCleaners)
            {
                if (!EntityManager.HasComponent<MoveCharacterCompleted>(cleanerEntity))
                {
                    EntityManager.AddComponentData(cleanerEntity,
                        new MoveCharacter { TargetPoint = exitPoint.Position });
                }

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(cleanerEntity) ||
                    !EntityManager.HasComponent<CleaningCompletedCleaner>(cleanerEntity))
                {
                    continue;
                }

                CleanerDoor(CleanerAnimationConstants.CloseDoor);
                EntityManager.RemoveComponent<CleaningCompletedCleaner>(cleanerEntity);
                EntityManager.RemoveComponent<MoveExitCleaner>(cleanerEntity);
                EntityManager.RemoveComponent<MoveCharacterCompleted>(cleanerEntity);
                EntityManager.AddComponent<FreeCleaner>(cleanerEntity);
            }
        }
    }
}