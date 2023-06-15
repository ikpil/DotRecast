using System.Collections.Immutable;
using System.Linq;

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

        public static PartitionType OfIdx(int idx)
        {
            return Values.FirstOrDefault(x => x.Idx == idx) ?? WATERSHED;
        }

        public override string ToString() => Name;
    }
}