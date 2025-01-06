using Core.Authoring.SelectGameObjects;
using Core.Authoring.TVs;
using Unity.Entities;

namespace Core.Authoring.EventObjects.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class TVClickedSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<TVEntity , TVView, Clicked>().WithNone<Breakdown>().ForEach((Entity entity, in TVView tvView) =>
                {
                    NextСhanel(entity, tvView);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
            
        }

        private void NextСhanel(Entity tVEntity, TVView tvView)
        {
            if (tvView.Chanal == tvView.Value.Chanel.Length - 1 )
            {
                tvView.Chanal = 0;
                tvView.Value.OnRenderer.material = tvView.Value.Chanel[0];
                EntityManager.RemoveComponent<Clicked>(tVEntity);
                return;
            }
            
            tvView.Chanal += 1;
            tvView.Value.OnRenderer.material = tvView.Value.Chanel[tvView.Chanal];
            EntityManager.RemoveComponent<Clicked>(tVEntity);
        }
    }
}