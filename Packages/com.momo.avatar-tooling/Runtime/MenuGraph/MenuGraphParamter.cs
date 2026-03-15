using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    [Serializable]
    public class MenuGraphParamter
    {
        public string name = "New Paramter";
        public VRCExpressionParameters.ValueType valueType = VRCExpressionParameters.ValueType.Int;
        public bool saved = false;
        public bool networkSynced = false;
        public float defaultValue = 0;

        public static MenuGraphParamter ConvertToMenuGraphParamters(VRCExpressionParameters.Parameter VRCParameter)
        {
            MenuGraphParamter menuGraphParamter = new MenuGraphParamter();

            menuGraphParamter.name = VRCParameter.name;
            menuGraphParamter.valueType = VRCParameter.valueType;
            menuGraphParamter.saved = VRCParameter.saved;
            menuGraphParamter.defaultValue = VRCParameter.defaultValue;
            menuGraphParamter.networkSynced = VRCParameter.networkSynced;

            return menuGraphParamter;
        }
    }
}
