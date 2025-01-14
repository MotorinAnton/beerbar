using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.RootCanvas.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class RootCanvasChildReorderSystem : SystemBase
    {
        private EntityQuery _reorderRootCanvasQuery;

        private EntityQuery _rootCanvasChildrenQuery;

        protected override void OnCreate()
        {
            using var reorderRootCanvasBuilder = new EntityQueryBuilder(Allocator.Temp);
            _reorderRootCanvasQuery = reorderRootCanvasBuilder.WithAll<RootCanvas, ReorderRootCanvas>().Build(this);

            using var reorderRootCanvasChildrenBuilder = new EntityQueryBuilder(Allocator.Temp);
            _rootCanvasChildrenQuery = reorderRootCanvasChildrenBuilder.WithAll<RootCanvasChild>().Build(this);
        }

        protected override void OnUpdate()
        {
            if (_reorderRootCanvasQuery.IsEmpty)
            {
                return;
            }

            if (_rootCanvasChildrenQuery.IsEmpty)
            {
                return;
            }

            var rootCanvasChildren = _rootCanvasChildrenQuery.ToComponentArray<RootCanvasChild>();

            rootCanvasChildren = rootCanvasChildren.OrderBy(x => x.SortingOrder).ToArray();

            foreach (var child in rootCanvasChildren)
            {
                child.Transform.SetSiblingIndex(child.SortingOrder);
            }
        }
    }
}