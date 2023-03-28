using System;
using DotRecast.Core;

namespace DotRecast.Detour.Extras.Jumplink
{
    class EdgeSamplerFactory
    {
        public EdgeSampler get(JumpLinkBuilderConfig acfg, JumpLinkType type, Edge edge)
        {
            EdgeSampler es = null;
            switch (type.Bit)
            {
                case JumpLinkType.EDGE_JUMP_BIT:
                    es = initEdgeJumpSampler(acfg, edge);
                    break;
                case JumpLinkType.EDGE_CLIMB_DOWN_BIT:
                    es = initClimbDownSampler(acfg, edge);
                    break;
                case JumpLinkType.EDGE_JUMP_OVER_BIT:
                default:
                    throw new ArgumentException("Unsupported jump type " + type);
            }

            return es;
        }


        private EdgeSampler initEdgeJumpSampler(JumpLinkBuilderConfig acfg, Edge edge)
        {
            EdgeSampler es = new EdgeSampler(edge, new JumpTrajectory(acfg.jumpHeight));
            es.start.height = acfg.agentClimb * 2;
            Vector3f offset = new Vector3f();
            trans2d(ref offset, es.az, es.ay, new Vector2f { x = acfg.startDistance, y = -acfg.agentClimb, });
            vadd(ref es.start.p, edge.sp, offset);
            vadd(ref es.start.q, edge.sq, offset);

            float dx = acfg.endDistance - 2 * acfg.agentRadius;
            float cs = acfg.cellSize;
            int nsamples = Math.Max(2, (int)Math.Ceiling(dx / cs));

            for (int j = 0; j < nsamples; ++j)
            {
                float v = (float)j / (float)(nsamples - 1);
                float ox = 2 * acfg.agentRadius + dx * v;
                trans2d(ref offset, es.az, es.ay, new Vector2f { x = ox, y = acfg.minHeight });
                GroundSegment end = new GroundSegment();
                end.height = acfg.heightRange;
                vadd(ref end.p, edge.sp, offset);
                vadd(ref end.q, edge.sq, offset);
                es.end.Add(end);
            }

            return es;
        }

        private EdgeSampler initClimbDownSampler(JumpLinkBuilderConfig acfg, Edge edge)
        {
            EdgeSampler es = new EdgeSampler(edge, new ClimbTrajectory());
            es.start.height = acfg.agentClimb * 2;
            Vector3f offset = new Vector3f();
            trans2d(ref offset, es.az, es.ay, new Vector2f() { x = acfg.startDistance, y = -acfg.agentClimb });
            vadd(ref es.start.p, edge.sp, offset);
            vadd(ref es.start.q, edge.sq, offset);

            trans2d(ref offset, es.az, es.ay, new Vector2f() { x = acfg.endDistance, y = acfg.minHeight });
            GroundSegment end = new GroundSegment();
            end.height = acfg.heightRange;
            vadd(ref end.p, edge.sp, offset);
            vadd(ref end.q, edge.sq, offset);
            es.end.Add(end);
            return es;
        }

        private void vadd(float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[0] + v2[0];
            dest[1] = v1[1] + v2[1];
            dest[2] = v1[2] + v2[2];
        }
        
        private void vadd(ref Vector3f dest, Vector3f v1, Vector3f v2)
        {
            dest[0] = v1[0] + v2[0];
            dest[1] = v1[1] + v2[1];
            dest[2] = v1[2] + v2[2];
        }


        private void trans2d(float[] dst, float[] ax, float[] ay, float[] pt)
        {
            dst[0] = ax[0] * pt[0] + ay[0] * pt[1];
            dst[1] = ax[1] * pt[0] + ay[1] * pt[1];
            dst[2] = ax[2] * pt[0] + ay[2] * pt[1];
        }
        
        private void trans2d(ref Vector3f dst, Vector3f ax, Vector3f ay, Vector2f pt)
        {
            dst[0] = ax[0] * pt[0] + ay[0] * pt[1];
            dst[1] = ax[1] * pt[0] + ay[1] * pt[1];
            dst[2] = ax[2] * pt[0] + ay[2] * pt[1];
        }

    }
}