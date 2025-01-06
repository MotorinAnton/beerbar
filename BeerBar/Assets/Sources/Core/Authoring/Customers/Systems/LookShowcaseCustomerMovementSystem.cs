using System.Linq;
using Core.Authoring.Characters;
using Core.Authoring.Containers;
using Core.Authoring.Points;
using Core.Authoring.Tables;
using Core.Components;
using Core.Components.Wait;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Customers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class LookShowcaseCustomerMovementSystem : SystemBase
    {
        private EntityQuery _purchaseQueueAllCustomersQuery;
        private EntityQuery _moveCustomerLookShowcaseQuery;
        private EntityQuery _waitingCustomerLookShowcaseQuery;
        private EntityQuery _waitingFreePointCustomerLookShowcaseQuery;
        private EntityQuery _allCustomerLookShowcaseQuery;
        private EntityQuery _purchasePointsQuery;
        private EntityQuery _lookContainerPointsQuery;

        protected override void OnCreate()
        {
            using var purchaseQueueAllCustomersBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchaseQueueAllCustomersQuery = purchaseQueueAllCustomersBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint>().Build(this);

            using var moveCustomerLookShowcaseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveCustomerLookShowcaseQuery = moveCustomerLookShowcaseBuilder
                .WithAll<Customer, LookShowcaseCustomer, IndexMovePoint>()
                .WithNone<MoveCharacter, WaitTime, WaitingCustomer>().Build(this);

            using var waitingCustomerLookShowcaseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _waitingCustomerLookShowcaseQuery = waitingCustomerLookShowcaseBuilder
                .WithAll<Customer, LookShowcaseCustomer, IndexMovePoint, WaitTime, WaitingCustomer, NavMeshAgentView>()
                .Build(this);

            using var waitingFinishedCustomerLookShowcaseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _waitingFreePointCustomerLookShowcaseQuery = waitingFinishedCustomerLookShowcaseBuilder
                .WithAll<Customer, LookShowcaseCustomer, IndexMovePoint, WaitingCustomer, NavMeshAgentView>()
                .WithNone<WaitTime, MoveCharacter>().Build(this);

            using var allCustomerLookShowcaseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _allCustomerLookShowcaseQuery = allCustomerLookShowcaseBuilder
                .WithAll<Customer, LookShowcaseCustomer, IndexMovePoint>().Build(this);

            using var lookContainerPointsArrayBuilder = new EntityQueryBuilder(Allocator.Temp);
            _lookContainerPointsQuery = lookContainerPointsArrayBuilder.WithAll<CustomerPointContainer>().Build(this);

            using var purchasePointsArrayBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchasePointsQuery = purchasePointsArrayBuilder.WithAll<PurchasePoint, MoveCustomerPoint>()
                .WithNone<PointNotAvailable>().Build(this);
        }

        protected override void OnUpdate()
        {
            MoveCustomers();
            WaitingFreePointCustomers();
            LookShowcaseCustomers();
        }

        private void MoveCustomers()
        {
            var moveCustomerEntityArray = _moveCustomerLookShowcaseQuery.ToEntityArray(Allocator.Temp);
            var lookContainerPoints =
                _lookContainerPointsQuery.ToComponentDataArray<CustomerPointContainer>(Allocator.Temp);

            foreach (var customerEntity in moveCustomerEntityArray)
            {
                if (EntityManager.HasComponent<MoveCharacterCompleted>(customerEntity))
                {
                    var randomWaitingTime = Random.Range
                    (
                        CustomerAnimationConstants.MinLookShowcaseTime,
                        CustomerAnimationConstants.MaxLookShowcaseTime
                        );
                    EntityManager.AddComponentData(customerEntity,
                        new WaitTime { Current = randomWaitingTime });
                    EntityManager.AddComponent<WaitingCustomer>(customerEntity);
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(customerEntity);
                }
                else
                {
                    var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                    var customerTargetPoint = lookContainerPoints
                        .First(indexPoint => indexPoint.Index == customerIndex.Value).Point
                        .Position;
                    EntityManager.AddComponentData(customerEntity,
                        new MoveCharacter { TargetPoint = customerTargetPoint });
                }
            }
        }

        private void WaitingFreePointCustomers()
        {
            var waitingFreePointCustomerEntityArray =
                _waitingFreePointCustomerLookShowcaseQuery.ToEntityArray(Allocator.Temp);
            var lookContainerPoints =
                _lookContainerPointsQuery.ToComponentDataArray<CustomerPointContainer>(Allocator.Temp);
            
            foreach (var customerEntity in waitingFreePointCustomerEntityArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                var randomChance = Random.Range(0, 2);
                
                animator.SetBool(CustomerAnimationConstants.Look, false);
                
                switch (randomChance)
                {
                    case 0 when CheckFreeLookContainerPoint(customerEntity, out var indexPoint):
                        customerIndex.Value = indexPoint;
                        EntityManager.SetComponentData(customerEntity, customerIndex);
                        var customerTargetPoint = lookContainerPoints.First(index => index.Index == customerIndex.Value).Point
                            .Position;
                        EntityManager.AddComponentData(customerEntity, new MoveCharacter{ TargetPoint = customerTargetPoint});
                        EntityManager.RemoveComponent<WaitingCustomer>(customerEntity);
                        continue;
                    
                    case 1 when CheckFreePurchasePoint(customerEntity, out var freeIndex):
                        customerIndex.Value = freeIndex;
                        EntityManager.SetComponentData(customerEntity, customerIndex);
                        EntityManager.AddComponent<PurchaseQueueCustomer>(customerEntity);
                        EntityManager.RemoveComponent<LookShowcaseCustomer>(customerEntity);
                        EntityManager.RemoveComponent<WaitingCustomer>(customerEntity);
                        continue;
                }
            }
        }

        private void LookShowcaseCustomers()
        {
            var waitingCustomerEntityArray = _waitingCustomerLookShowcaseQuery.ToEntityArray(Allocator.Temp);
            var lookContainerPoints =
                _lookContainerPointsQuery.ToComponentDataArray<CustomerPointContainer>(Allocator.Temp);
            
            foreach (var customerEntity in waitingCustomerEntityArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                var customerView = EntityManager.GetComponentObject<CustomerView>(customerEntity);
                var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                var container = lookContainerPoints.FirstOrDefault(point => point.Index == customerIndex.Value).Container;
                var targetPoint = EntityManager.GetComponentObject<ContainerView>(container).Value.transform.position;;
                
                customerView.Value.TurningCharacterToPoint(targetPoint);
                animator.SetBool(CustomerAnimationConstants.Look, true);
            }
        }

        private bool CheckFreeLookContainerPoint( Entity customerEntity, out int result )
        {
            var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
            var lookContainerCustomerIndexes =
                _allCustomerLookShowcaseQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            var lookContainerPoints =
                _lookContainerPointsQuery.ToComponentDataArray<CustomerPointContainer>(Allocator.Temp);
            var customerIndexes = lookContainerCustomerIndexes
                .Select(customerLookShowcase => customerLookShowcase.Value).ToHashSet();
            var freePointIndexes = lookContainerPoints.Select(pointIndex => pointIndex.Index).ToHashSet();
            
            freePointIndexes.ExceptWith(customerIndexes);
            
            if (freePointIndexes.Count > 0)
            {
                var randomPoint = Random.Range(0, freePointIndexes.Count);
                result = freePointIndexes.ToArray()[randomPoint];
                return true;
            }
            
            result = customerIndex.Value;
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
            var freePurchasePoints = purchaseQueuePoints.Select(pointIndex => pointIndex.IndexPoint).ToHashSet();
            freePurchasePoints.ExceptWith(customerIndexes);

            if (freePurchasePoints.Count > 0)
            {
                
                result = freePurchasePoints.Min();
                return true;

            }
            
            result = result = customerIndex.Value;
            return false;
        }
    }
}