using System.Linq;
using Core.Authoring.Characters;
using Core.Authoring.Customers;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.PhraseCustomerUi.System
{
    [RequireMatchingQueriesForUpdate]
    public partial class PhraseCustomerSpawnSystem : SystemBase
    {
        private EntityQuery _phraseSayCustomerQuery;
        private EntityQuery _purchaseQueueCustomerQuery;
        private EntityQuery _spawnPhrasePanelCustomerUiQuery;
        private EntityQuery _phrasePanelCustomerUiQuery;
        private EntityQuery _phrasePanelCustomerUiManagerQuery;

        protected override void OnCreate()
        {
            using var phraseSayCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phraseSayCustomerQuery = phraseSayCustomerBuilder.WithAll<Customer, PhraseSayCustomer>().WithNone<MoveExit, DissatisfiedCustomer>().Build(this);

            using var purchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchaseQueueCustomerQuery = purchaseQueueCustomerBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint>().WithNone<PhraseSayCustomer, MoveCharacter,DissatisfiedCustomer>().Build(this);

            using var spawnPhrasePanelCustomerUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPhrasePanelCustomerUiQuery = spawnPhrasePanelCustomerUiBuilder
                .WithAll<SpawnPhrasePanelCustomerUi>().Build(this);
            
            using var phrasePanelCustomerUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phrasePanelCustomerUiQuery = phrasePanelCustomerUiBuilder
                .WithAll<PhrasePanelCustomerUi>().WithNone<PanelFadeOut>().Build(this);
            
            using var phrasePanelCustomerUiManagerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phrasePanelCustomerUiManagerQuery = phrasePanelCustomerUiManagerBuilder
                .WithAll<PhraseCustomerUiPositionView>().Build(this);
        }

        protected override void OnUpdate()
        {
            var spawnPhrasePanelArray = _spawnPhrasePanelCustomerUiQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in spawnPhrasePanelArray)
            {
                var spawnPhrasePanel = EntityManager.GetComponentObject<SpawnPhrasePanelCustomerUi>(entity);
                var config = EntityUtilities.GetUIConfig();
                var panelEntity = EntityManager.CreateEntity();
                var phrasePositionsEntity = _phrasePanelCustomerUiManagerQuery.ToEntityArray(Allocator.Temp)[0];
                var phrasePositions = EntityManager.GetComponentObject<PhraseCustomerUiPositionView>(phrasePositionsEntity).Positions;
                var panel = Object.Instantiate(config.PhraseCustomerUiPrefab,phrasePositions.transform);
                var customerView = EntityManager.GetComponentObject<CustomerView>(spawnPhrasePanel.Customer);
                var imageProductArray = ImageProductArray(spawnPhrasePanel.Customer);
                var randomPhraseIndex = Random.Range(0, customerView.Dialogs.PurchaseRequest.Length);
                var phraseText = customerView.Dialogs.PurchaseRequest[randomPhraseIndex];
                var phrasePanelArray = _phrasePanelCustomerUiQuery.ToEntityArray(Allocator.Temp);
                
                panel.SetPhraseComponent(phraseText, customerView.Avatar, imageProductArray, spawnPhrasePanel.Customer,
                    spawnPhrasePanel.Index, phrasePanelArray.Length);

                EntityManager.AddComponentObject(panelEntity, new PhraseCustomerUiView
                {
                    Value = panel
                });

                panel.PanelFadeIn(phrasePositions.PanelPonts[spawnPhrasePanel.Index].localPosition);
                EntityManager.SetName(panelEntity, EntityConstants.PhraseCustomerUiName);
                EntityManager.AddComponent<PhrasePanelCustomerUi>(panelEntity);
                panel.Initialize(EntityManager, panelEntity);
                EntityManager.DestroyEntity(entity);
            }
        }

        private Sprite[] ImageProductArray(Entity customerEntity)
        {
            var config = EntityUtilities.GetGameConfig().ProductConfig.Products;
            
            var customerProducts = EntityManager.GetComponentObject<CustomerProduct>(customerEntity).Products;
            var productSpriteArray = new Sprite[customerProducts.Length];

            for (var index = 0; index < customerProducts.Length; index++)
            {
                var productCustomer = customerProducts[index];
                var spriteProduct = config.FirstOrDefault(product =>
                        product.ProductType == productCustomer.ProductType && product.Level == productCustomer.Level)
                    .Visual;
                productSpriteArray[index] = spriteProduct;
            }

            return productSpriteArray;
        }
    }
}

