using SQLite;
using System;

namespace org.neurul.Cortex.Domain.Model.Users
{
    public class LayerPermit
    {
        [PrimaryKey, AutoIncrement]
        public long SequenceId { get; set; }

        public Guid UserNeuronId { get; set; }

        public Guid LayerNeuronId { get; set; }

        public int WriteLevel { get; set; }

        public bool CanRead { get; set; }

        public bool Equals(LayerPermit other)
        {
            if (object.ReferenceEquals(this, other)) return true;
            if (object.ReferenceEquals(null, other)) return false;
            return this.SequenceId.Equals(other.SequenceId);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LayerPermit);
        }

        public override int GetHashCode()
        {
            return this.SequenceId.GetHashCode();
        }
    }
}
