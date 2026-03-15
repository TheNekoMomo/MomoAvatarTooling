using System;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    [Serializable]
    public class MenuGraphParamter
    {
        public string name;
        public VRCExpressionParameters.ValueType valueType;
        public bool saved;
        public bool networkSynced;
        public float defaultValue;
    }
}
