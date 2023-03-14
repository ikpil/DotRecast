namespace DotRecast.Recast;

/// < Tessellate edges between areas during contour
/// simplification.
public enum PartitionType
{
    WATERSHED,
    MONOTONE,
    LAYERS
}