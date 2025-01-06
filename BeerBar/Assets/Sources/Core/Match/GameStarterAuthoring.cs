/*using Core.Characters;
using Core.Characters.Authoring;
using Unity.Entities;
using UnityEngine;

namespace Core.Match
{
    public class GameStarterAuthoring : MonoBehaviour
    {
        public BarmanAuthoring Barman;
        
        public class GameStarterBaker : Baker<GameStarterAuthoring>
        {
            public override void Bake(GameStarterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var barmanEntity = GetEntity(authoring.Barman, TransformUsageFlags.Dynamic);
                AddComponent(entity, new GameStarter { Barman = barmanEntity });
            }
        }
    }

    public struct GameStarter : IComponentData
    {
        public Entity Barman;
    }
}*/