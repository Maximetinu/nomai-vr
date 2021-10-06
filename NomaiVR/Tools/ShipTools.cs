﻿
using NomaiVR.ReusableBehaviours;
using UnityEngine;
using UnityEngine.UI;
using static NomaiVR.Tools.AutopilotButtonPatch;

namespace NomaiVR
{
    internal class ShipTools : NomaiVRModule<ShipTools.Behaviour, ShipTools.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            private ReferenceFrameTracker _referenceFrameTracker;
            private static Transform _mapGridRenderer;
            private static ShipMonitorInteraction _probe;
            private static ShipMonitorInteraction _signalscope;
            private static ShipMonitorInteraction _landingCam;
            private static ShipMonitorInteraction _autoPilot;
            private static ShipCockpitController _cockpitController;
            private static bool _isLandingCamEnabled;

            internal void Awake()
            {
                _referenceFrameTracker = FindObjectOfType<ReferenceFrameTracker>();
                _cockpitController = FindObjectOfType<ShipCockpitController>();
                _mapGridRenderer = FindObjectOfType<MapController>()._gridRenderer.transform;
            }

            internal void Update()
            {
                if (_referenceFrameTracker.isActiveAndEnabled && ToolHelper.IsUsingAnyTool())
                {
                    _referenceFrameTracker.enabled = false;
                }
                else if (!_referenceFrameTracker.isActiveAndEnabled && !ToolHelper.IsUsingAnyTool())
                {
                    _referenceFrameTracker.enabled = true;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ShipBody>("Start", nameof(ShipStart));
                    Prefix<ReferenceFrameTracker>(nameof(ReferenceFrameTracker.FindReferenceFrameInLineOfSight), nameof(PreFindFrame));
                    Postfix<ReferenceFrameTracker>(nameof(ReferenceFrameTracker.FindReferenceFrameInLineOfSight), nameof(PostFindFrame));
                    Prefix<ReferenceFrameTracker>(nameof(ReferenceFrameTracker.FindReferenceFrameInMapView), nameof(PreFindFrame));
                    Postfix<ReferenceFrameTracker>(nameof(ReferenceFrameTracker.FindReferenceFrameInMapView), nameof(PostFindFrame));
                    Empty<PlayerCameraController>("OnEnterLandingView");
                    Empty<PlayerCameraController>("OnExitLandingView");
                    Empty<PlayerCameraController>("OnEnterShipComputer");
                    Empty<PlayerCameraController>("OnExitShipComputer");
                    Prefix<ShipCockpitController>("EnterLandingView", nameof(PreEnterLandingView));
                    Prefix<ShipCockpitController>("ExitLandingView", nameof(PreExitLandingView));
                    Postfix<ShipCockpitController>("ExitFlightConsole", nameof(PostExitFlightConsole));
                    Prefix<ShipCockpitUI>("Update", nameof(PreCockpitUIUpdate));
                    Postfix<ShipCockpitUI>("Update", nameof(PostCockpitUIUpdate));
                    Prefix(typeof(ReferenceFrameTracker).GetMethod("UntargetReferenceFrame", new[] { typeof(bool) }), nameof(PreUntargetFrame));
                }

                private static void PreCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr._usingLandingCam = _isLandingCamEnabled;
                }

                private static void PostCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr._usingLandingCam = false;
                }

                private static bool PreEnterLandingView(
                    LandingCamera ____landingCam,
                    ShipLight ____landingLight,
                    ShipCameraComponent ____landingCamComponent,
                    ShipAudioController ____shipAudioController
                )
                {
                    _isLandingCamEnabled = true;
                    ____landingCam.enabled = true;
                    ____landingLight.SetOn(true);

                    if (____landingCamComponent.isDamaged)
                    {
                        ____shipAudioController.PlayLandingCamOn(AudioType.ShipCockpitLandingCamStatic_LP);
                    }
                    else
                    {
                        ____shipAudioController.PlayLandingCamOn(AudioType.ShipCockpitLandingCamAmbient_LP);
                    }

                    return false;
                }

                private static bool PreExitLandingView(
                    LandingCamera ____landingCam,
                    ShipLight ____landingLight,
                    ShipAudioController ____shipAudioController
                )
                {
                    _isLandingCamEnabled = false;
                    ____landingCam.enabled = false;
                    ____landingLight.SetOn(false);
                    ____shipAudioController.PlayLandingCamOff();

                    return false;
                }

                private static void PostExitFlightConsole(ShipCockpitController __instance)
                {
                    __instance.ExitLandingView();
                }

                private static bool ShouldRenderScreenText()
                {
                    return Locator.GetToolModeSwapper().IsInToolMode(ToolMode.None);
                }

                private static void ShipStart(ShipBody __instance)
                {
                    var cockpitUI = __instance.transform.Find("Module_Cockpit/Systems_Cockpit/ShipCockpitUI");

                    var probeScreenPivot = cockpitUI.Find("ProbeScreen/ProbeScreenPivot");
                    _probe = probeScreenPivot.Find("ProbeScreen").gameObject.AddComponent<ShipMonitorInteraction>();
                    _probe.mode = ToolMode.Probe;
                    _probe.text = UITextType.ScoutModePrompt;

                    var font = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular");

                    var probeCamDisplay = probeScreenPivot.Find("ProbeCamDisplay");
                    var probeScreenText = new GameObject().AddComponent<Text>();
                    probeScreenText.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = ShouldRenderScreenText;
                    probeScreenText.transform.SetParent(probeCamDisplay.transform, false);
                    probeScreenText.transform.localScale = Vector3.one * 0.0035f;
                    probeScreenText.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    probeScreenText.text = "<color=grey>PROBE LAUNCHER</color>\n\ninteract with screen\nto activate";
                    probeScreenText.color = new Color(1, 1, 1, 0.1f);
                    probeScreenText.alignment = TextAnchor.MiddleCenter;
                    probeScreenText.fontSize = 8;
                    probeScreenText.font = font;

                    var signalScreenPivot = cockpitUI.Find("SignalScreen/SignalScreenPivot");
                    _signalscope = signalScreenPivot.Find("SignalScopeScreenFrame_geo").gameObject.AddComponent<ShipMonitorInteraction>();
                    _signalscope.mode = ToolMode.SignalScope;
                    _signalscope.text = UITextType.UISignalscope;

                    var sigScopeDisplay = signalScreenPivot.Find("SigScopeDisplay");
                    var scopeTextCanvas = new GameObject().AddComponent<Canvas>();
                    scopeTextCanvas.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = ShouldRenderScreenText;
                    scopeTextCanvas.transform.SetParent(sigScopeDisplay.transform.parent, false);
                    scopeTextCanvas.transform.localPosition = sigScopeDisplay.transform.localPosition;
                    scopeTextCanvas.transform.localRotation = sigScopeDisplay.transform.localRotation;
                    scopeTextCanvas.transform.localScale = sigScopeDisplay.transform.localScale;
                    var scopeScreenText = new GameObject().AddComponent<Text>();
                    scopeScreenText.transform.SetParent(scopeTextCanvas.transform, false);
                    scopeScreenText.transform.localScale = Vector3.one * 0.5f;
                    scopeScreenText.text = "<color=grey>SIGNALSCOPE</color>\n\ninteract with screen to activate";
                    scopeScreenText.color = new Color(1, 1, 1, 0.1f);
                    scopeScreenText.alignment = TextAnchor.MiddleCenter;
                    scopeScreenText.fontSize = 8;
                    scopeScreenText.font = font;

                    var cockpitTech = __instance.transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");

                    _landingCam = cockpitTech.Find("LandingCamScreen").gameObject.AddComponent<ShipMonitorInteraction>();
                    _landingCam.button = InputConsts.InputCommandType.LANDING_CAMERA;
                    _landingCam.skipPressCallback = () =>
                    {
                        if (_isLandingCamEnabled)
                        {
                            _cockpitController.ExitLandingView();
                            return true;
                        }
                        return false;
                    };
                    _landingCam.text = UITextType.ShipLandingPrompt;

                    var landingTextCanvas = new GameObject().AddComponent<Canvas>();
                    landingTextCanvas.transform.SetParent(_landingCam.transform.parent, false);
                    landingTextCanvas.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = () => ShouldRenderScreenText() && !_isLandingCamEnabled;
                    landingTextCanvas.transform.localPosition = new Vector3(-0.017f, 3.731f, 5.219f);
                    landingTextCanvas.transform.localRotation = Quaternion.Euler(53.28f, 0, 0);
                    landingTextCanvas.transform.localScale = Vector3.one * 0.007f;
                    var landingText = new GameObject().AddComponent<Text>();
                    landingText.transform.SetParent(landingTextCanvas.transform, false);
                    landingText.transform.localScale = Vector3.one * 0.6f;
                    landingText.text = "<color=grey>LANDING CAMERA</color>\n\ninteract with screen\nto activate";
                    landingText.color = new Color(1, 1, 1, 0.1f);
                    landingText.alignment = TextAnchor.MiddleCenter;
                    landingText.fontSize = 8;
                    landingText.font = font;

                    _autoPilot = cockpitTech.GetComponentInChildren<AutopilotButton>().GetComponent<ShipMonitorInteraction>();
                }

                private static Vector3 _cameraPosition;
                private static Quaternion _cameraRotation;

                private static void PreFindFrame(ReferenceFrameTracker __instance)
                {
                    if (__instance._isLandingView)
                    {
                        return;
                    }

                    var activeCam = __instance._activeCam.transform;
                    _cameraPosition = activeCam.position;
                    _cameraRotation = activeCam.rotation;

                    if (__instance._isMapView)
                    {
                        activeCam.position = _mapGridRenderer.position + _mapGridRenderer.up * 10000;
                        activeCam.rotation = Quaternion.LookRotation(_mapGridRenderer.up * -1);
                    }
                    else
                    {
                        activeCam.position = LaserPointer.Behaviour.Laser.position;
                        activeCam.rotation = LaserPointer.Behaviour.Laser.rotation;
                    }
                }

                private static bool IsAnyInteractionFocused()
                {
                    return (_probe != null && _probe.IsFocused()) || 
                           (_signalscope != null && _signalscope.IsFocused()) || 
                           (_landingCam != null && _landingCam.IsFocused()) ||
                           (_autoPilot != null && _autoPilot.IsFocused());
                }

                private static bool PreUntargetFrame()
                {
                    return !IsAnyInteractionFocused();
                }

                private static ReferenceFrame PostFindFrame(ReferenceFrame __result, ReferenceFrameTracker __instance)
                {
                    if (__instance._isLandingView) return __result;

                    var activeCam = __instance._activeCam.transform;
                    activeCam.position = _cameraPosition;
                    activeCam.rotation = _cameraRotation;

                    return IsAnyInteractionFocused() ? __instance._currentReferenceFrame : __result;
                }
            }
        }
    }
}
