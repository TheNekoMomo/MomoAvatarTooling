namespace MomoVRChatTools
{
    [System.Serializable]
    public struct MenuGraphConnection
    {
        public MenuGraphConnectionPort inputPort;
        public MenuGraphConnectionPort outputPort;

        public MenuGraphConnection(MenuGraphConnectionPort inputPort, MenuGraphConnectionPort outputPort)
        {
            this.inputPort = inputPort;
            this.outputPort = outputPort;
        }

        public MenuGraphConnection(string inputPortGUID, int inputPortIndex, string outputPortGUID, int outputPortIndex)
        {
            inputPort = new MenuGraphConnectionPort(inputPortGUID, inputPortIndex);
            outputPort = new MenuGraphConnectionPort(outputPortGUID, outputPortIndex);
        }
    }
    [System.Serializable]
    public struct MenuGraphConnectionPort
    {
        public string nodeGUID;
        public int portIndex;

        public MenuGraphConnectionPort(string nodeGUID, int portIndex)
        {
            this.nodeGUID = nodeGUID;
            this.portIndex = portIndex;
        }
    }
}
