using System.Collections.Generic;
using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Core.Authoring.PhraseCustomerUi.System
{
    [RequireMatchingQueriesForUpdate]
    public partial class PhraseCustomerUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnPhraseCustomerUiManager>().ForEach((Entity entity, in SpawnPhraseCustomerUiManager spawnCoinsUI) =>
            {
                SpawnPhraseCustomerUi(entity, spawnCoinsUI);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnPhraseCustomerUi(Entity entity, in SpawnPhraseCustomerUiManager spawnPhraseCustomerUi)
        {

            var phraseCustomerUi = EntityManager.CreateEntity();
            EntityManager.SetName(phraseCustomerUi, EntityConstants.PhraseCustomerUiName);
            
            var phraseCustomerUiView = Object.Instantiate(spawnPhraseCustomerUi.PhraseCustomerUiPrefab);
            
            phraseCustomerUiView.gameObject.SetActive(false);
            
            foreach (var panel in phraseCustomerUiView.PhrasePanels)
            {
                panel.DisablePanel();
            }
            phraseCustomerUiView.EventPanel.DisablePanel();


            var customerList = new List<Entity>();
            
            
            EntityManager.AddComponentObject(phraseCustomerUi,
                new SpawnRootCanvasChild { Transform = phraseCustomerUiView.transform });
            EntityManager.AddComponentObject(phraseCustomerUi,
                new PhraseCustomerUiManagerView
                {
                    PhraseCustomerUiManager = phraseCustomerUiView, CustomerList = customerList
                });
            EntityManager.AddComponent<UpdatePhraseCustomerList>(phraseCustomerUi);
            phraseCustomerUiView.Initialize(EntityManager, phraseCustomerUi);
            EntityManager.DestroyEntity(entity);
        }
    }
}