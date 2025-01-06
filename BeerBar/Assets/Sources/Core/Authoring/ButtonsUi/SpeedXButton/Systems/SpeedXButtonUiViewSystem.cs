using Core.Authoring.SelectGameObjects;
using Core.Constants;
using Unity.Entities;

namespace Core.Authoring.ButtonsUi.SpeedXButton.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class AddButtonUiViewSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll< SpeedXButtonUiView>()
                .ForEach((in SpeedXButtonUiView speedXButtonUiView) =>
                {
                    CheckSpeedXButtonUi(speedXButtonUiView);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
            
            Entities.WithAll<SpeedXButtonUiView>().WithAll<Clicked>()
                .ForEach((Entity entity, in SpeedXButtonUiView speedXButtonUiView) =>
                {
                    SpeedXButtonClicked(entity, speedXButtonUiView);
                    
                }).WithoutBurst().WithStructuralChanges().Run();
            
        }

        private void CheckSpeedXButtonUi(in SpeedXButtonUiView speedXButtonUiView)
        {
            speedXButtonUiView.SpeedXButtonUiAuthoring.Text.text = ">>X" + speedXButtonUiView.SpeedX;
        }

        private void SpeedXButtonClicked(Entity entity, SpeedXButtonUiView speedXButtonUiView)
        {
            speedXButtonUiView.SpeedX *= ButtonsConstants.MultiplierSpeedGame;

            if (speedXButtonUiView.SpeedX > ButtonsConstants.MaxMultiplierSpeedGame)
            {
                speedXButtonUiView.SpeedX = 1;
            }
            
            UnityEngine.Time.timeScale = speedXButtonUiView.SpeedX;

            EntityManager.RemoveComponent<Clicked>(entity);
        }
    }
}