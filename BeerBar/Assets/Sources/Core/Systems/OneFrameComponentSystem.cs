using Unity.Collections;
using Unity.Entities;

namespace Core.Systems
{
    // TODO: Удостовериться, что именно в этой группе должно выполняться
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    //[UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public partial class OneFrameComponentSystem : SystemBase
    {
        private EntityCommandBuffer _ecb;

        protected override void OnCreate()
        {
            _ecb = new EntityCommandBuffer(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _ecb.Dispose();
        }

        protected override void OnUpdate()
        {
            if (_ecb.IsEmpty)
            {
                return;
            }

            _ecb.Playback(EntityManager);
            _ecb.Dispose();

            _ecb = new EntityCommandBuffer(Allocator.Persistent);
        }

        // TODO: По аналогии с EntityManager добавить методы с другими сигнатурами 
        // Нужно ли добавлять вариант с AddComponentData? Или мы такое только для теговых компонентов будем использовать?
        public void AddOneFrameComponent<T>(Entity entity)
        {
            AddOneFrameComponent(entity, ComponentType.ReadWrite<T>());
        }

        public void AddOneFrameComponent(Entity entity, ComponentType componentType)
        {
            _ecb.RemoveComponent(entity, componentType);
            EntityManager.AddComponent(entity, componentType);
        }

        public void AddOneFrameComponent<T>(NativeArray<Entity> entities)
        {
            AddOneFrameComponent(entities, ComponentType.ReadWrite<T>());
        }

        public void AddOneFrameComponent(NativeArray<Entity> entities, ComponentType componentType)
        {
            _ecb.RemoveComponent(entities, componentType);
            EntityManager.AddComponent(entities, componentType);
        }

        public void AddOneFrameComponentData<T>(Entity entity, T componentData) where T : unmanaged, IComponentData
        {
            _ecb.RemoveComponent<T>(entity);
            EntityManager.AddComponentData(entity, componentData);
        }
    }
}