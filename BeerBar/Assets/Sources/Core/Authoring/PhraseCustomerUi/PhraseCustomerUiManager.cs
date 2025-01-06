using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Unity.Entities;

namespace Core.Authoring.PhraseCustomerUi
{
    public class PhraseCustomerUiManager : EntityBehaviour
    {
        public PhraseCustomerUiAuthoring[] PhrasePanels;
        public PhraseCustomerUiAuthoring EventPanel;
        public List<Vector3> PositionList;
        public Vector3 EventPanelPosition;
        public List<Sequence> FadeInSequence;
        public List<Sequence> FadeOutSequence;
        public List<Sequence> MoveSequence;

        private void Awake()
        {
            PositionList = PhrasePanels.Select(panel => panel.RectTransform.position).ToList();
            FadeInSequence = new List<Sequence>();
            FadeOutSequence = new List<Sequence>();
            MoveSequence = new List<Sequence>();
            EventPanelPosition = EventPanel.RectTransform.localPosition;
        }

        public void StartEventPanelTween()
        {
            EventPanelFadeIn();
        }

        // public void CreateFadeInSequenceArray(List<PhraseCustomerUiAuthoring> panels)
        // {
        //     var sortedListPanel = panels.OrderBy(panel => panel.Index).ToList();
        //
        //     foreach (var panel in sortedListPanel)
        //     {
        //         Sequence.Add(PanelFadeIn(panel).Pause());
        //     }
        // }
        
        /*public void CreateMoveUpPanelSequenceArray( int startIndex)
        {
            var sortedPanels = PhrasePanels.OrderBy(panel => panel.Index).ToList();
            foreach (var panel in sortedPanels)
            {
                if (panel.Index == startIndex)
                {
                    panel.Index = 3;
                    Sequence.Add(PanelFadeOut(panel).Pause());
                }

                if (panel.Index > startIndex)
                {
                    panel.Index -= 1;
                    Sequence.Add(PanelMoveUp(panel).Pause());
                }
            }
        }*/

        /*public void PanelFadeIn(PhraseCustomerUiAuthoring panel)
        {
            panel.EnablePanel();
            
            panel.transform.DOKill();
            var targetPosition = PositionList[panel.Index];
            var startPosition = new Vector3();

            panel.CanvasGroup.alpha = 0;
            startPosition = panel.RectTransform.position;
            startPosition.y -= 300f;

            panel.RectTransform.position = startPosition;

            var newSequence = DOTween.Sequence();
            var tweenPosition = panel.RectTransform.DOAnchorPos3D(targetPosition, 0.9f).SetEase(Ease.OutQuint);
            var tweenFade = panel.CanvasGroup.DOFade(1, 0.5f);

            newSequence.Append(tweenPosition);
            newSequence.Join(tweenFade);
            newSequence.AppendCallback(TweenFinished);
            newSequence.Pause();
            Sequence.Add(newSequence);
        }*/
        
        public void PanelFadeIn(PhraseCustomerUiAuthoring panel, int index)
        {
            FadeInSequence.Add(panel.PanelFadeIn(PositionList[index]).AppendCallback(TweenFinished));
        }

        /*public void PanelFadeOut(PhraseCustomerUiAuthoring panel)
        {
            panel.transform.DOKill();
            var targetPosition = new Vector3();
            targetPosition = panel.RectTransform.localPosition;
            targetPosition.x += 20f;
            //panel.IsShow = false;

            var newSequence = DOTween.Sequence();
            var tweenPosition = panel.RectTransform.DOAnchorPos3D(targetPosition, 0.2f).SetEase(Ease.Linear);
            var tweenFade = panel.CanvasGroup.DOFade(0, 0.1f);

            newSequence.Append(tweenPosition);
            newSequence.Join(tweenFade);
            newSequence.AppendCallback(TweenFinished);
            //newSequence.AppendCallback(panel.DeactivatedPanel);
            newSequence.AppendCallback(panel.DisablePanel);
            newSequence.Pause();
            Sequence.Add(newSequence);
        }*/
        public void PanelFadeOut(PhraseCustomerUiAuthoring panel)
        {
            FadeOutSequence.Add(panel.PanelFadeOut().AppendCallback(TweenFinished));
        }
        
        /*public void PanelMoveUp(PhraseCustomerUiAuthoring panel, int index)
        {
            panel.transform.DOKill();
            
            var targetPosition = PositionList[index];
            var tween = panel.RectTransform.DOAnchorPos3D(targetPosition, 0.2f).SetEase(Ease.OutQuint);
            var tweenFade = panel.CanvasGroup.DOFade(1, 0.1f).SetEase(Ease.Flash);
            var newSequence = DOTween.Sequence();
            newSequence.Append(tween);
            newSequence.Join(tweenFade);
            newSequence.AppendCallback(() => SetIndex(panel, index)).OnComplete(TweenFinished);
            newSequence.Pause();
            Sequence.Add(newSequence);
        }*/
        
        public void PanelMoveUp(PhraseCustomerUiAuthoring panel, int index)
        {
            MoveSequence.Add(panel.PanelMoveUp(PositionList[index], index).AppendCallback(TweenFinished));
        }
        
        
        /*public void SwapPanel( PhraseCustomerUiAuthoring panel1 , PhraseCustomerUiAuthoring panel2)
        {
            panel1.transform.DOKill();
            panel2.transform.DOKill();
           
            var indexPanel1 = panel1.Index;
            var indexPanel2 = panel2.Index;
            
            var tween = panel1.RectTransform.DOAnchorPos3D(PositionList[indexPanel2], 0.2f).SetEase(Ease.OutQuint);
            var tweenFade = panel1.CanvasGroup.DOFade(1, 0.1f).SetEase(Ease.Flash);
            var tween2 = panel2.RectTransform.DOAnchorPos3D(PositionList[indexPanel1], 0.2f).SetEase(Ease.OutQuint);
            var tweenFade2 = panel2.CanvasGroup.DOFade(1, 0.1f).SetEase(Ease.Flash);
            var newSequence = DOTween.Sequence();
            newSequence.Append(tween);
            newSequence.Join(tweenFade);
            newSequence.Append(tween2);
            newSequence.Join(tweenFade2);
            //newSequence.AppendCallback(TweenFinished).OnComplete(() => SetIndexes(panel1, panel2, indexPanel1, indexPanel2));
            Sequence.Add(newSequence);
        }*/
        private void SetIndex(PhraseCustomerUiAuthoring panel, int indexPanel)
        {
            panel.Index = indexPanel;
        }

        private void EventPanelFadeIn()
        {
            EventPanel.EnablePanel();
            EventPanel.CanvasGroup.alpha = 0;
            var startPosition = new Vector3();
            startPosition = EventPanelPosition;
            startPosition.x += 30f;
            EventPanel.RectTransform.localPosition = startPosition;
            var newSequence = DOTween.Sequence();
            var tweenPosition = EventPanel.RectTransform.DOAnchorPos3D(EventPanelPosition, 0.7f).SetEase(Ease.OutQuint);
            var tweenFade = EventPanel.CanvasGroup.DOFade(1, 0.4f);
            newSequence.Append(tweenPosition);
            newSequence.Join(tweenFade);
            newSequence.AppendInterval(1f);
            var tweenEndPosition = EventPanel.RectTransform.DOAnchorPos3D(startPosition, 0.2f).SetEase(Ease.OutQuint)
                .OnComplete(EventPanel.DisablePanel);
            var tweenEndFade = EventPanel.CanvasGroup.DOFade(0, 0.2f);

            newSequence.Append(tweenEndPosition);
            newSequence.Join(tweenEndFade);
            newSequence.AppendCallback(TweenFinished);
        }
        
        
        
        private void TweenFinished()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            manager.RemoveComponent<TweenProcessing>(Entity);
        }
        
    }
}