using Core.Authoring.Banks;
using Core.Authoring.Bartenders;
using Core.Authoring.Customers;
using Core.Authoring.Customers.CustomersUi;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.ProfitUi;
using Core.Authoring.StoreRatings;
using Core.Components.Wait;
using Core.Constants;
using Core.Utilities;
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
        
        protected override void OnCreate()
        {
            using var bankBuilder = new EntityQueryBuilder(Allocator.Temp);
            _bankQuery = bankBuilder.WithAllRW<Bank>().Build(this);
            
            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAllRW<StoreRating>().Build(this);
            
            using var phraseUiManagerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phraseUiManager = phraseUiManagerBuilder.WithAll<PhraseCustomerUiManagerView>().Build(this);
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
            var customerProducts = EntityManager.GetComponentObject<CustomerProduct>(order.CustomerEntity);
            var orderProduct = order.CompletedProduct;
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
            var customerUiView = EntityManager.GetComponentObject<CustomerUiView>(customerUiEntity).Value;

            switch (purchaseStore)
            {
                case 0:
                    if (storeRating.CurrentValue > 0)
                    {
                        //storeRating.CurrentValue -= 1;
                        storeRating.SuccessPoints -= 1;
                        //customerUiView.FaceEmotionImage.overrideSprite = customerUiView.FaceEmotionSprites.Displeased;
                    }

                    break;

                case > 0:
                {
                    storeRating.CurrentValue += 1; 
                    storeRating.SuccessPoints += 1;

                    var barmanTransformPosition =
                        EntityManager.GetComponentObject<TransformView>(barmanEntity).Value.position;
                    var profitUiPosition = barmanTransformPosition;
                    profitUiPosition.y += 2f;
                    profitUiPosition.x -= 1f;

                    var spawnProfitUiEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponentObject(spawnProfitUiEntity,
                        new SpawnProfitUi { Type = ProfitUiType.Profit, Point = profitUiPosition, Text = "+" + score });

                    var bank = _bankQuery.GetSingleton<Bank>();
                    bank.Coins += score;
                    _bankQuery.SetSingleton(bank);
                 
                    EntityManager.AddComponent<PleasedEmotionCustomer>(customerUiEntity);
                    
                    var coinsAudio = EntityUtilities.GetGameConfig().AudioConfig.Coins;
                    var randomCoinsAudio = Random.Range(0, coinsAudio.Length);
                    audioSource.PlayOneShot(coinsAudio[randomCoinsAudio]);
                    break;
                }
            }

            _storeRatingQuery.SetSingleton(storeRating);
            
            EntityManager.AddComponent<MoveExit>(order.CustomerEntity);
            EntityManager.AddComponentData(order.CustomerEntity,
             new UpdatePurchaseQueuePosition { UpdateRow = barmanIndex.Value });
            EntityManager.AddComponentData(order.CustomerEntity,
                               new WaitTime { Current = BarmanAnimationConstants.ServiceTime });
            
            if (!_phraseUiManager.IsEmpty)
            {
                var phraseUiManagerEntity = _phraseUiManager.ToEntityArray(Allocator.Temp)[0];
                        
                var phraseUiManagerView =
                    EntityManager.GetComponentObject<PhraseCustomerUiManagerView>(phraseUiManagerEntity);
                        
                var phraseUiManager =
                    EntityManager.GetComponentObject<PhraseCustomerUiManagerView>(phraseUiManagerEntity).PhraseCustomerUiManager;

                phraseUiManagerView.CustomerList.Remove(order.CustomerEntity);

                //EntityManager.RemoveComponent<StartTweenPhraseCustomerUi>(phraseUiManagerEntity);
                //EntityManager.RemoveComponent<TweenProcessing>(phraseUiManagerEntity);
              
                
                // foreach (var panel in phraseUiManager.PhrasePanels)
                // {
                //     if (panel.Customer == order.CustomerEntity)
                //     {
                //         // phraseUiManager.PanelFadeOut(panel); 
                //         // EntityManager.AddComponent<StartTweenPhraseCustomerUi>(phraseUiManagerEntity);
                //         phraseUiManagerView.CustomerList.Remove(order.CustomerEntity);
                //     }
                // }
            }
            
            if (EntityManager.HasComponent<PhraseSayCustomer>(order.CustomerEntity))
            {
                EntityManager.RemoveComponent<PhraseSayCustomer>(order.CustomerEntity);
            }
            
            EntityManager.RemoveComponent<Purchase>(barmanEntity);
            EntityManager.RemoveComponent<OrderBarman>(barmanEntity);
            EntityManager.AddComponent<FreeBarman>(barmanEntity);
        }
    }
}