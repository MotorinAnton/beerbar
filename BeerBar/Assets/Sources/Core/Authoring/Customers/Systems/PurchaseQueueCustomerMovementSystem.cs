using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Bartenders;
using Core.Authoring.Characters;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.Points;
using Core.Authoring.Products;
using Core.Authoring.Tables;
using Core.Components;
using Core.Components.Wait;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using MoveCharacterCompleted = Core.Authoring.Characters.MoveCharacterCompleted;

namespace Core.Authoring.Customers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class PurchaseQueueCustomerMovementSystem : SystemBase
    {
        private EntityQuery _movePurchaseQueueCustomerQuery;
        private EntityQuery _waitingPurchaseQueueCustomerQuery;
        private EntityQuery _waitingFreePointPurchaseQueueCustomerQuery;
        private EntityQuery _dissatisfiedPurchaseQueueCustomerQuery;
        private EntityQuery _afterDissatisfiedPurchaseQueueCustomerQuery;
        private EntityQuery _customerLookShowcaseQuery;
        private EntityQuery _freeBarmanQuery;
        private EntityQuery _purchasePointsQuery;
        private EntityQuery _barmanCashPointsPointArray;
        private EntityQuery _phrasePanelUi;
        private EntityQuery _eventPhrasePanelUi;

        protected override void OnCreate()
        {
            using var movePurchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _movePurchaseQueueCustomerQuery = movePurchaseQueueCustomerBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint>()
                .WithNone<MoveExit, WaitTime, WaitingCustomer, DissatisfiedCustomer>().Build(this);
            
            using var waitingPurchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _waitingPurchaseQueueCustomerQuery = waitingPurchaseQueueCustomerBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint, WaitingCustomer, WaitTime>()
                .WithNone<MoveExit, DissatisfiedCustomer>().Build(this);
            
            using var waitingFreePointPurchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _waitingFreePointPurchaseQueueCustomerQuery = waitingFreePointPurchaseQueueCustomerBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint, WaitingCustomer>()
                .WithNone<WaitTime, MoveExit, DissatisfiedCustomer>().Build(this);
            
            using var dissatisfiedPurchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _dissatisfiedPurchaseQueueCustomerQuery = dissatisfiedPurchaseQueueCustomerBuilder
                .WithAll<Customer, DissatisfiedCustomer , WaitTime>().Build(this);
            
            using var afterDissatisfiedPurchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterDissatisfiedPurchaseQueueCustomerQuery = afterDissatisfiedPurchaseQueueCustomerBuilder
                .WithAll<Customer, DissatisfiedCustomer>().WithNone<WaitTime, MoveExit>().Build(this);
            
            using var freeBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _freeBarmanQuery = freeBarmanBuilder.WithAll<Barman, FreeBarman>().WithNone<OrderBarman, WaitTime>().Build(this);
            
            using var purchasePointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchasePointsQuery = purchasePointsBuilder.WithAll<PurchasePoint, MoveCustomerPoint>().WithNone<PointNotAvailable>().Build(this);
            
            using var barmanCashPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _barmanCashPointsPointArray = barmanCashPointsBuilder.WithAll<BarmanSpawnPoint, SpawnPoint>().Build(this);
            
            using var phrasePanelUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phrasePanelUi = phrasePanelUiBuilder.WithAll<PhraseCustomerUi.PhrasePanelCustomerUi, PhraseCustomerUiView>().Build(this);
            
            using var eventPhrasePanelUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _eventPhrasePanelUi = eventPhrasePanelUiBuilder.WithAll<EventPhrasePanelCustomerUi>().Build(this);
        }

        protected override void OnUpdate()
        {
            WaitingCustomers();
            WaitingFreePointCustomers();
            DissatisfiedCustomers();
            AfterDissatisfiedCustomers();
            MoveCustomers();
        }

        private void MoveCustomers()
        {
            var moveCustomerEntityArray = _movePurchaseQueueCustomerQuery.ToEntityArray(Allocator.Temp);
            var purchaseQueuePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);

            foreach (var customerEntity in moveCustomerEntityArray)
            {
                if (EntityManager.HasComponent<MoveCharacterCompleted>(customerEntity))
                {
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(customerEntity);
                    EntityManager.AddComponent<WaitingCustomer>(customerEntity);
                    
                    var customerUiEntity = EntityManager.GetComponentData<CustomerUIEntity>(customerEntity).UiEntity;

                    if (EntityManager.HasComponent<WaitTime>(customerUiEntity))
                    {
                        continue;
                    }

                    var maxWaitTime = EntityUtilities.GetCustomerConfig().MaxWaitTimeInPurchaseQueue;
                    EntityManager.AddComponentData(customerUiEntity, new WaitTime { Current = maxWaitTime });
                }
                else
                {
                    var indexCustomer = EntityManager.GetComponentData<IndexMovePoint>(customerEntity).Value;
                    var rowCount = purchaseQueuePoints.Select(point => point.Row).ToHashSet().Count;
                    var column = indexCustomer / rowCount;
                    var row = indexCustomer % rowCount;
                    var targetPoint = purchaseQueuePoints
                        .FirstOrDefault(index => index.Column == column && index.Row == row).Point.Position;
                    
                    EntityManager.AddComponentData(customerEntity, new MoveCharacter { TargetPoint = targetPoint });
                }
            }
        }

        private void WaitingFreePointCustomers()
        {
            var purchaseQueuePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
            var waitingFreePointCustomerEntityArray = _waitingFreePointPurchaseQueueCustomerQuery.ToEntityArray(Allocator.Temp);
            var barmanCashPoints = _barmanCashPointsPointArray.ToComponentDataArray<SpawnPoint>(Allocator.Temp);
            
            foreach (var customerEntity in waitingFreePointCustomerEntityArray)
            {
                var customerView = EntityManager.GetComponentObject<CustomerView>(customerEntity).Value;
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                var indexCustomer = EntityManager.GetComponentData<IndexMovePoint>(customerEntity).Value;
                var rowCount = purchaseQueuePoints.Select(point => point.Row).ToHashSet().Count;
                var column = indexCustomer / rowCount;
                var row = indexCustomer % rowCount;
                var targetRotationPoint = barmanCashPoints[row].Position;
                
                customerView.TurningCharacterToPoint(targetRotationPoint);

                if (column != 0)
                {
                    continue;
                }
                
                EntityManager.AddComponent<PhraseSayCustomer>(customerEntity);
                animator.SetBool(CustomerAnimationConstants.CashIdle, true);

                if (!EntityManager.HasComponent<PanelCustomerIsShow>(customerEntity))
                {
                    var phraseEn = EntityManager.CreateEntity();
                    EntityManager.AddComponentObject(phraseEn, new SpawnPhrasePanelCustomerUi
                    {
                        Customer = customerEntity,
                        Index = row
                    });
                    EntityManager.AddComponent<PanelCustomerIsShow>(customerEntity);
                }

                var freeBarman = _freeBarmanQuery.ToEntityArray(Allocator.Temp)
                    .FirstOrDefault(entity => EntityManager.GetComponentData<BarmanIndex>(entity).Value == row);

                if (freeBarman == default)
                {
                    continue;
                }

                var customerOrderProducts =
                    EntityManager.GetComponentObject<CustomerProduct>(customerEntity).Products;
                            
                EntityManager.AddComponentObject(freeBarman,
                    new OrderBarman
                    {
                        Products = customerOrderProducts, 
                        CustomerEntity = customerEntity,
                        CompletedProduct = new List<ProductData>().ToArray()
                    });
                EntityManager.AddComponentData(freeBarman,
                    new WaitTime { Current = BarmanAnimationConstants.ServiceTime });
                
            }
        }
        private void WaitingCustomers()
        {
            var purchaseQueuePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
            var waitingCustomerEntity = _waitingPurchaseQueueCustomerQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var customerEntity in waitingCustomerEntity)
            {
                var customerView = EntityManager.GetComponentObject<CustomerView>(customerEntity).Value;
                var targetPoint = purchaseQueuePoints[0].Point.Position;
                
                customerView.TurningCharacterToPoint(targetPoint);
            }
        }
        
        private void DissatisfiedCustomers()
        {
            var dissatisfiedCustomerEntity = _dissatisfiedPurchaseQueueCustomerQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var customerEntity in dissatisfiedCustomerEntity)
            {
                SpawnEventPanel(customerEntity, EventPhraseType.Displeased);
            } 
        }

        private void AfterDissatisfiedCustomers()
        {
            var afterDissatisfiedCustomerEntity =
                _afterDissatisfiedPurchaseQueueCustomerQuery.ToEntityArray(Allocator.Temp);

            foreach (var customerEntity in afterDissatisfiedCustomerEntity)
            {
                var purchaseQueuePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
                var indexCustomer = EntityManager.GetComponentData<IndexMovePoint>(customerEntity).Value;
                var rowCount = purchaseQueuePoints.Select(point => point.Row).ToHashSet().Count;
                var row = indexCustomer % rowCount;

                EntityManager.RemoveComponent<DissatisfiedCustomer>(customerEntity);
                EntityManager.AddComponent<MoveExit>(customerEntity);
                EntityManager.AddComponentData(customerEntity,
                    new UpdatePurchaseQueuePosition { UpdateRow = row });
                
                if (EntityManager.HasComponent<PhraseSayCustomer>(customerEntity))
                {
                    EntityManager.RemoveComponent<PhraseSayCustomer>(customerEntity);
                }
                
                var panelArray = _phrasePanelUi.ToComponentArray<PhraseCustomerUiView>();
                
                foreach (var panel in panelArray)
                {
                    if (panel.Value._customer == customerEntity)
                    {
                        panel.Value.PanelFadeOut();
                    }
                }
            }
        }
        
        private void SpawnEventPanel(Entity customerEntity, EventPhraseType type)
        {
            if (!_eventPhrasePanelUi.IsEmpty)
            {
                return;
            }

            var eventPhraseEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(eventPhraseEntity, new SpawnEventPhrasePanelCustomerUi
            {
                Customer = customerEntity,
                Type = type
                    
            });
            EntityManager.AddComponent<EventPanelCustomerIsShow>(customerEntity);
        }
    }
}