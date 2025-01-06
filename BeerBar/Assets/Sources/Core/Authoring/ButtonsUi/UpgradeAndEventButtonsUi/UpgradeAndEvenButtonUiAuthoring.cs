using System.Collections.Generic;
using Core.Configs;
using Core.Utilities;
using DG.Tweening;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Authoring.UpgradeAndEventButtonsUi
{
    public class UpgradeAndEvenButtonUiAuthoring : EntityBehaviour
    {
        [SerializeField] private Button _upgradeButton;
        
        public Button UpgradeButton => _upgradeButton;
        
        [SerializeField] private Button _eventButton;
        
        public Button EventButton => _eventButton;
        private bool _isPointerEnter;
        private List<Sequence> _sequence;
        private Vector3 _upgradeButtonPosition;
        private Vector3 _eventButtonPosition;

        private void Start()
        {
            _upgradeButton.onClick.AddListener(UpgradeButtonClicked);
            _eventButton.onClick.AddListener(EventButtonClicked);
            _sequence = new List<Sequence>();
            _upgradeButtonPosition = UpgradeButton.transform.localPosition;
            _eventButtonPosition = EventButton.transform.localPosition;
        }

        private void UpgradeButtonClicked()
        {
            EntityUtilities.AddOneFrameComponent<UpgradeButtonClicked>(Entity);
        }
        
        private void EventButtonClicked()
        {
            EntityUtilities.AddOneFrameComponent<EventButtonClicked>(Entity);
        }

        public void Select()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            _sequence = new List<Sequence>();
            gameObject.SetActive(true);
        }
        public void CreateFadeInSequence()
        {
            gameObject.SetActive(true);
            UpgradeButton.gameObject.SetActive(true);
            EventButton.gameObject.SetActive(true);
            
            UpgradeButton.transform.localPosition = _upgradeButtonPosition;
            EventButton.transform.localPosition = _eventButtonPosition;
            
            _sequence.Add(FadeInButton(UpgradeButton));
            _sequence.Add(FadeInButton(EventButton));
        }

        private Sequence FadeInButton(Button button)
        {
            button.transform.localScale = new Vector3(0f, 0f, 0f);
            var newSequence = DOTween.Sequence();
            var tweenScale = button.transform.DOScale(new Vector3(1.3f,1.3f,1.3f), 0.4f);
            newSequence.Join(tweenScale);
            return newSequence;
        }
    }
    
    public struct UpgradeButtonClicked : IComponentData { }
    
    public struct EventButtonClicked : IComponentData { }

    public class UpButtonAvailable : IComponentData
    {
        public List<Up> List;
    }

    public struct UpgradeAndEventButtonUi : IComponentData
    {
        public Entity Entity;
    }
    
    public class SpawnUpgradeAndEvenButtonUi : IComponentData
    {
        public Entity ObjectEntity;
    }
    
    public class UpgradeAndEvenButtonUiView : IComponentData
    {
        public UpgradeAndEvenButtonUiAuthoring UpgradeAndEventButton;
        public Entity ObjectEntity;

        public void SetTextRepairsEventButton() => UpgradeAndEventButton.EventButton.GetComponentInChildren<TMP_Text>().text = "Repairs";
        public void SetTextClearEventButton() => UpgradeAndEventButton.EventButton.GetComponentInChildren<TMP_Text>().text = "Clear";
        
        public void SetTextReplenishEventButton() => UpgradeAndEventButton.EventButton.GetComponentInChildren<TMP_Text>().text = "Replenish";
        
        public void EnableUpgradeAndEventButton() => UpgradeAndEventButton.Select();

        public void DisableUpgradeAndEvenButtons()
        {
            UpgradeAndEventButton.UpgradeButton.gameObject.SetActive(false);
            UpgradeAndEventButton.EventButton.gameObject.SetActive(false);
            UpgradeAndEventButton.gameObject.SetActive(false);
        }
    }
}