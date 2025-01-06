/*
using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Characters;
using Core.Authoring.Customers;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.PhraseCustomerUi.System
{
    [RequireMatchingQueriesForUpdate]
    public partial class PhraseCustomerListSystem : SystemBase
    {
        private EntityQuery _phraseSayCustomerQuery;
        private EntityQuery _purchaseQueueCustomerQuery;
        private EntityQuery _phraseCustomerListQuery;

        protected override void OnCreate()
        {
            using var phraseSayCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phraseSayCustomerQuery = phraseSayCustomerBuilder.WithAll<Customer, PhraseSayCustomer>().WithNone<MoveExit, DissatisfiedCustomer>().Build(this);

            using var purchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchaseQueueCustomerQuery = purchaseQueueCustomerBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint>().WithNone<PhraseSayCustomer, MoveCharacter,DissatisfiedCustomer>().Build(this);
            
            
            using var phraseCustomerListBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phraseCustomerListQuery = phraseCustomerListBuilder
                .WithAll<PhraseCustomerUiManagerView>().Build(this);

            
        }

        protected override void OnUpdate()
        {
            var phraseManagerUiEntityArray = _phraseCustomerListQuery.ToEntityArray(Allocator.Temp);

            foreach (var phraseManagerUiEntity in phraseManagerUiEntityArray)
            {
                var phraseCustomerUiManagerView =
                    EntityManager.GetComponentObject<PhraseCustomerUiManagerView>(phraseManagerUiEntity);
                
                phraseCustomerUiManagerView.EnablePhrasePanelsUi();
                        
                var phraseCustomerList = phraseCustomerUiManagerView.CustomerList;
                var newCustomerList = NewCustomerList(phraseCustomerUiManagerView);
                
                //EntityManager.AddComponent<MovePhrasePanels>(phraseManagerUiEntity);
                //EntityManager.RemoveComponent<UpdatePhraseCustomerList>(phraseManagerUiEntity);
                        
                if (newCustomerList.Equals(phraseCustomerList))
                {
                    continue;
                }
                        
                var panels = phraseCustomerUiManagerView.PhraseCustomerUiManager.PhrasePanels;
                var phraseUiManager = phraseCustomerUiManagerView.PhraseCustomerUiManager;
                
                for (var i = 0; i < newCustomerList.Count; i++)
                {
                    if (phraseCustomerList.Contains(newCustomerList[i]))
                    {
                        continue;
                    }
                    
                    // foreach (var p in panels)
                    // {
                    //     if (newCustomerList[i] == p.Customer && i != p.Index)
                    //     {
                    //         var indexCustomer = newCustomerList.IndexOf(p.Customer);
                    //         phraseUiManager.PanelMoveUp(p, indexCustomer);
                    //         EntityManager.AddComponent<StartTweenPhraseCustomerUi>(phraseManagerUiEntity);
                    //     }
                    // }

                    if (!FreePanel(panels, newCustomerList, out var freePanel))
                    {
                        continue;
                    }
                    
                    var customerView = EntityManager.GetComponentObject<CustomerView>(newCustomerList[i]);
                    var imageProductArray = ImageProductArray(newCustomerList[i]);
                    var randomPhrase = Random.Range(0, customerView.Dialogs.PurchaseRequest.Length);
                    
                    freePanel.SetPhraseComponent(customerView.Dialogs.PurchaseRequest[randomPhrase], customerView.Avatar, imageProductArray,
                        newCustomerList[i], i);
                    phraseUiManager.PanelFadeIn(freePanel, i);
                    phraseCustomerUiManagerView.CustomerList = newCustomerList;
                }
                
                foreach (var panel in panels)
                {
                    var customerIndex = phraseCustomerUiManagerView.CustomerList.IndexOf(panel.Customer);

                    if (customerIndex != panel.Index && newCustomerList.Contains(panel.Customer))
                    {
                        phraseUiManager.PanelMoveUp(panel, customerIndex);
                    }
                            
                    if (!newCustomerList.Contains(panel.Customer) && panel.IsShow)
                    {
                        phraseUiManager.PanelFadeOut(panel);
                    }
                }
                
                
                EntityManager.AddComponent<StartTweenPhraseCustomerUi>(phraseManagerUiEntity);
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
        
        private List<Entity> NewCustomerList(PhraseCustomerUiManagerView phraseCustomerUiManagerView)
        {
            var phraseSayCustomerArray = _phraseSayCustomerQuery.ToEntityArray(Allocator.Temp);
            var purchaseQueueCustomerArray = _purchaseQueueCustomerQuery.ToEntityArray(Allocator.Temp);
            var panels = phraseCustomerUiManagerView.PhraseCustomerUiManager.PhrasePanels;

            var sortedPhraseCustomerList = SortCustomerIndexList(phraseSayCustomerArray);
            var sortedQueueCustomerList = SortCustomerIndexList(purchaseQueueCustomerArray);


            foreach (var customerEntity in sortedQueueCustomerList)
            {
                if (sortedPhraseCustomerList.Count < panels.Length)
                {
                    sortedPhraseCustomerList.Add(customerEntity);
                }
            }
            
            return sortedPhraseCustomerList;
        }

        private List<Entity> SortCustomerIndexList(NativeArray<Entity> customers)
        {
                        
            NativeArray<int> indices = new NativeArray<int>(customers.Length, Allocator.Temp);
        
            for (int i = 0; i < customers.Length; i++)
            {
                indices[i] = EntityManager.GetComponentData<IndexMovePoint>(customers[i]).Value;
            }
            
            for (int i = 0; i < customers.Length - 1; i++)
            {
                for (int j = 0; j < customers.Length - 1 - i; j++)
                {
                    if (indices[j] > indices[j + 1])
                    {
                        (indices[j], indices[j + 1]) = (indices[j + 1], indices[j]);
                        (customers[j], customers[j + 1]) = (customers[j + 1], customers[j]);
                    }
                }
            }
            List<Entity> sortedEntities = new List<Entity>();
            
            for (int i = 0; i < customers.Length; i++)
            {
                sortedEntities.Add(customers[i]);
            }
            
            return sortedEntities;
        }
        
        private bool FreePanel(PhraseCustomerUiAuthoring[] panels,
            List<Entity> newCustomerList, out PhraseCustomerUiAuthoring freePanel)
        {
            foreach (var panel in panels)
            {
                if (newCustomerList.Contains(panel.Customer))
                {
                    continue;
                }

                freePanel = panel;
                return true;
            }

            freePanel = null;
            return false;
        }
    }
}
*/

