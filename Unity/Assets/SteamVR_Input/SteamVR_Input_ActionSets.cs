//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Valve.VR
{
    using System;
    using UnityEngine;
    
    
    public partial class SteamVR_Actions
    {
        
        private static SteamVR_Input_ActionSet_default p__default;
        
        private static SteamVR_Input_ActionSet_inverted p_inverted;
        
        private static SteamVR_Input_ActionSet_tools p_tools;
        
        public static SteamVR_Input_ActionSet_default _default
        {
            get
            {
                return SteamVR_Actions.p__default.GetCopy<SteamVR_Input_ActionSet_default>();
            }
        }
        
        public static SteamVR_Input_ActionSet_inverted inverted
        {
            get
            {
                return SteamVR_Actions.p_inverted.GetCopy<SteamVR_Input_ActionSet_inverted>();
            }
        }
        
        public static SteamVR_Input_ActionSet_tools tools
        {
            get
            {
                return SteamVR_Actions.p_tools.GetCopy<SteamVR_Input_ActionSet_tools>();
            }
        }
        
        private static void StartPreInitActionSets()
        {
            SteamVR_Actions.p__default = ((SteamVR_Input_ActionSet_default)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_default>("/actions/default")));
            SteamVR_Actions.p_inverted = ((SteamVR_Input_ActionSet_inverted)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_inverted>("/actions/inverted")));
            SteamVR_Actions.p_tools = ((SteamVR_Input_ActionSet_tools)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_tools>("/actions/tools")));
            Valve.VR.SteamVR_Input.actionSets = new Valve.VR.SteamVR_ActionSet[] {
                    SteamVR_Actions._default,
                    SteamVR_Actions.inverted,
                    SteamVR_Actions.tools};
        }
    }
}
