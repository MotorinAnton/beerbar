using Core.Scenes.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Content;
using Unity.Loading;
using UnityEngine.SceneManagement;

namespace Core.Scenes.Systems
{
    public partial struct SceneLoadSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            foreach (var (loadScene, entity) in SystemAPI.Query<RefRO<LoadScene>>().WithNone<LoadSceneProcess>().WithEntityAccess())
            {
                var scene = RuntimeContentManager.LoadSceneAsync(loadScene.ValueRO.Reference.Id,
                    new ContentSceneParameters
                    {
                        loadSceneMode = LoadSceneMode.Single
                    });
                
                ecb.AddComponent(entity, new LoadSceneProcess{Scene = scene});
            }

            foreach (var (loadScene, entity) in SystemAPI.Query<RefRO<LoadSceneProcess>>().WithNone<SceneLoaded>().WithEntityAccess())
            {
                if (!loadScene.ValueRO.Scene.isLoaded)
                {
                    return;
                }
                
                ecb.AddComponent<SceneLoaded>(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}