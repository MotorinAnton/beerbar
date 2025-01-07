using System;
using System.Linq;
using Core.Authoring.Bartenders.AddBarmanFX;
using Core.Authoring.Containers;
using Core.Authoring.EventObjects;
using Core.Authoring.SelectGameObjects.Types;
using Core.Authoring.Tables;
using Core.Authoring.TVs;
using Core.Authoring.UpgradeAndEventButtonsUi;
using Core.Components.Wait;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.SelectGameObjects.Systems
{
    public partial class SelectSystem : SystemBase
    {
        private EntityQuery _selectMaterialQuery;

        protected override void OnCreate()
        {
            using var selectMaterialBuilder = new EntityQueryBuilder(Allocator.Temp);
            _selectMaterialQuery = selectMaterialBuilder.WithAll<SelectMaterial>()
                .Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SelectObject>()
                .ForEach((Entity entity) =>
                {
                    if (EntityManager.HasComponent<UpgradeAndEventButtonUi>(entity))
                    {
                        var buttons = EntityManager.GetComponentData<UpgradeAndEventButtonUi>(entity).Entity;
                        var upgradeAndEventButtonView =
                            EntityManager.GetComponentObject<UpgradeAndEvenButtonUiView>(buttons);
                        upgradeAndEventButtonView.EnableUpgradeAndEventButton();
                    }

                    if (EntityManager.HasComponent<ContainerView>(entity))
                    {
                        var containerView = EntityManager.GetComponentObject<ContainerView>(entity);
                        NewRendererArray(containerView.Value.Select, true);
                    }

                    if (EntityManager.HasComponent<TableView>(entity))
                    {
                        var tableView = EntityManager.GetComponentObject<TableView>(entity);
                        NewRendererArray(tableView.Value.Select, true);
                    }

                    if (EntityManager.HasComponent<TubeView>(entity))
                    {
                        var containerView = EntityManager.GetComponentObject<TubeView>(entity);
                        NewRendererArray(containerView.Value.Select, true);
                    }

                    if (EntityManager.HasComponent<ElectricityView>(entity))
                    {
                        var containerView = EntityManager.GetComponentObject<ElectricityView>(entity);
                        NewRendererArray(containerView.Value.Select, true);
                    }

                    if (EntityManager.HasComponent<TVView>(entity))
                    {
                        var tvView = EntityManager.GetComponentObject<TVView>(entity);
                        NewRendererArray(tvView.Value.Select, true);
                    }

                    if (EntityManager.HasComponent<LossWalletView>(entity))
                    {
                        var lossWalletView = EntityManager.GetComponentObject<LossWalletView>(entity);

                        NewRendererArray(lossWalletView.Value.Select, true);
                    }

                    if (EntityManager.HasComponent<BreakBottleView>(entity))
                    {
                        var breakBottleView = EntityManager.GetComponentObject<BreakBottleView>(entity);

                        NewRendererArray(breakBottleView.Value.Select, true);
                    }

                    if (EntityManager.HasComponent<AddBarmanFXView>(entity))
                    {
                        var addBarmanFXView = EntityManager.GetComponentObject<AddBarmanFXView>(entity);

                        NewRendererArray(addBarmanFXView.Value.Select, true);
                    }

                    EntityManager.RemoveComponent<SelectObject>(entity);

                }).WithStructuralChanges().Run();

            Entities.WithAll<DeselectObject>().WithNone<WaitTime>().ForEach(
                (Entity entity) =>
                {
                    if (EntityManager.HasComponent<UpgradeAndEventButtonUi>(entity))
                    {
                        var buttonsEntity = EntityManager.GetComponentData<UpgradeAndEventButtonUi>(entity).Entity;
                        var upgradeAndEventButtonView =
                            EntityManager.GetComponentObject<UpgradeAndEvenButtonUiView>(buttonsEntity);
                        upgradeAndEventButtonView.DisableUpgradeAndEvenButtons();
                    }

                    if (EntityManager.HasComponent<ContainerView>(entity))
                    {
                        var containerView = EntityManager.GetComponentObject<ContainerView>(entity);
                        NewRendererArray(containerView.Value.Select, false);
                    }

                    if (EntityManager.HasComponent<TableView>(entity))
                    {
                        var tableView = EntityManager.GetComponentObject<TableView>(entity);
                        NewRendererArray(tableView.Value.Select, false);
                    }

                    if (EntityManager.HasComponent<TVView>(entity))
                    {
                        var tvView = EntityManager.GetComponentObject<TVView>(entity);
                        NewRendererArray(tvView.Value.Select, false);
                    }


                    if (EntityManager.HasComponent<ElectricityView>(entity))
                    {
                        var containerView = EntityManager.GetComponentObject<ElectricityView>(entity);
                        NewRendererArray(containerView.Value.Select, false);
                    }

                    if (EntityManager.HasComponent<LossWalletView>(entity))
                    {
                        var lossWalletView = EntityManager.GetComponentObject<LossWalletView>(entity);

                        NewRendererArray(lossWalletView.Value.Select, false);
                    }

                    if (EntityManager.HasComponent<AddBarmanFXView>(entity))
                    {
                        var addBarmanFXView = EntityManager.GetComponentObject<AddBarmanFXView>(entity);

                        NewRendererArray(addBarmanFXView.Value.Select, false);
                    }

                    EntityManager.RemoveComponent<DeselectObject>(entity);

                }).WithStructuralChanges().Run();

            Entities.WithAll<DeselectObject>().ForEach(
                (Entity entity) =>
                {
                    if (EntityManager.HasComponent<BreakBottleView>(entity))
                    {
                        var breakBottleView = EntityManager.GetComponentObject<BreakBottleView>(entity);

                        NewRendererArray(breakBottleView.Value.Select, false);
                    }

                    if (EntityManager.HasComponent<TubeView>(entity))
                    {
                        var containerView = EntityManager.GetComponentObject<TubeView>(entity);
                        NewRendererArray(containerView.Value.Select, false);
                    }

                    EntityManager.RemoveComponent<DeselectObject>(entity);

                }).WithStructuralChanges().Run();

        }

        private void NewRendererArray(SelectObjectAuthoring selectAuthoring, bool select)
        {
            var selectMaterialEntity = _selectMaterialQuery.ToEntityArray(Allocator.Temp)[0];
            var particleSelectMaterial = EntityManager.GetComponentObject<SelectMaterial>(selectMaterialEntity);

            switch (selectAuthoring.SelectType)
            {
                case SelectObjectType.Renderer:

                    var selectObject = (RendererSelectAuthoring)selectAuthoring;

                    for (var index = 0; index < selectObject.Renderers.Length; index++)
                    {
                        var renderer = selectObject.Renderers[index];
                        renderer.sharedMaterials = SelectedMaterial(renderer.sharedMaterials, select);
                    }

                    break;

                case SelectObjectType.Skinned:

                    var skinnedSelectObject = (SkinnedSelectAuthoring)selectAuthoring;

                    for (var index = 0; index < skinnedSelectObject.Skinned.Length; index++)
                    {
                        var skinnedRenderer = skinnedSelectObject.Skinned[index];
                        skinnedRenderer.sharedMaterials = SelectedMaterial(skinnedRenderer.sharedMaterials, select);
                    }

                    break;

                case SelectObjectType.Particle:

                    var particleSelectObject = (ParticleRendererSelectAuthoring)selectAuthoring;
                    var particleRenderer = particleSelectObject.Particle;


                    if (select)
                    {
                        particleRenderer.sharedMaterial = particleSelectMaterial.ParticleBreakBottleRendererObject[0];
                    }

                    if (!select)
                    {
                        particleRenderer.sharedMaterial = particleSelectMaterial.ParticleBreakBottleRendererObject[1];
                    }


                    break;

                case SelectObjectType.RendererAndSkinned:

                    var rendererAndSkinnedSelectObject = (RendererAndSkinnedSelectAuthoring)selectAuthoring;

                    rendererAndSkinnedSelectObject.Skinned.sharedMaterials =
                        SelectedMaterial(rendererAndSkinnedSelectObject.Skinned.sharedMaterials, select);

                    for (var index = 0; index < rendererAndSkinnedSelectObject.Renderers.Length; index++)
                    {
                        var renderer = rendererAndSkinnedSelectObject.Renderers[index];
                        renderer.sharedMaterials = SelectedMaterial(renderer.sharedMaterials, select);
                    }

                    break;

                case SelectObjectType.RendererAndParticle:

                    var rendererAndParticleSelectObject = (RendererAndParticleSelectAuthoring)selectAuthoring;
                    var particle = rendererAndParticleSelectObject.Particle;

                    if (select)
                    {
                        particle.sharedMaterial = particleSelectMaterial.ParticleSprayRendererObject[0];
                    }
                    else
                    {
                        particle.sharedMaterial = particleSelectMaterial.ParticleSprayRendererObject[1];
                    }

                    for (var index = 0; index < rendererAndParticleSelectObject.Renderers.Length; index++)
                    {
                        var renderer = rendererAndParticleSelectObject.Renderers[index];
                        rendererAndParticleSelectObject.Renderers[index].sharedMaterials =
                            SelectedMaterial(renderer.sharedMaterials, select);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Material[] SelectedMaterial(Material[] array, bool select)
        {
            var selectMaterialEntity = _selectMaterialQuery.ToEntityArray(Allocator.Temp)[0];
            var rendererSelectMaterial =
                EntityManager.GetComponentObject<SelectMaterial>(selectMaterialEntity).RendererObject;
            var newRendererMaterialArray = array.ToHashSet();

            if (select)
            {
                newRendererMaterialArray.Add(rendererSelectMaterial);
                return newRendererMaterialArray.ToArray();
            }

            newRendererMaterialArray.Remove(rendererSelectMaterial);
            return newRendererMaterialArray.ToArray();
        }
    }
}