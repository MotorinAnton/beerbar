using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Entities;

namespace Core.Authoring.NoteBookShops
{
    public sealed class NoteBookShopAuthoring : SelectAuthoring<RendererSelectAuthoring>
    {
        
    }
    public class SpawnNoteBookShop : IComponentData
    {
        public NoteBookShopAuthoring NoteBookShopPrefab;
    }
    
    public class NoteBookShopView : IComponentData
    {
        public NoteBookShopAuthoring Value;
    }
}