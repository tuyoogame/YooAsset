#if UNITY_2019_4_OR_NEWER
using UnityEditor.Experimental.GraphView;

namespace YooAsset.Editor
{
    public class GraphPort
    {
        public Port Port { get; set; }

        public GraphPort(string portName, Port port)
        {
            Port = port;
            Port.portName = portName;
        }

        public GraphPort(string portName, Orientation orientation, Direction direction, Port.Capacity capacity,
            System.Type type)
        {
            Port = Port.Create<Edge>(orientation, direction, capacity, type);
            Port.portName = portName;
        }
    }
}

#endif