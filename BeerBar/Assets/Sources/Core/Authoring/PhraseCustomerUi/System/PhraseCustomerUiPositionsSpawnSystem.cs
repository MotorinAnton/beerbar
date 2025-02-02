using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Core.Authoring.PhraseCustomerUi.System
{
    [RequireMatchingQueriesForUpdate]
    public partial class PhraseCustomerUiPositionsSpawnSystem : SystemBase
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
            var phraseCustomerUiView = Object.Instantiate(spawnPhraseCustomerUi.PhraseCustomerUiPrefab);
            
            EntityManager.SetName(phraseCustomerUi, EntityConstants.PhraseCustomerUiManagerName);
            EntityManager.AddComponentObject(phraseCustomerUi,
                new SpawnRootCanvasChild { Transform = phraseCustomerUiView.transform });
            EntityManager.AddComponentObject(phraseCustomerUi,
                new PhraseCustomerUiPositionView { Positions = phraseCustomerUiView });
            phraseCustomerUiView.Initialize(EntityManager, phraseCustomerUi);
            EntityManager.DestroyEntity(entity);
        }
    }
}