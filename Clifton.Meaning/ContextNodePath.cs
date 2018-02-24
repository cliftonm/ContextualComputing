using System;
using System.Collections.Generic;

namespace Clifton.Meaning
{
    public class NodeTypeInstance
    {
        public Type Type { get; set; }
        public Guid InstanceId { get; set; }
    }

    public class ContextNodePath
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public List<NodeTypeInstance> Path { get; set; }

        public ContextNodePath()
        {
            Path = new List<NodeTypeInstance>();
        }
    }
}
