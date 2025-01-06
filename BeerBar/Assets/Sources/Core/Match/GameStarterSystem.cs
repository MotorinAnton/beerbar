/*using Core.Characters;
using Core.Characters.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Content;
using UnityEngine;

namespace Core.Match
{
    public partial struct GameStarterSystem : ISystem // чтобы запускать инициализация игры происходила с лбюбой сцены
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            foreach (var (gameStarter, entity) in SystemAPI.Query<RefRO<GameStarter>>().WithEntityAccess())
            {
                var barmanPrefab = gameStarter.ValueRO.Barman;
                var barmanEntity = state.EntityManager.Instantiate(barmanPrefab);
                ecb.AddComponent<Barman>(barmanEntity);
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}*/