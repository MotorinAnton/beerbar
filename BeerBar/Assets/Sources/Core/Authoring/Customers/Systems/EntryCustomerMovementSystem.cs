using System.Linq;
using Core.Authoring.Characters;
using Core.Authoring.Points;
using Core.Authoring.Tables;
using Core.Components;
using Core.Components.Wait;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using MoveCharacter = Core.Authoring.Characters.MoveCharacter;

namespace Core.Authoring.Customers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class EntryCustomerMovementSystem : SystemBase
    {
        private EntityQuery _entryMoveCustomersQuery;
        private EntityQuery _entryWaitingCustomersQuery;
        private EntityQuery _entryWaitFinishedCustomersQuery;
        private EntityQuery _allEntryCustomersQuery;
        private EntityQuery _purchaseQueueAllCustomersQuery;
        private EntityQuery _customerLookShowcaseQuery;
        private EntityQuery _entryPointsQuery;
        private EntityQuery _purchasePointsQuery;
        private EntityQuery _lookContainerPoints;

        protected override void OnCreate()
        {
            using var entryMoveCustomersBuilder = new EntityQueryBuilder(Allocator.Temp);
            _entryMoveCustomersQuery = entryMoveCustomersBuilder.WithAll<Customer, EntryCustomer, IndexMovePoint>()
                .WithNone<MoveCharacter, WaitingCustomer, WaitTime>().Build(this);

            using var entryWaitingCustomersBuilder = new EntityQueryBuilder(Allocator.Temp);
            _entryWaitingCustomersQuery = entryWaitingCustomersBuilder
                .WithAll<Customer, EntryCustomer, WaitingCustomer, WaitTime>().Build(this);

            using var entryWaitFinishedCustomersBuilder = new EntityQueryBuilder(Allocator.Temp);
            _entryWaitFinishedCustomersQuery = entryWaitFinishedCustomersBuilder
                .WithAll<Customer, EntryCustomer, WaitingCustomer>().WithNone<WaitTime>().Build(this);

            using var allEntryCustomersBuilder = new EntityQueryBuilder(Allocator.Temp);
            _allEntryCustomersQuery =
                allEntryCustomersBuilder.WithAll<Customer, EntryCustomer, IndexMovePoint>().Build(this);

            using var purchaseQueueAllCustomersBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchaseQueueAllCustomersQuery = purchaseQueueAllCustomersBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint>().Build(this);

            using var customerLookShowcaseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _customerLookShowcaseQuery = customerLookShowcaseBuilder
                .WithAll<Customer, LookShowcaseCustomer, IndexMovePoint>().Build(this);
            
            using var entryPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _entryPointsQuery = entryPointsBuilder.WithAll<EntryPoint, MoveCustomerPoint>().Build(this);

            using var purchasePointsArrayBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchasePointsQuery = purchasePointsArrayBuilder.WithAll<PurchasePoint, MoveCustomerPoint>()
                .WithNone<PointNotAvailable>().Build(this);

            using var lookContainerPointArrayBuilder = new EntityQueryBuilder(Allocator.Temp);
            _lookContainerPoints =
                lookContainerPointArrayBuilder.WithAll<CustomerPointContainer>().Build(this);
        }

        protected override void OnUpdate()
        {
            MoveCustomers();
            WaitingCustomers();
            UpdateWaitingFreePointCustomers();
        }

        private void MoveCustomers()
        {
            var moveCustomerEntityArray = _entryMoveCustomersQuery.ToEntityArray(Allocator.Temp);
            var entryPoints = _entryPointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);

            foreach (var customerEntity in moveCustomerEntityArray)
            {
                if (EntityManager.HasComponent<MoveCharacterCompleted>(customerEntity))
                {
                    EntityManager.AddComponent<WaitingCustomer>(customerEntity);
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(customerEntity);
                }
                else
                {
                    var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                    var customerTargetPoint = entryPoints[customerIndex.Value].Point.Position;
                    EntityManager.AddComponentData(customerEntity, new MoveCharacter { TargetPoint = customerTargetPoint });
                }
            }
        }

        private void WaitingCustomers()
        {
            var waitingCustomerEntityArray = _entryWaitingCustomersQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var customerEntity in waitingCustomerEntityArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                animator.SetBool(CustomerAnimationConstants.Walk, false);
            }
        }

        private void UpdateWaitingFreePointCustomers()
        {
            var waitFinishedCustomerEntityArray = _entryWaitFinishedCustomersQuery.ToEntityArray(Allocator.Temp);

            foreach (var customerEntity in waitFinishedCustomerEntityArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                var entryPoints = _entryPointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
                var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                
                animator.SetBool(CustomerAnimationConstants.Walk, false);
                
                UpdateEntryCustomersQueuePosition(customerEntity);

                if (customerIndex.Value > entryPoints.Select(point => point.Row).Max())
                {
                    continue;
                }

                var randomChance = Random.Range(0, 2);
                
                switch (randomChance)
                {
                    case 0 when CheckFreeShowcasePoint(customerEntity, out var freeShowcasePoint):
                        
                        customerIndex.Value = freeShowcasePoint;
                        EntityManager.SetComponentData(customerEntity, customerIndex);
                        EntityManager.AddComponent<LookShowcaseCustomer>(customerEntity);
                        EntityManager.RemoveComponent<EntryCustomer>(customerEntity);
                        EntityManager.RemoveComponent<WaitingCustomer>(customerEntity);
                        break;

                    case 1 when CheckFreePurchasePoint(customerEntity, out var freePurchasePoint):
                        
                        customerIndex.Value = freePurchasePoint;
                        EntityManager.SetComponentData(customerEntity, customerIndex);
                        EntityManager.AddComponent<PurchaseQueueCustomer>(customerEntity);
                        EntityManager.RemoveComponent<EntryCustomer>(customerEntity);
                        EntityManager.RemoveComponent<WaitingCustomer>(customerEntity);
                        break;
                }
            }
        }
        

        private void UpdateEntryCustomersQueuePosition(Entity customerEntity)
        {
            var indexCustomer = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
            var entryPoints = _entryPointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
            var indexCustomerArray = _allEntryCustomersQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            var freePoints = entryPoints.Select(indexPoint => indexPoint.IndexPoint).ToHashSet();
            var customerPointIndexes = indexCustomerArray.Select(index => index.Value).ToHashSet();

            freePoints.ExceptWith(customerPointIndexes);

            if (!freePoints.Contains(indexCustomer.Value - 1))
            {
                return;
            }

            indexCustomer.Value -= 1;
            EntityManager.SetComponentData(customerEntity, indexCustomer);
            EntityManager.RemoveComponent<WaitingCustomer>(customerEntity);
        }

        private bool CheckFreeShowcasePoint(Entity customerEntity, out int result)
        {
            var indexCustomer = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
            var lookContainerCustomerIndexes =
                _customerLookShowcaseQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            var lookContainerPoints =
                _lookContainerPoints.ToComponentDataArray<CustomerPointContainer>(Allocator.Temp);
            var customerIndexes = lookContainerCustomerIndexes.Select(lookCustomerIndex => lookCustomerIndex.Value)
                .ToHashSet();
            var containerLookPointIndexes = lookContainerPoints.Select(indexPoint => indexPoint.Index).ToHashSet();

            containerLookPointIndexes.ExceptWith(customerIndexes);

            if (containerLookPointIndexes.Count > 0)
            {
                var randomPont = Random.Range(0, containerLookPointIndexes.Count);
                result = containerLookPointIndexes.ToArray()[randomPont];
                return true;
            }

            result = indexCustomer.Value;
            return false;
        }

        private bool CheckFreePurchasePoint(Entity customerEntity, out int result)
        {
            var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
            var purchaseQueueCustomerIndexes =
                _purchaseQueueAllCustomersQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            var purchaseQueuePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
            var customerIndexes = purchaseQueueCustomerIndexes
                .Select(customerPurchaseQueueIndex => customerPurchaseQueueIndex.Value).ToHashSet();
            var freePointIndexes = purchaseQueuePoints.Select(point => point.IndexPoint).ToHashSet();

            freePointIndexes.ExceptWith(customerIndexes);

            if (freePointIndexes.Count > 0)
            {
                result = freePointIndexes.Min();
                return true;
            }

            result = result = customerIndex.Value;
            return false;
        }
    }
}
