using System.Collections.Generic;
using Core.Authoring.Banks;
using Core.Authoring.Characters;
using Core.Authoring.Cleaners;
using Core.Authoring.Customers;
using Core.Authoring.Customers.CustomersUi;
using Core.Authoring.MovementArrows;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.ProfitUi;
using Core.Authoring.SelectGameObjects;
using Core.Components;
using Core.Components.Destroyed;
using Core.Components.Wait;
using Core.Constants;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.EventObjects.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class EventObjectClickedSystem : SystemBase
    {
        private EntityQuery _bankQuery;
        private EntityQuery _cleanerQuery;
        private EntityQuery _customerQuery;
        private EntityQuery _phraseUiManager;

        protected override void OnCreate()
        {
            using var bankBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _bankQuery = bankBuilder.WithAllRW<Bank>().Build(this);
            
            using var cleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _cleanerQuery = cleanerBuilder.WithAll<Cleaner>()
                .Build(this);
            
            using var customerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _customerQuery = customerBuilder.WithAll<Customer>()
                .Build(this);
            
            using var phraseUiManagerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phraseUiManager = phraseUiManagerBuilder.WithAll<PhraseCustomerUiManagerView>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<BreakBottleEntity, Clicked>().ForEach((Entity entity) =>
                {
                    CreateClearBreakBottleOrder(entity);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
            
            Entities.WithAll<BreakBottleEntity>().WithNone<WaitTime>().ForEach((Entity entity) =>
            {
                CheckNearCustomer(entity);
                
            }).WithoutBurst().WithStructuralChanges().Run();
            
            Entities.WithAll<LossWalletEntity, Clicked>()
                .ForEach((Entity entity, in LossWalletEntity lossWalletEntity) =>
                {
                    PickUpLossWallet(entity, lossWalletEntity);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void CheckNearCustomer(Entity breakBottleEntity)
        {
            var breakBottleView = EntityManager.GetComponentObject<BreakBottleView>(breakBottleEntity).Value;
            var customers = _customerQuery.ToEntityArray(Allocator.Temp);

            foreach (var customerEntity in customers)
            {
                var customerTransform = EntityManager.GetComponentObject<TransformView>(customerEntity).Value;
                var distance = Vector3.Distance(customerTransform.position, breakBottleView.transform.position);

                const float minDistance = CustomerAnimationConstants.MinDistanceBreakBottleCustomer;
                
                if (!(distance < minDistance))
                {
                    continue;
                }

                var customerUiEntity = EntityManager.GetComponentData<CustomerUIEntity>(customerEntity).UiEntity;
                var customerUiView = EntityManager.GetComponentObject<CustomerUiView>(customerUiEntity);

                if (!_phraseUiManager.IsEmpty)
                {
                    var customerView = EntityManager.GetComponentObject<CustomerView>(customerEntity);
                    var randomPhrase = Random.Range(0, customerView.Dialogs.DirtyTable.Length);
                    var phraseUiManagerEntity = _phraseUiManager.ToEntityArray(Allocator.Temp)[0];
                    var phraseUiManager =
                        EntityManager.GetComponentObject<PhraseCustomerUiManagerView>(phraseUiManagerEntity)
                            .PhraseCustomerUiManager;

                    phraseUiManager.EventPanel.SetPhrasePanelUiComponent(
                        customerView.Dialogs.DirtyTable[randomPhrase], customerView.Avatar);
                    phraseUiManager.StartEventPanelTween();
                }

                if (!customerUiView.Value.FaceEmotionImage.IsActive())
                {
                    customerUiView.Value.EnableFaceEmotion();
                }

                if (!EntityManager.HasComponent<SwearEmotionCustomer>(customerUiEntity))
                {
                    EntityManager.AddComponent<SwearEmotionCustomer>(customerUiEntity);
                }
            }

            EntityManager.AddComponentData(breakBottleEntity, new WaitTime { Current = 5f });
        }
        
        private void CreateClearBreakBottleOrder(Entity breakBottleEntity)
        {
            if (EntityManager.HasComponent<Destroyed>(breakBottleEntity))
            {
                EntityManager.RemoveComponent<Clicked>(breakBottleEntity);
                return;
            }
            
            var cleanerArray = _cleanerQuery.ToEntityArray(Allocator.Temp);
            var orderTables = new HashSet<Entity>();
            
            foreach (var cleanerEntity in cleanerArray)
            {
                if (!EntityManager.HasBuffer<OrderCleanBreakBottle>(cleanerEntity))
                {
                    continue;
                }

                var cleanerOrderBuffer = EntityManager.GetBuffer<OrderCleanBreakBottle>(cleanerEntity);
                    
                foreach (var orderClearBreakBottle in cleanerOrderBuffer)
                {
                    orderTables.Add(orderClearBreakBottle.BreakBottle);
                }
            }
            
            if (orderTables.Contains(breakBottleEntity))
            {
                return;
            }
            
            EnableClearArrow(breakBottleEntity);
            
            foreach (var cleanerEntity in cleanerArray)
            {
                if (EntityManager.HasBuffer<OrderCleanBreakBottle>(cleanerEntity))
                {
                    var bufferOrders = EntityManager.GetBuffer<OrderCleanBreakBottle>(cleanerEntity);
                    bufferOrders.Add(new OrderCleanBreakBottle { BreakBottle = breakBottleEntity });
                }
                else
                {
                    var bufferOrdersBarman = EntityManager.AddBuffer<OrderCleanBreakBottle>(cleanerEntity);
                    bufferOrdersBarman.Add(new OrderCleanBreakBottle { BreakBottle = breakBottleEntity });
                }

                EntityManager.RemoveComponent<Clicked>(breakBottleEntity);

                if (!EntityManager.HasComponent<CleaningCompletedCleaner>(cleanerEntity))
                {
                    continue;
                }

                EntityManager.RemoveComponent<MoveCharacter>(cleanerEntity);
                EntityManager.RemoveComponent<CleaningCompletedCleaner>(cleanerEntity);
                EntityManager.RemoveComponent<MoveExitCleaner>(cleanerEntity);
                EntityManager.AddComponent<FreeCleaner>(cleanerEntity);
            }
        }
        
        private void PickUpLossWallet(Entity entity, LossWalletEntity lossWalletEntity)
        {
            var bank = _bankQuery.GetSingleton<Bank>();
            var profitUiPosition = EntityManager.GetComponentObject<LossWalletView>(entity).Value.transform.position;
            var lossWalletView = EntityManager.GetComponentObject<LossWalletView>(entity).Value;
            DOTween.KillAll(lossWalletView.gameObject);
            
            profitUiPosition.y += BreakdownObjectConstants.PickUpLossWalletOffsetY;
            
            var spawnProfitUiEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(spawnProfitUiEntity,
                new SpawnProfitUi
                {
                    Profit = true,
                    Point = profitUiPosition,
                    Text = "+" + lossWalletEntity.Coins
                });
            
            bank.Coins += lossWalletEntity.Coins;
            _bankQuery.SetSingleton(bank);
            EntityManager.AddComponent<Destroyed>(entity);
        }
        
        private void EnableClearArrow(Entity entity)
        {
            var movementArrows =
                EntityManager.GetComponentObject<ClearMovementArrowView>(entity).Arrow;
            movementArrows.EnableArrow();
        }
    }
}