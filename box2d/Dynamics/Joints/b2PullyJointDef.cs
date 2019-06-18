using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2D.Common;

namespace Box2D.Dynamics.Joints
{
    /// Pulley joint definition. This requires two ground anchors,
    /// two dynamic body anchor points, and a pulley ratio.
    public class b2PulleyJointDef : b2JointDef
    {
        public b2PulleyJointDef()
        {
            JointType = b2JointType.e_pulleyJoint;
            groundAnchorA.Set(-1.0f, 1.0f);
            groundAnchorB.Set(1.0f, 1.0f);
            localAnchorA.Set(-1.0f, 0.0f);
            localAnchorB.Set(1.0f, 0.0f);
            lengthA = 0.0f;
            lengthB = 0.0f;
            ratio = 1.0f;
            CollideConnected = true;
        }


        /// Initialize the bodies, anchors, lengths, max lengths, and ratio using the world anchors.
public void Initialize(b2Body bA, b2Body bB,
                b2Vec2 groundA, b2Vec2 groundB,
                b2Vec2 anchorA, b2Vec2 anchorB,
                float r)
{
    BodyA = bA;
    BodyB = bB;
    groundAnchorA = groundA;
    groundAnchorB = groundB;
    localAnchorA = BodyA.GetLocalPoint(anchorA);
    localAnchorB = BodyB.GetLocalPoint(anchorB);
    b2Vec2 dA = anchorA - groundA;
    lengthA = dA.Length;
    b2Vec2 dB = anchorB - groundB;
    lengthB = dB.Length;
    ratio = r;
    System.Diagnostics.Debug.Assert(ratio > b2Settings.b2_epsilon);
}

        /// The first ground anchor in world coordinates. This point never moves.
        public b2Vec2 groundAnchorA;

        /// The second ground anchor in world coordinates. This point never moves.
        public b2Vec2 groundAnchorB;

        /// The local anchor point relative to bodyA's origin.
        public b2Vec2 localAnchorA;

        /// The local anchor point relative to bodyB's origin.
        public b2Vec2 localAnchorB;

        /// The a reference length for the segment attached to bodyA.
        public float lengthA;

        /// The a reference length for the segment attached to bodyB.
        public float lengthB;

        /// The pulley ratio, used to simulate a block-and-tackle.
        public float ratio;
    }
}
