using TMPro;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.CoinsUi
{
    public sealed class CoinsUiAuthoring : EntityBehaviour
    {
        public TMP_Text Text => _text;

        public int SortingOrder => _sortingOrder;

        [SerializeField]
        private TMP_Text _text;

        [SerializeField]
        private int _sortingOrder;
    }

    public struct CoinsUi : IComponentData { }

    public class SpawnCoinsUi : IComponentData
    {
        public CoinsUiAuthoring CoinsUiPrefab;
    }

    public class CoinsUiView : IComponentData
    {
        public TMP_Text Text;
    }
}