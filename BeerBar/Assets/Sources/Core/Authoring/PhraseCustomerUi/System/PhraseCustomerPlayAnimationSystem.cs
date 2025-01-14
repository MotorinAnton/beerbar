/*
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.PhraseCustomerUi.System
{
    [RequireMatchingQueriesForUpdate]
    public partial class PhraseCustomerPlayAnimationSystem : SystemBase
    {
        private EntityQuery _startAnimationPhraseUiManagerQuery;
        protected override void OnCreate()
        {
            using var startAnimationPhraseUiManagerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _startAnimationPhraseUiManagerQuery = startAnimationPhraseUiManagerBuilder
                .WithAll<PhraseCustomerUiManagerView, StartTweenPhraseCustomerUi>().WithNone<TweenProcessing>()
                .Build(this);
            
        }

        protected override void OnUpdate()
        {
            var startAnimationPhraseUi = _startAnimationPhraseUiManagerQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in startAnimationPhraseUi)
            {
                var phraseUiManager = EntityManager.GetComponentObject<PhraseCustomerUiManagerView>(entity).PhraseCustomerUiManager;

                if (phraseUiManager.FadeOutSequence.Count > 0)
                {
                    phraseUiManager.FadeOutSequence[0].Play();
                    phraseUiManager.FadeOutSequence.Remove(phraseUiManager.FadeOutSequence[0]);
                    EntityManager.AddComponent<TweenProcessing>(entity);
                    return;
                }
                if (phraseUiManager.MoveSequence.Count > 0)
                {
                    phraseUiManager.MoveSequence[0].Play();
                    phraseUiManager.MoveSequence.Remove(phraseUiManager.MoveSequence[0]);
                    EntityManager.AddComponent<TweenProcessing>(entity);
                    return;
                }
                if (phraseUiManager.FadeInSequence.Count > 0)
                {
                    phraseUiManager.FadeInSequence[0].Play();
                    phraseUiManager.FadeInSequence.Remove(phraseUiManager.FadeInSequence[0]);
                    EntityManager.AddComponent<TweenProcessing>(entity);
                    return;
                }
                
                EntityManager.RemoveComponent<TweenProcessing>(entity);
                EntityManager.RemoveComponent<StartTweenPhraseCustomerUi>(entity);
            }
        }
    }
}
*/

