using System;

namespace DotRecast.Detour.Extras.Jumplink;

class EdgeSamplerFactory {

    public EdgeSampler get(JumpLinkBuilderConfig acfg, JumpLinkType type, Edge edge) {
        EdgeSampler es = null;
        switch (type) {
        case JumpLinkType.EDGE_JUMP:
            es = initEdgeJumpSampler(acfg, edge);
            break;
        case JumpLinkType.EDGE_CLIMB_DOWN:
            es = initClimbDownSampler(acfg, edge);
            break;
        case JumpLinkType.EDGE_JUMP_OVER:
        default:
            throw new ArgumentException("Unsupported jump type " + type);
        }
        return es;
    }


    private EdgeSampler initEdgeJumpSampler(JumpLinkBuilderConfig acfg, Edge edge) {

        EdgeSampler es = new EdgeSampler(edge, new JumpTrajectory(acfg.jumpHeight));
        es.start.height = acfg.agentClimb * 2;
        float[] offset = new float[3];
        trans2d(offset, es.az, es.ay, new float[] { acfg.startDistance, -acfg.agentClimb });
        vadd(es.start.p, edge.sp, offset);
        vadd(es.start.q, edge.sq, offset);

        float dx = acfg.endDistance - 2 * acfg.agentRadius;
        float cs = acfg.cellSize;
        int nsamples = Math.Max(2, (int) Math.Ceiling(dx / cs));

        for (int j = 0; j < nsamples; ++j) {
            float v = (float) j / (float) (nsamples - 1);
            float ox = 2 * acfg.agentRadius + dx * v;
            trans2d(offset, es.az, es.ay, new float[] { ox, acfg.minHeight });
            GroundSegment end = new GroundSegment();
            end.height = acfg.heightRange;
            vadd(end.p, edge.sp, offset);
            vadd(end.q, edge.sq, offset);
            es.end.Add(end);
        }
        return es;
    }

    private EdgeSampler initClimbDownSampler(JumpLinkBuilderConfig acfg, Edge edge) {
        EdgeSampler es = new EdgeSampler(edge, new ClimbTrajectory());
        es.start.height = acfg.agentClimb * 2;
        float[] offset = new float[3];
        trans2d(offset, es.az, es.ay, new float[] { acfg.startDistance, -acfg.agentClimb });
        vadd(es.start.p, edge.sp, offset);
        vadd(es.start.q, edge.sq, offset);

        trans2d(offset, es.az, es.ay, new float[] { acfg.endDistance, acfg.minHeight });
        GroundSegment end = new GroundSegment();
        end.height = acfg.heightRange;
        vadd(end.p, edge.sp, offset);
        vadd(end.q, edge.sq, offset);
        es.end.Add(end);
        return es;
    }

    private void vadd(float[] dest, float[] v1, float[] v2) {
        dest[0] = v1[0] + v2[0];
        dest[1] = v1[1] + v2[1];
        dest[2] = v1[2] + v2[2];
    }

    private void trans2d(float[] dst, float[] ax, float[] ay, float[] pt) {
        dst[0] = ax[0] * pt[0] + ay[0] * pt[1];
        dst[1] = ax[1] * pt[0] + ay[1] * pt[1];
        dst[2] = ax[2] * pt[0] + ay[2] * pt[1];
    }

}
