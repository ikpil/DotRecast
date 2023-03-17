using System.Collections.Immutable;

namespace DotRecast.Recast
{
    /// < Tessellate edges between areas during contour
    /// simplification.
    public class PartitionType
    {
        public static readonly PartitionType WATERSHED = new PartitionType(0, nameof(WATERSHED));
        public static readonly PartitionType MONOTONE = new PartitionType(1, nameof(MONOTONE));
        public static readonly PartitionType LAYERS = new PartitionType(2, nameof(LAYERS));

        public static readonly ImmutableArray<PartitionType> Values = ImmutableArray.Create(WATERSHED, MONOTONE, LAYERS);

        public int Idx { get; }
        public string Name { get; }

        private PartitionType(int idx, string name)
        {
            Idx = idx;
            Name = name;
        }

        public override string ToString() => Name;
    }
}