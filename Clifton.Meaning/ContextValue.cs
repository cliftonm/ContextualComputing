using System;
using System.Collections.Generic;
using System.Linq;

namespace Clifton.Meaning
{
    public class ContextValue
    {
        public Guid InstanceId { get; protected set; }
        public string Value { get; protected set; }
        public int RecordNumber { get; set; }       // Public setter should be used internally.
        public Type Type { get { return typePath.Last(); } }

        public IReadOnlyList<Guid> InstancePath { get { return instancePath; } }
        public IReadOnlyList<Type> TypePath { get { return typePath; } }

        protected List<Guid> instancePath = new List<Guid>();
        protected List<Type> typePath = new List<Type>();

        public ContextValue(string value, List<Guid> instancePath, List<Type> typePath, int recordNumber = 0)
        {
            InstanceId = instancePath.Last();
            Value = value;
            RecordNumber = recordNumber;
            this.instancePath = instancePath;
            this.typePath = typePath;
        }
    }
}
