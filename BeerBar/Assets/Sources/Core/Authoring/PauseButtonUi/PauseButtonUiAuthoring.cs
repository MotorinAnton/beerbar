using Core.Authoring.SelectGameObjects;
using Core.Utilities;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.PauseButtonUi
{
    public class PauseButtonUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => _sortingOrder;

        [SerializeField]
        private Button _button;

        [SerializeField]
        private int _sortingOrder;

        private void Start()
        {
            _button.onClick.AddListener(() => EntityUtilities.AddOneFrameComponent<Clicked>(Entity));
        }
    }
    
    public struct PauseButtonUi : IComponentData { }

    public class SpawnPauseButtonUi : IComponentData
    {
        public PauseButtonUiAuthoring PauseButtonUiPrefab;
    }
}