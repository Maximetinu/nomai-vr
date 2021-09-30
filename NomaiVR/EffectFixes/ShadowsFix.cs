﻿
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace NomaiVR
{
    internal class ShadowsFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, ShadowsFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                //Solve (1x)PreCull->(2x)PostRender conflicts
                Prefix<CSMTextureCacher>(nameof(CSMTextureCacher.OnAnyCameraPostRender), nameof(Patch.PreOnAnyCameraPostRender));
                Prefix<RingworldShadowsOverride>(nameof(RingworldShadowsOverride.OnCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<CloudEffectBubbleController>(nameof(CloudEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<FogWarpEffectBubbleController>(nameof(FogWarpEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<QuantumFogEffectBubbleController>(nameof(QuantumFogEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<SandEffectBubbleController>(nameof(SandEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<UnderwaterEffectBubbleController>(nameof(UnderwaterEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //FIXME: These seem to break even more ?_?
                Prefix<PerCameraRendererState>(nameof(PerCameraRendererState.OnOWCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<GeyserWaterVillageHack>(nameof(GeyserWaterVillageHack.OnPlayerCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<LightLOD>(nameof(LightLOD.RevertLODSettings), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<RingworldSunController>(nameof(RingworldSunController.OnOWCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<IPExteriorVisualsManager>(nameof(IPExteriorVisualsManager.OnOWCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<TessellatedSphereRenderer>(nameof(TessellatedSphereRenderer.Clear), nameof(Patch.OWCameraPostRenderDisablerLeft));
                //Prefix<TessellatedPlaneRenderer>(nameof(TessellatedPlaneRenderer.Clear), nameof(Patch.OWCameraPostRenderDisablerLeft));
                //Prefix<TessellatedRingRenderer>(nameof(TessellatedRingRenderer.Clear), nameof(Patch.OWCameraPostRenderDisablerLeft));
                //Prefix<TessellatedSphereRenderer>(nameof(TessellatedSphereRenderer.Rebuild), nameof(Patch.OWCameraPostRenderDisablerLeft));
                //Prefix<TessellatedPlaneRenderer>(nameof(TessellatedPlaneRenderer.Rebuild), nameof(Patch.OWCameraPostRenderDisablerLeft));
                //Prefix<TessellatedRingRenderer>(nameof(TessellatedRingRenderer.Rebuild), nameof(Patch.OWCameraPostRenderDisablerLeft));

                //Ensure CommandBuffers are only built once
                Prefix<ProxyShadowLight>(nameof(ProxyShadowLight.BuildCommandBuffers), nameof(Patch.Pre_ProxyShadowLight_BuildCommandBuffers));
                Prefix<ProxyShadowLight>(nameof(ProxyShadowLight.ClearCommandBuffers), nameof(Patch.Pre_ProxyShadowLight_ClearCommandBuffers));

                Prefix<UnderwaterCurrentFadeController>(nameof(UnderwaterCurrentFadeController.OnAnyPrerender), nameof(Patch.Pre_UnderwaterCurrentFadeController_OnAnyPrerender));
                Prefix<UnderwaterCurrentFadeController>(nameof(UnderwaterCurrentFadeController.OnAnyPostrender), nameof(Patch.Pre_UnderwaterCurrentFadeController_OnAnyPostRender));
            }

            private static bool OWCameraPostRenderDisablerLeft(OWCamera owCamera)
            {
                return owCamera.mainCamera == null || !owCamera.mainCamera.stereoEnabled || owCamera.mainCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }

            private static bool OWCameraPostRenderDisabler(OWCamera owCamera)
            {
                return owCamera.mainCamera == null || !owCamera.mainCamera.stereoEnabled || owCamera.mainCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }

            private static bool PreOnAnyCameraPostRender(Camera camera)
            {
                return !camera.stereoEnabled || camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }

            //ProxyShadowLight updates for some reason have garbage data on the left eye, only update them on the right
            private static bool Pre_ProxyShadowLight_BuildCommandBuffers(Camera camera)
            {
                return !camera.stereoEnabled || camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }

            private static bool Pre_ProxyShadowLight_ClearCommandBuffers(Camera camera)
            {
                return !camera.stereoEnabled || camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }

            //ProxyShadowLight updates for some reason have garbage data on the left eye, only update them on the right
            private static bool Pre_UnderwaterCurrentFadeController_OnAnyPrerender(OWCamera owCamera)
            {
                return owCamera.mainCamera == null || !owCamera.mainCamera.stereoEnabled || owCamera.mainCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }

            private static bool Pre_UnderwaterCurrentFadeController_OnAnyPostRender(OWCamera owCamera)
            {
                return owCamera.mainCamera == null || !owCamera.mainCamera.stereoEnabled || owCamera.mainCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }
        }
    }
}
