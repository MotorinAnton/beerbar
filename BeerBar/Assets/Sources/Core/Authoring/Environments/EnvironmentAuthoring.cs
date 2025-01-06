using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Environments
{
    public class EnvironmentAuthoring : MonoBehaviour 
    {
        public class  EnvironmentBaker : Baker<EnvironmentAuthoring>
        {
            public override void Bake(EnvironmentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponentObject(entity, new EnvironmentView { Environment = authoring });
            }
        }
    }
    public class EnvironmentView : IComponentData
    {
        public EnvironmentAuthoring Environment;
    }
}