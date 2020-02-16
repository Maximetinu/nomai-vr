﻿using OWML.Common;
using UnityEngine;

namespace NomaiVR {
    public class HoldSignalscope: MonoBehaviour {
        protected static Transform SignalscopeReticule;
        protected static Transform ShipWindshield;
        protected static Signalscope SignalScope;

        void Awake () {
            ShipWindshield = GameObject.Find("ShipLODTrigger_Cockpit").transform;


            var signalScope = Camera.main.transform.Find("Signalscope");
            Hands.HoldObject(signalScope, Hands.RightHand, new Vector3(-0.047f, 0.053f, 0.143f), Quaternion.Euler(32.8f, 0, 0));
            SignalScope = signalScope.GetComponent<Signalscope>();

            var signalScopeModel = signalScope.GetChild(0);
            // Tools have a special shader that draws them on top of everything
            // and screws with perspective. Changing to Standard shader so they look
            // like a normal 3D object.
            signalScopeModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            signalScopeModel.localPosition = Vector3.up * -0.1f;
            signalScopeModel.localRotation = Quaternion.identity;

            // This child seems to be only for some kind of shader effect.
            // Disabling it since it looks glitchy and doesn't seem necessary.
            signalScopeModel.GetChild(0).gameObject.SetActive(false);

            var signalScopeHolster = Instantiate(signalScopeModel).gameObject;
            signalScopeHolster.SetActive(true);
            var holster = signalScopeHolster.AddComponent<HolsterTool>();
            holster.hand = Hands.RightHand;
            holster.position = new Vector3(0.3f, -0.55f, 0);
            holster.mode = ToolMode.SignalScope;
            holster.scale = 0.8f;
            holster.angle = Vector3.right * 90;

            var playerHUD = GameObject.Find("PlayerHUD").transform;
            SignalscopeReticule = playerHUD.Find("HelmetOffUI/SignalscopeReticule");
            var helmetOn = playerHUD.Find("HelmetOnUI/UICanvas/SigScopeDisplay");
            var helmetOff = playerHUD.Find("HelmetOffUI/SignalscopeCanvas");

            // Attatch Signalscope UI to the Signalscope.
            SignalscopeReticule.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            SignalscopeReticule.parent = signalScope;
            SignalscopeReticule.localScale = Vector3.one * 0.0005f;
            SignalscopeReticule.localPosition = Vector3.forward * 0.5f;
            SignalscopeReticule.localRotation = Quaternion.identity;

            helmetOff.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            helmetOff.parent = signalScope;
            helmetOff.localScale = Vector3.one * 0.0005f;
            helmetOff.localPosition = Vector3.zero;
            helmetOff.localRotation = Quaternion.identity;

            helmetOn.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            helmetOn.parent = signalScope;
            helmetOn.localScale = Vector3.one * 0.0005f;
            helmetOn.localPosition = Vector3.down * 0.5f;
            helmetOn.localRotation = Quaternion.identity;
        }

        private void OnEvent (MonoBehaviour behaviour, Events ev) {

        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<OWInput>("ChangeInputMode", typeof(Patches), nameof(Patches.ChangeInputMode));
                NomaiVR.Post<ShipCockpitUI>("Start", typeof(Patches), nameof(Patches.ShipCockpitStart));

                // For fixing signalscope zoom
                //NomaiVR.Post<Signalscope>("EnterSignalscopeZoom", typeof(Patches), nameof(Patches.ZoomIn));
                //NomaiVR.Post<Signalscope>("ExitSignalscopeZoom", typeof(Patches), nameof(Patches.ZoomOut));
            }

            static void ShipCockpitStart (Signalscope ____signalscopeTool) {
                ____signalscopeTool = SignalScope;
            }

            static void ZoomIn () {
                Camera.main.transform.localScale = Vector3.one * 0.1f;
            }

            static void ZoomOut () {
                Camera.main.transform.localScale = Vector3.one;
            }

            static void ChangeInputMode (InputMode mode) {
                if (!SignalscopeReticule) {
                    return;
                }
                if (mode == InputMode.ShipCockpit || mode == InputMode.LandingCam) {
                    SignalscopeReticule.parent = ShipWindshield;
                    SignalscopeReticule.localScale = Vector3.one * 0.004f;
                    SignalscopeReticule.localPosition = Vector3.forward * 3f;
                    SignalscopeReticule.localRotation = Quaternion.identity;
                } else {
                    SignalscopeReticule.parent = SignalScope.transform;
                    SignalscopeReticule.localScale = Vector3.one * 0.0005f;
                    SignalscopeReticule.localPosition = Vector3.forward * 0.5f;
                    SignalscopeReticule.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
