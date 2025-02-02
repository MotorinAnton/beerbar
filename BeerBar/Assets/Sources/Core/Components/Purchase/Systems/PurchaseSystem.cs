using System.Linq;
using Core.Authoring.Banks;
using Core.Authoring.Bartenders;
using Core.Authoring.Customers;
using Core.Authoring.Customers.CustomersUi;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.Points;
using Core.Authoring.ProfitUi;
using Core.Authoring.StoreRatings;
using Core.Authoring.Tables;
using Core.Components.Wait;
using Core.Constants;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Components.Purchase.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class PurchaseSystem : SystemBase
    {
        private EntityQuery _bankQuery;
        private EntityQuery _storeRatingQuery;
        private EntityQuery _phraseUiManager;
        private EntityQuery _phrasePanelUi;
        private EntityQuery _purchaseQueueCustomerQuery;
        private EntityQuery _purchasePointsQuery;
        
        protected override void OnCreate()
        {
            using var bankBuilder = new EntityQueryBuilder(Allocator.Temp);
            _bankQuery = bankBuilder.WithAllRW<Bank>().Build(this);
            
            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAllRW<StoreRating>().Build(this);
            
            using var phraseUiManagerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phraseUiManager = phraseUiManagerBuilder.WithAll<PhraseCustomerUiPositionView>().Build(this);
            
            using var phrasePanelUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phrasePanelUi = phrasePanelUiBuilder.WithAll<PhrasePanelCustomerUi,PhraseCustomerUiView>().Build(this);
            
            using var purchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchaseQueueCustomerQuery = purchaseQueueCustomerBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint>().Build(this);
            
            using var purchasePointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchasePointsQuery = purchasePointsBuilder.WithAll<PurchasePoint, MoveCustomerPoint>().WithNone<PointNotAvailable>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Purchase , BarmanIndex , OrderBarman>().ForEach((Entity entity, in BarmanIndex barmanIndex  , in OrderBarman orderBarman) =>
            {
                AttemptToBuy(entity, barmanIndex, orderBarman);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void AttemptToBuy(Entity barmanEntity, BarmanIndex barmanIndex, OrderBarman order)
        {
            var storeRating = _storeRatingQuery.GetSingleton<StoreRating>();
            var audioSource = EntityManager.GetComponentObject<AudioSourceView>(barmanEntity).Value;
            var score = 0;
            var purchaseStore = 0;

            for (int i = 0; i < order.CompletedProduct.Length; i++)
            {
                var product = order.CompletedProduct[i];
                score = product.SellPrice * product.Count;
                purchaseStore += 1;
            }

            var customerUiEntity = EntityManager.GetComponentObject<CustomerView>(order.CustomerEntity).UiEntity;

            switch (purchaseStore)
            {
                case 0:
                    if (storeRating.CurrentValue > 0)
                    {
                        storeRating.SuccessPoints -= 1;
                    }
                    break;

                case > 0:
                {
                    storeRating.CurrentValue += 1; 
                    storeRating.SuccessPoints += 1;

                    var barmanTransformPosition =
                        EntityManager.GetComponentObject<TransformView>(barmanEntity).Value.position;
                    var profitUiPosition = barmanTransformPosition;
                    
                    profitUiPosition.y += CustomerAnimationConstants.ProfitOffsetY;
                    profitUiPosition.x -= CustomerAnimationConstants.ProfitOffsetX;

                    var spawnProfitUiEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponentObject(spawnProfitUiEntity,
                        new SpawnProfitUi
                        {
                            Profit = true,
                            Point = profitUiPosition,
                            Text = "+" + score
                        });

                    var bank = _bankQuery.GetSingleton<Bank>();
                    bank.Coins += score;
                    _bankQuery.SetSingleton(bank);
                    
                    var coinsAudio = EntityUtilities.GetGameConfig().AudioConfig.Coins;
                    var randomCoinsAudio = Random.Range(0, coinsAudio.Length);
                    
                    audioSource.PlayOneShot(coinsAudio[randomCoinsAudio]);
                    EntityManager.AddComponent<PleasedEmotionCustomer>(customerUiEntity);
                    break;
                }
            }

            _storeRatingQuery.SetSingleton(storeRating);
            
            EntityManager.AddComponent<MoveExit>(order.CustomerEntity);
            EntityManager.AddComponentData(order.CustomerEntity,
             new UpdatePurchaseQueuePosition { UpdateRow = barmanIndex.Value });
            EntityManager.AddComponentData(order.CustomerEntity,
                               new WaitTime { Current = BarmanAnimationConstants.ServiceTime });
            
            var panelArray = _phrasePanelUi.ToComponentArray<PhraseCustomerUiView>();

            foreach (var panel in panelArray)
            {
                if (panel.Value._customer == order.CustomerEntity)
                {
                    panel.Value.PanelFadeOut();
                }
            }
            
            if (EntityManager.HasComponent<PhraseSayCustomer>(order.CustomerEntity))
            {
                EntityManager.RemoveComponent<PhraseSayCustomer>(order.CustomerEntity);
            }
            
            EntityManager.RemoveComponent<Purchase>(barmanEntity);
            EntityManager.RemoveComponent<OrderBarman>(barmanEntity);
            EntityManager.AddComponent<FreeBarman>(barmanEntity);
        }

        private bool CheckCustomerInQueueRow( int row )
        {
            var purchaseCustomerArray = _purchaseQueueCustomerQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            var purchaseQueuePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
            var rowCount = purchaseQueuePoints.Select(point => point.Row).ToHashSet().Count;
            
            foreach (var indexCustomer in purchaseCustomerArray)
            {
                var rowCustomer = indexCustomer.Value % rowCount;
                
                if (rowCustomer == row)
                {
                    return false;
                }
            }

            return true;
        }
    }
}