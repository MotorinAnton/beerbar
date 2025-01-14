using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Customers;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.PhraseCustomerUi.System
{
    public partial class PhraseCustomerViewSystem : SystemBase
    {
        private EntityQuery _phraseSayCustomerQuery;
        private EntityQuery _purchaseQueueCustomerQuery;

        protected override void OnCreate()
        {
            using var phraseSayCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phraseSayCustomerQuery = phraseSayCustomerBuilder.WithAll<Customer, PhraseSayCustomer>().Build(this);

            using var purchaseQueueCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchaseQueueCustomerQuery = purchaseQueueCustomerBuilder
                .WithAll<Customer, PurchaseQueueCustomer, IndexMovePoint>().WithNone<PhraseSayCustomer>().Build(this);
        }

        protected override void OnUpdate()
        {
            /*Entities.WithAll<PhraseCustomerUiManagerView>().WithNone<StartTweenPhraseCustomerUi, TweenProcessing>()
                .ForEach(
                    (Entity entity, in PhraseCustomerUiManagerView phraseCustomerUiManagerView) =>
                    {
                        if (_phraseSayCustomerQuery.IsEmpty)
                        {
                            return;
                        }
                        
                        phraseCustomerUiManagerView.EnablePhrasePanelsUi();
                        
                        var phraseCustomerList = phraseCustomerUiManagerView.CustomerList;
                        var newCustomerList = NewCustomerList(phraseCustomerUiManagerView);
                        
                        if (new HashSet<Entity>(newCustomerList).SetEquals(phraseCustomerList))
                        {
                            return;
                        }
                        
                        var panels = phraseCustomerUiManagerView.PhraseCustomerUiManager.PhrasePanels;
                        var phraseUiManager = phraseCustomerUiManagerView.PhraseCustomerUiManager;
                        
                        for (var i = 0; i < newCustomerList.Count; i++)
                        {
                            var panel = NewPanel(panels);
                            
                            if (panel.Customer != newCustomerList[i] && !phraseCustomerList.Contains(newCustomerList[i]))
                            {
                                var customerView = EntityManager.GetComponentObject<CustomerView>(newCustomerList[i]);
                                var imageProductArray = ImageProductArray(newCustomerList[i]);
                                var randomPhrase = Random.Range(0, customerView.Dialogs.PurchaseRequest.Length);
                                    
                                panel.SetPhraseComponent(customerView.Dialogs.PurchaseRequest[randomPhrase], customerView.Avatar, imageProductArray,
                                    newCustomerList[i], i);
                                phraseUiManager.PanelFadeIn(panel, i);
                            }
                        }

                        phraseCustomerUiManagerView.CustomerList = newCustomerList;
                        
                        //EntityManager.AddComponent<StartTweenPhraseCustomerUi>(entity);

                    }).WithoutBurst().WithStructuralChanges().Run();
            
            
            Entities.WithAll<PhraseCustomerUiManagerView>().WithNone<StartTweenPhraseCustomerUi, TweenProcessing>().ForEach(
                (Entity entity, in PhraseCustomerUiManagerView phraseCustomerUiManagerView) =>
                {
                    var customerList = phraseCustomerUiManagerView.CustomerList;
                    var panels = phraseCustomerUiManagerView.PhraseCustomerUiManager.PhrasePanels;
                    var panelManager = phraseCustomerUiManagerView.PhraseCustomerUiManager;

                    if (customerList.Count == 0 )
                    {
                        return;
                    }
                    
                    foreach (var panel in panels)
                    {
                        var customerIndex = phraseCustomerUiManagerView.CustomerList.IndexOf(panel.Customer);
                        
                        if (customerIndex != panel.Index && customerList.Contains(panel.Customer))
                        {
                            panelManager.PanelMoveUp(panel, customerIndex);
                            EntityManager.AddComponent<StartTweenPhraseCustomerUi>(entity);
                        }
                        
                        if (!customerList.Contains(panel.Customer) || panel.Customer == default)
                        {
                            panel.DisablePanel();
                            panel.IsShow = false;
                        }
                    }
                }).WithoutBurst().WithStructuralChanges().Run();*/

            
            /*Entities.WithAll<PhraseCustomerUiManagerView, StartTweenPhraseCustomerUi>().WithNone<TweenProcessing>()
                .ForEach(
                    (Entity entity, in PhraseCustomerUiManagerView phraseCustomerUiManagerView) =>
                    {
                        var phraseUiManager = phraseCustomerUiManagerView.PhraseCustomerUiManager;
                       
                        if (phraseUiManager.Sequence.Count > 0)
                        {
                            foreach (var sequence in phraseUiManager.Sequence)
                            {
                                sequence.Play();
                                phraseUiManager.Sequence.Remove(sequence);
                                EntityManager.AddComponent<TweenProcessing>(entity);
                                return;
                            }
                        }
                        EntityManager.RemoveComponent<TweenProcessing>(entity);
                        EntityManager.RemoveComponent<StartTweenPhraseCustomerUi>(entity);

                    }).WithoutBurst().WithStructuralChanges().Run()*/;
        }

        private  Sprite[] ImageProductArray(Entity customerEntity)
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

        private List<Entity> UpdateCustomerList(PhraseCustomerUiManagerView phraseCustomerUiManagerView)
        {
            var phraseSayCustomerArray = _phraseSayCustomerQuery.ToEntityArray(Allocator.Temp);
            var purchaseQueueCustomerArray = _purchaseQueueCustomerQuery.ToEntityArray(Allocator.Temp);
            var panels = phraseCustomerUiManagerView.PhraseCustomerUiManager.PhrasePanels;
            var phraseCustomerList = new List<Entity>();
            var customerCount = 0;

            foreach (var phraseSayCustomer in phraseSayCustomerArray)
            {
                phraseCustomerList.Add(phraseSayCustomer);
                customerCount += 1;
            }
            
            if (customerCount < panels.Length)
            {
                if (!_purchaseQueueCustomerQuery.IsEmpty)
                {
                    foreach (var phraseSayCustomer in purchaseQueueCustomerArray)
                    {
                        if (customerCount < panels.Length)
                        {
                            phraseCustomerList.Add(phraseSayCustomer);
                            customerCount += 1;
                        }
                    }
                }
            }
            
            return phraseCustomerList;
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

        
        private PhraseCustomerUiAuthoring NewPanel(PhraseCustomerUiAuthoring[] panels)
        {
            foreach (var panel in panels)
            {
                if (!panel.IsShow)
                {
                    return panel;
                }
            }

            return default;
        }
        

        private PhraseCustomerUiAuthoring FoundPanel(List<PhraseCustomerUiAuthoring> panels)
        {

            foreach (var panel in panels)
            {
                if (!panel.IsShow && !panel.gameObject.activeInHierarchy)
                {
                    return panel;
                }
            }

            return default;
        }
    }
}

