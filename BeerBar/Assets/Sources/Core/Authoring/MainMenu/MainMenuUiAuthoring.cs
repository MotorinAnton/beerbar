using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.MainMenu
{
    public class MainMenuUiAuthoring : EntityBehaviour
    {
        [SerializeField]
        public Button _playButton;
        
        private void Start()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = manager.CreateEntity();
            manager.AddComponentObject(entity, new MainMenuView
            {
                Value = this
            });
            Initialize(manager, entity);
        }
    }

    public class MainMenuView : IComponentData
    {
        public MainMenuUiAuthoring Value;
    }
}