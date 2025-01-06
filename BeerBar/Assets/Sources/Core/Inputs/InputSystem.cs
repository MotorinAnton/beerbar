using Unity.Entities;

namespace Core.Inputs
{
    public partial class InputSystem : SystemBase
    {
        protected override void OnCreate()
        {
            var actions = new PlayerActions();
            actions.Enable();
            
            var inputEntity = EntityManager.CreateSingleton<InputEntity>();
            EntityManager.AddComponentObject(inputEntity, new Actions{Value = actions});
        }

        protected override void OnUpdate() { }
        
        public PlayerActions GetActions()
        {
            var entity = SystemAPI.GetSingletonEntity<InputEntity>();
            var actions = EntityManager.GetComponentObject<Actions>(entity);
            return actions.Value;
        }
    }
}