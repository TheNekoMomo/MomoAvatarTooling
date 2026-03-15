using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    public struct MenuGraphParamter
    {
        public string name;
        public VRCExpressionParameters.ValueType valueType;
        public bool saved;
        public bool networkSynced;
        public float defaultValue;
    }
}
