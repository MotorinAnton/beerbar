using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.RootCanvas
{
    public sealed class RootCanvasAuthoring : EntityBehaviour { }

    public class RootCanvasView : IComponentData
    {
        public RootCanvasAuthoring RootCanvasAuthoring;
    }

    public struct RootCanvas : IComponentData { }

    public struct ReorderRootCanvas : IComponentData { }

    public class SpawnRootCanvas : IComponentData
    {
        public RootCanvasAuthoring RootCanvasPrefab;
    }

    public class SpawnRootCanvasChild : IComponentData
    {
        public Transform Transform;
        public int SortingOrder;
    }

    public class RootCanvasChild : IComponentData
    {
        public Transform Transform;
        public int SortingOrder;
    }
}