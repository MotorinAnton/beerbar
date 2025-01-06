using Core.Authoring.SelectGameObjects;
using Core.Utilities;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.ParametersButtonUi
{
    public class ParametersButtonUiAuthoring : EntityBehaviour
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

    public struct ParametersButtonUi : IComponentData { }

    public class SpawnParametersButtonUi : IComponentData
    {
        public ParametersButtonUiAuthoring ParametersButtonUiPrefab;
    }
}