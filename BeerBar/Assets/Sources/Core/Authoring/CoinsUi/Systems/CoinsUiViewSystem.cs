using Core.Authoring.Banks;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.CoinsUi.Systems
{
    public partial class CoinsUiViewSystem : SystemBase
    {
        private EntityQuery _bankQuery;

        protected override void OnCreate()
        {
            using var bankBuilder = new EntityQueryBuilder(Allocator.Temp);
            _bankQuery = bankBuilder.WithAll<Bank>().Build(this);
        }
        
        protected override void OnUpdate()
        {
            Entities.WithAll<CoinsUiView>().ForEach((Entity entity, in CoinsUiView coinsUIView) =>
            {
                ChangeText(entity, coinsUIView);
                
            }).WithoutBurst().Run();
        }

        private void ChangeText(Entity entity, in CoinsUiView coinsUIView)
        {
            var bank = _bankQuery.GetSingleton<Bank>();

            coinsUIView.Text.text = bank.Coins.ToString();
        }
    }
}