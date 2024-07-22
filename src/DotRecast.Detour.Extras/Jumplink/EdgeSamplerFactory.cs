using System;
using System.Numerics;

namespace DotRecast.Detour.Extras.Jumplink
{
    public class EdgeSamplerFactory
    {
        public EdgeSampler Get(JumpLinkBuilderConfig acfg, JumpLinkType type, JumpEdge edge)
        {
            EdgeSampler es = null;
            switch (type.Bit)
            {
                case JumpLinkType.EDGE_JUMP_BIT:
                    es = InitEdgeJumpSampler(acfg, edge);
                    break;
                case JumpLinkType.EDGE_CLIMB_DOWN_BIT:
                    es = InitClimbDownSampler(acfg, edge);
                    break;
                case JumpLinkType.EDGE_JUMP_OVER_BIT:
                default:
                    throw new ArgumentException("Unsupported jump type " + type);
            }

            return es;
        }


        private EdgeSampler InitEdgeJumpSampler(JumpLinkBuilderConfig acfg, JumpEdge edge)
        {
            EdgeSampler es = new EdgeSampler(edge, new JumpTrajectory(acfg.jumpHeight));
            es.start.height = acfg.agentClimb * 2;
            Vector3 offset = new Vector3();
            Trans2d(ref offset, es.az, es.ay, new Vector2 { X = acfg.startDistance, Y = -acfg.agentClimb, });
            Vadd(ref es.start.p, edge.sp, offset);
            Vadd(ref es.start.q, edge.sq, offset);

            float dx = acfg.endDistance - 2 * acfg.agentRadius;
            float cs = acfg.cellSize;
            int nsamples = Math.Max(2, (int)MathF.Ceiling(dx / cs));

            for (int j = 0; j < nsamples; ++j)
            {
                float v = (float)j / (float)(nsamples - 1);
                float ox = 2 * acfg.agentRadius + dx * v;
                Trans2d(ref offset, es.az, es.ay, new Vector2 { X = ox, Y = acfg.minHeight });
                GroundSegment end = new GroundSegment();
                end.height = acfg.heightRange;
                Vadd(ref end.p, edge.sp, offset);
                Vadd(ref end.q, edge.sq, offset);
                es.end.Add(end);
            }

            return es;
        }

        private EdgeSampler InitClimbDownSampler(JumpLinkBuilderConfig acfg, JumpEdge edge)
        {
            EdgeSampler es = new EdgeSampler(edge, new ClimbTrajectory());
            es.start.height = acfg.agentClimb * 2;
            Vector3 offset = new Vector3();
            Trans2d(ref offset, es.az, es.ay, new Vector2() { X = acfg.startDistance, Y = -acfg.agentClimb });
            Vadd(ref es.start.p, edge.sp, offset);
            Vadd(ref es.start.q, edge.sq, offset);

            Trans2d(ref offset, es.az, es.ay, new Vector2() { X = acfg.endDistance, Y = acfg.minHeight });
            GroundSegment end = new GroundSegment();
            end.height = acfg.heightRange;
            Vadd(ref end.p, edge.sp, offset);
            Vadd(ref end.q, edge.sq, offset);
            es.end.Add(end);
            return es;
        }

        private void Vadd(float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[0] + v2[0];
            dest[1] = v1[1] + v2[1];
            dest[2] = v1[2] + v2[2];
        }
        
        private void Vadd(ref Vector3 dest, Vector3 v1, Vector3 v2)
        {
            dest.X = v1.X + v2.X;
            dest.Y = v1.Y + v2.Y;
            dest.Z = v1.Z + v2.Z;
        }


        private void Trans2d(float[] dst, float[] ax, float[] ay, float[] pt)
        {
            dst[0] = ax[0] * pt[0] + ay[0] * pt[1];
            dst[1] = ax[1] * pt[0] + ay[1] * pt[1];
            dst[2] = ax[2] * pt[0] + ay[2] * pt[1];
        }
        
        private void Trans2d(ref Vector3 dst, Vector3 ax, Vector3 ay, Vector2 pt)
        {
            dst.X = ax.X * pt.X + ay.X * pt.Y;
            dst.Y = ax.Y * pt.X + ay.Y * pt.Y;
            dst.Z = ax.Z * pt.X + ay.Z * pt.Y;
        }

    }
}
