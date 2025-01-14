/*
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
    [RequireMatchingQueriesForUpdate]
    public partial class PhraseCustomerMovePositionSystem : SystemBase
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
            Entities.WithAll<PhraseCustomerUiManagerView, MovePhrasePanels>().WithNone<TweenProcessing>().ForEach(
                    (Entity entity, in PhraseCustomerUiManagerView phraseCustomerUiManagerView) =>
                    {
                        var customerList = phraseCustomerUiManagerView.CustomerList;
                        var panels = phraseCustomerUiManagerView.PhraseCustomerUiManager.PhrasePanels;
                        var panelManager = phraseCustomerUiManagerView.PhraseCustomerUiManager;

                        foreach (var panel in panels)
                        {
                            var customerIndex = phraseCustomerUiManagerView.CustomerList.IndexOf(panel.Customer);

                            if (customerIndex != panel.Index && customerList.Contains(panel.Customer))
                            {
                                panelManager.PanelMoveUp(panel, customerIndex);
                            }
                            
                            if (!customerList.Contains(panel.Customer) && panel.IsShow)
                            {
                                panelManager.PanelFadeOut(panel);
                            }
                        }

                        EntityManager.RemoveComponent<MovePhrasePanels>(entity);
                        EntityManager.AddComponent<UpdatePhraseCustomerList>(entity);

                    }).WithoutBurst().WithStructuralChanges().Run();
        }
    }
}
*/

