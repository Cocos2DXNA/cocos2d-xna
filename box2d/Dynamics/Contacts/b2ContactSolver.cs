/*
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org
*
* This software is provided 'as-is', without any express or implied
* warranty.  In no event will the authors be held liable for any damages
* arising from the use of this software.
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications, and to alter it and redistribute it
* freely, subject to the following restrictions:
* 1. The origin of this software must not be misrepresented; you must not
* claim that you wrote the original software. If you use this software
* in a product, an acknowledgment in the product documentation would be
* appreciated but is not required.
* 2. Altered source versions must be plainly marked as such, and must not be
* misrepresented as being the original software.
* 3. This notice may not be removed or altered from any source distribution.
*/
using System;
using System.Diagnostics;
using Box2D.Common;
using Box2D.Collision;
using Box2D.Collision.Shapes;

namespace Box2D.Dynamics.Contacts
{

    public struct b2VelocityConstraintPoint
    {
        
        public void Defaults() 
        {
            rA = b2Vec2.Zero;
            rB = b2Vec2.Zero;
        }
        
        static public b2VelocityConstraintPoint Create() 
        {
            var vcp = new b2VelocityConstraintPoint();
            vcp.Defaults();
            return vcp;
        }

        public b2Vec2 rA;
        public b2Vec2 rB;
        public float normalImpulse;
        public float tangentImpulse;
        public float normalMass;
        public float tangentMass;
        public float velocityBias;
    }

    public struct b2ContactVelocityConstraint
    {
        

		public void Defaults() 
        {
            points = new b2VelocityConstraintPoint[2] {b2VelocityConstraintPoint.Create(), b2VelocityConstraintPoint.Create()};
            normal = b2Vec2.Zero;

        }
        
        static public b2ContactVelocityConstraint Create() 
        {
            var cvc = new b2ContactVelocityConstraint();
            cvc.Defaults();
            return cvc;
        }

        public b2VelocityConstraintPoint[] points;
        public b2Vec2 normal;
        public b2Mat22 normalMass;
        public b2Mat22 K;
        public int indexA;
        public int indexB;
        public float invMassA, invMassB;
        public float invIA, invIB;
        public float friction;
        public float restitution;
        public int pointCount;
        public int contactIndex;
    }

    public struct b2ContactSolverDef
    {
        public b2TimeStep step;
        public b2Contact[] contacts;
        public int count;
        public b2Position[] positions;
        public b2Velocity[] velocities;
    }

    public struct b2ContactPositionConstraint
    {
        
        public void Defaults() 
        {
            localPoints = new b2Vec2[] { b2Vec2.Zero, b2Vec2.Zero};
            localNormal = b2Vec2.Zero;
            localPoint = b2Vec2.Zero;

        }
        
        static public b2ContactPositionConstraint Create() 
        {
            var cvc = new b2ContactPositionConstraint();
            cvc.Defaults();
            return cvc;
        }

        public b2Vec2[] localPoints;
        public b2Vec2 localNormal;
        public b2Vec2 localPoint;
        public int indexA;
        public int indexB;
        public float invMassA, invMassB;
        public b2Vec2 localCenterA, localCenterB;
        public float invIA, invIB;
        public b2ManifoldType type;
        public float radiusA, radiusB;
        public int pointCount;
    };

    public class b2ContactSolver
    {
        public b2TimeStep m_step;
        public b2Position[] m_positions;
        public b2Velocity[] m_velocities;
        public b2ContactPositionConstraint[] m_positionConstraints;
        public b2ContactVelocityConstraint[] m_velocityConstraints;
        public b2Contact[] m_contacts;
        public int m_count;

        public b2ContactSolver(b2ContactSolverDef def)
        {
            m_step = def.step;
            m_count = def.count;
            m_positionConstraints = new b2ContactPositionConstraint[m_count];
            for (int pc = 0; pc < m_count; pc++)
                m_positionConstraints[pc] = b2ContactPositionConstraint.Create();

            m_velocityConstraints = new b2ContactVelocityConstraint[m_count];
            for (int vc = 0; vc < m_count; vc++)
                m_velocityConstraints[vc] = b2ContactVelocityConstraint.Create();

            m_positions = def.positions;
            m_velocities = def.velocities;
            m_contacts = def.contacts;

            // Initialize position independent portions of the constraints.
            for (int i = 0; i < m_count; ++i)
            {
                b2Contact contact = m_contacts[i];

                b2Fixture fixtureA = contact.FixtureA;
                b2Fixture fixtureB = contact.FixtureB;
                b2Shape shapeA = fixtureA.Shape;
                b2Shape shapeB = fixtureB.Shape;
                float radiusA = shapeA.Radius;
                float radiusB = shapeB.Radius;
                b2Body bodyA = fixtureA.Body;
                b2Body bodyB = fixtureB.Body;
                b2Manifold manifold = contact.GetManifold();

                int pointCount = manifold.pointCount;
                Debug.Assert(pointCount > 0);

                b2ContactVelocityConstraint vc = m_velocityConstraints[i];
                vc.friction = contact.Friction;
                vc.restitution = contact.Restitution;
                vc.indexA = bodyA.IslandIndex;
                vc.indexB = bodyB.IslandIndex;
                vc.invMassA = bodyA.InvertedMass;
                vc.invMassB = bodyB.InvertedMass;
                vc.invIA = bodyA.InvertedI;
                vc.invIB = bodyB.InvertedI;
                vc.contactIndex = i;
                vc.pointCount = pointCount;
                vc.K.SetZero();
                vc.normalMass.SetZero();

                b2ContactPositionConstraint pc = m_positionConstraints[i];
                pc.indexA = bodyA.IslandIndex;
                pc.indexB = bodyB.IslandIndex;
                pc.invMassA = bodyA.InvertedMass;
                pc.invMassB = bodyB.InvertedMass;
                pc.localCenterA = bodyA.Sweep.localCenter;
                pc.localCenterB = bodyB.Sweep.localCenter;
                pc.invIA = bodyA.InvertedI;
                pc.invIB = bodyB.InvertedI;
                pc.localNormal = manifold.localNormal;
                pc.localPoint = manifold.localPoint;
                pc.pointCount = pointCount;
                pc.radiusA = radiusA;
                pc.radiusB = radiusB;
                pc.type = manifold.type;

                for (int j = 0; j < pointCount; ++j)
                {
                    b2ManifoldPoint cp = manifold.points[j];
                    b2VelocityConstraintPoint vcp = vc.points[j];

                    if (m_step.warmStarting)
                    {
                        vcp.normalImpulse = m_step.dtRatio * cp.normalImpulse;
                        vcp.tangentImpulse = m_step.dtRatio * cp.tangentImpulse;
                    }
                    else
                    {
                        vcp.normalImpulse = 0.0f;
                        vcp.tangentImpulse = 0.0f;
                    }

                    vcp.rA.SetZero();
                    vcp.rB.SetZero();
                    vcp.normalMass = 0.0f;
                    vcp.tangentMass = 0.0f;
                    vcp.velocityBias = 0.0f;

                    pc.localPoints[j] = cp.localPoint;
                    vc.points[j] = vcp;
                }
                //Put back the struct data since struct data is copied by value
                m_positionConstraints[i] = pc;
                m_velocityConstraints[i] = vc;
            }
        }

        // Initialize position dependent portions of the velocity constraints.
        public virtual void InitializeVelocityConstraints()
        {
            for (int i = 0; i < m_count; ++i)
            {
                b2ContactVelocityConstraint vc = m_velocityConstraints[i];
                b2ContactPositionConstraint pc = m_positionConstraints[i];

                float radiusA = pc.radiusA;
                float radiusB = pc.radiusB;
                b2Manifold manifold = m_contacts[vc.contactIndex].GetManifold();

                int indexA = vc.indexA;
                int indexB = vc.indexB;

                float mA = vc.invMassA;
                float mB = vc.invMassB;
                float iA = vc.invIA;
                float iB = vc.invIB;
                b2Vec2 localCenterA = pc.localCenterA;
                b2Vec2 localCenterB = pc.localCenterB;

                b2Vec2 cA = m_positions[indexA].c;
                float aA = m_positions[indexA].a;
                b2Vec2 vA = m_velocities[indexA].v;
                float wA = m_velocities[indexA].w;

                b2Vec2 cB = m_positions[indexB].c;
                float aB = m_positions[indexB].a;
                b2Vec2 vB = m_velocities[indexB].v;
                float wB = m_velocities[indexB].w;

                Debug.Assert(manifold.pointCount > 0);

                b2Transform xfA = b2Transform.Create(), xfB = b2Transform.Create();
                xfA.q.Set(aA);
                xfB.q.Set(aB);
                xfA.p = cA - b2Math.b2Mul(xfA.q, localCenterA);
                xfB.p = cB - b2Math.b2Mul(xfB.q, localCenterB);

                b2WorldManifold worldManifold = new b2WorldManifold();
                worldManifold.Initialize(ref manifold, xfA, radiusA, xfB, radiusB);

                vc.normal = worldManifold.normal;

                int pointCount = vc.pointCount;
                for (int j = 0; j < pointCount; ++j)
                {
                    b2VelocityConstraintPoint vcp = vc.points[j];

                    vcp.rA = worldManifold.points[j] - cA;
                    vcp.rB = worldManifold.points[j] - cB;

                    float rnA = b2Math.b2Cross(vcp.rA, vc.normal);
                    float rnB = b2Math.b2Cross(vcp.rB, vc.normal);

                    float kNormal = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    vcp.normalMass = kNormal > 0.0f ? 1.0f / kNormal : 0.0f;

                    b2Vec2 tangent = b2Math.b2Cross(vc.normal, 1.0f);

                    float rtA = b2Math.b2Cross(vcp.rA, tangent);
                    float rtB = b2Math.b2Cross(vcp.rB, tangent);

                    float kTangent = mA + mB + iA * rtA * rtA + iB * rtB * rtB;

                    vcp.tangentMass = kTangent > 0.0f ? 1.0f / kTangent : 0.0f;

                    // Setup a velocity bias for restitution.
                    vcp.velocityBias = 0.0f;
                    float vRel = b2Math.b2Dot(vc.normal, vB + b2Math.b2Cross(wB, vcp.rB) - vA - b2Math.b2Cross(wA, vcp.rA));
                    if (vRel < -b2Settings.b2_velocityThreshold)
                    {
                        vcp.velocityBias = -vc.restitution * vRel;
                    }
                    vc.points[j] = vcp;
                }

                // If we have two points, then prepare the block solver.
                if (vc.pointCount == 2)
                {
                    b2VelocityConstraintPoint vcp1 = vc.points[0];
                    b2VelocityConstraintPoint vcp2 = vc.points[1];

                    float rn1A = b2Math.b2Cross(vcp1.rA, vc.normal);
                    float rn1B = b2Math.b2Cross(vcp1.rB, vc.normal);
                    float rn2A = b2Math.b2Cross(vcp2.rA, vc.normal);
                    float rn2B = b2Math.b2Cross(vcp2.rB, vc.normal);

                    float k11 = mA + mB + iA * rn1A * rn1A + iB * rn1B * rn1B;
                    float k22 = mA + mB + iA * rn2A * rn2A + iB * rn2B * rn2B;
                    float k12 = mA + mB + iA * rn1A * rn2A + iB * rn1B * rn2B;

                    // Ensure a reasonable condition number.
                    float k_maxConditionNumber = 1000.0f;
                    if (k11 * k11 < k_maxConditionNumber * (k11 * k22 - k12 * k12))
                    {
                        // K is safe to invert.
                        vc.K.ex.Set(k11, k12);
                        vc.K.ey.Set(k12, k22);
                        vc.normalMass = vc.K.GetInverse();
                    }
                    else
                    {
                        // The constraints are redundant, just use one.
                        // TODO_ERIN use deepest?
                        vc.pointCount = 1;
                    }
                }
                m_positionConstraints[i] = pc;
                m_velocityConstraints[i] = vc;
            }
        }

        public virtual void WarmStart()
        {
            // Warm start.
            for (int i = 0; i < m_count; ++i)
            {
                b2ContactVelocityConstraint vc = m_velocityConstraints[i];

                int indexA = vc.indexA;
                int indexB = vc.indexB;
                float mA = vc.invMassA;
                float iA = vc.invIA;
                float mB = vc.invMassB;
                float iB = vc.invIB;
                int pointCount = vc.pointCount;

                b2Vec2 vA = m_velocities[indexA].v;
                float wA = m_velocities[indexA].w;
                b2Vec2 vB = m_velocities[indexB].v;
                float wB = m_velocities[indexB].w;

                b2Vec2 normal = vc.normal;
                b2Vec2 tangent = b2Math.b2Cross(normal, 1.0f);

                for (int j = 0; j < pointCount; ++j)
                {
                    b2VelocityConstraintPoint vcp = vc.points[j];
                    b2Vec2 P = vcp.normalImpulse * normal + vcp.tangentImpulse * tangent;
                    wA -= iA * b2Math.b2Cross(vcp.rA, P);
                    vA -= mA * P;
                    wB += iB * b2Math.b2Cross(vcp.rB, P);
                    vB += mB * P;
                }

                m_velocities[indexA].v = vA;
                m_velocities[indexA].w = wA;
                m_velocities[indexB].v = vB;
                m_velocities[indexB].w = wB;
            }
        }

        public virtual void SolveVelocityConstraints()
        {
            for (int i = 0; i < m_count; ++i)
            {
                b2ContactVelocityConstraint vc = m_velocityConstraints[i];

                int indexA = vc.indexA;
                int indexB = vc.indexB;
                float mA = vc.invMassA;
                float iA = vc.invIA;
                float mB = vc.invMassB;
                float iB = vc.invIB;
                int pointCount = vc.pointCount;

                b2Vec2 vA = m_velocities[indexA].v;
                float wA = m_velocities[indexA].w;
                b2Vec2 vB = m_velocities[indexB].v;
                float wB = m_velocities[indexB].w;

                b2Vec2 normal = vc.normal;
                b2Vec2 tangent = b2Math.b2Cross(normal, 1.0f);
                float friction = vc.friction;

                Debug.Assert(pointCount == 1 || pointCount == 2);

                // Solve tangent constraints first because non-penetration is more important
                // than friction.
                for (int j = 0; j < pointCount; ++j)
                {
                    b2VelocityConstraintPoint vcp = vc.points[j];

                    // Relative velocity at contact
                    b2Vec2 dv = vB + b2Math.b2Cross(wB, vcp.rB) - vA - b2Math.b2Cross(wA, vcp.rA);

                    // Compute tangent force
                    float vt = b2Math.b2Dot(dv, tangent);
                    float lambda = vcp.tangentMass * (-vt);

                    // b2Math.b2Clamp the accumulated force
                    float maxFriction = friction * vcp.normalImpulse;
                    float newImpulse = b2Math.b2Clamp(vcp.tangentImpulse + lambda, -maxFriction, maxFriction);
                    lambda = newImpulse - vcp.tangentImpulse;
                    vcp.tangentImpulse = newImpulse;

                    // Apply contact impulse
                    b2Vec2 P = lambda * tangent;

                    vA -= mA * P;
                    wA -= iA * b2Math.b2Cross(vcp.rA, P);

                    vB += mB * P;
                    wB += iB * b2Math.b2Cross(vcp.rB, P);
                    vc.points[j] = vcp;
                }

                // Solve normal constraints
                if (vc.pointCount == 1)
                {
                    b2VelocityConstraintPoint vcp = vc.points[0];

                    // Relative velocity at contact
                    b2Vec2 dv = vB + b2Math.b2Cross(wB, vcp.rB) - vA - b2Math.b2Cross(wA, vcp.rA);

                    // Compute normal impulse
                    float vn = b2Math.b2Dot(dv, normal);
                    float lambda = -vcp.normalMass * (vn - vcp.velocityBias);

                    // b2Math.b2Clamp the accumulated impulse
                    float newImpulse = Math.Max(vcp.normalImpulse + lambda, 0.0f);
                    lambda = newImpulse - vcp.normalImpulse;
                    vcp.normalImpulse = newImpulse;

                    // Apply contact impulse
                    b2Vec2 P = lambda * normal;
                    vA -= mA * P;
                    wA -= iA * b2Math.b2Cross(vcp.rA, P);

                    vB += mB * P;
                    wB += iB * b2Math.b2Cross(vcp.rB, P);
                    vc.points[0] = vcp;
                }
                else
                {
                    // Block solver developed in collaboration with Dirk Gregorius (back in 01/07 on Box2D_Lite).
                    // Build the mini LCP for this contact patch
                    //
                    // vn = A * x + b, vn >= 0, , vn >= 0, x >= 0 and vn_i * x_i = 0 with i = 1..2
                    //
                    // A = J * W * JT and J = ( -n, -r1 x n, n, r2 x n )
                    // b = vn0 - velocityBias
                    //
                    // The system is solved using the "Total enumeration method" (s. Murty). The complementary constraint vn_i * x_i
                    // implies that we must have in any solution either vn_i = 0 or x_i = 0. So for the 2D contact problem the cases
                    // vn1 = 0 and vn2 = 0, x1 = 0 and x2 = 0, x1 = 0 and vn2 = 0, x2 = 0 and vn1 = 0 need to be tested. The first valid
                    // solution that satisfies the problem is chosen.
                    // 
                    // In order to account of the accumulated impulse 'a' (because of the iterative nature of the solver which only requires
                    // that the accumulated impulse is clamped and not the incremental impulse) we change the impulse variable (x_i).
                    //
                    // Substitute:
                    // 
                    // x = a + d
                    // 
                    // a := old total impulse
                    // x := new total impulse
                    // d := incremental impulse 
                    //
                    // For the current iteration we extend the formula for the incremental impulse
                    // to compute the new total impulse:
                    //
                    // vn = A * d + b
                    //    = A * (x - a) + b
                    //    = A * x + b - A * a
                    //    = A * x + b'
                    // b' = b - A * a;

                    b2VelocityConstraintPoint cp1 = vc.points[0];
                    b2VelocityConstraintPoint cp2 = vc.points[1];

                    b2Vec2 a = new b2Vec2(cp1.normalImpulse, cp2.normalImpulse);
                    Debug.Assert(a.x >= 0.0f && a.y >= 0.0f);

                    // Relative velocity at contact
                    b2Vec2 dv1 = vB + b2Math.b2Cross(wB, cp1.rB) - vA - b2Math.b2Cross(wA, cp1.rA);
                    b2Vec2 dv2 = vB + b2Math.b2Cross(wB, cp2.rB) - vA - b2Math.b2Cross(wA, cp2.rA);

                    // Compute normal velocity
                    float vn1 = b2Math.b2Dot(dv1, normal);
                    float vn2 = b2Math.b2Dot(dv2, normal);

                    b2Vec2 b = b2Vec2.Zero;
                    b.x = vn1 - cp1.velocityBias;
                    b.y = vn2 - cp2.velocityBias;

                    // Compute b'
                    b -= b2Math.b2Mul(vc.K, a);

                    //            float k_errorTol = 1e-3f;
                    #region Iteration 
                    while (true)
                    {
                        //
                        // Case 1: vn = 0
                        //
                        // 0 = A * x + b'
                        //
                        // Solve for x:
                        //
                        // x = - inv(A) * b'
                        //
                        b2Vec2 x = -b2Math.b2Mul(vc.normalMass, b);

                        if (x.x >= 0.0f && x.y >= 0.0f)
                        {
                            // Get the incremental impulse
                            b2Vec2 d = x - a;

                            // Apply incremental impulse
                            b2Vec2 P1 = d.x * normal;
                            b2Vec2 P2 = d.y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (b2Math.b2Cross(cp1.rA, P1) + b2Math.b2Cross(cp2.rA, P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (b2Math.b2Cross(cp1.rB, P1) + b2Math.b2Cross(cp2.rB, P2));

                            // Accumulate
                            cp1.normalImpulse = x.x;
                            cp2.normalImpulse = x.y;

#if B2_DEBUG_SOLVER
                    // Postconditions
                    dv1 = vB + b2Math.b2Cross(wB, cp1.rB) - vA - b2Math.b2Cross(wA, cp1.rA);
                    dv2 = vB + b2Math.b2Cross(wB, cp2.rB) - vA - b2Math.b2Cross(wA, cp2.rA);

                    // Compute normal velocity
                    vn1 = b2Math.b2Dot(dv1, normal);
                    vn2 = b2Math.b2Dot(dv2, normal);

                    Debug.Assert(b2Abs(vn1 - cp1.velocityBias) < k_errorTol);
                    Debug.Assert(b2Abs(vn2 - cp2.velocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 2: vn1 = 0 and x2 = 0
                        //
                        //   0 = a11 * x1 + a12 * 0 + b1' 
                        // vn2 = a21 * x1 + a22 * 0 + b2'
                        //
                        x.x = -cp1.normalMass * b.x;
                        x.y = 0.0f;
                        vn1 = 0.0f;
                        vn2 = vc.K.ex.y * x.x + b.y;

                        if (x.x >= 0.0f && vn2 >= 0.0f)
                        {
                            // Get the incremental impulse
                            b2Vec2 d = x - a;

                            // Apply incremental impulse
                            b2Vec2 P1 = d.x * normal;
                            b2Vec2 P2 = d.y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (b2Math.b2Cross(cp1.rA, P1) + b2Math.b2Cross(cp2.rA, P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (b2Math.b2Cross(cp1.rB, P1) + b2Math.b2Cross(cp2.rB, P2));

                            // Accumulate
                            cp1.normalImpulse = x.x;
                            cp2.normalImpulse = x.y;

#if B2_DEBUG_SOLVER
                    // Postconditions
                    dv1 = vB + b2Math.b2Cross(wB, cp1.rB) - vA - b2Math.b2Cross(wA, cp1.rA);

                    // Compute normal velocity
                    vn1 = b2Math.b2Dot(dv1, normal);

                    Debug.Assert(b2Abs(vn1 - cp1.velocityBias) < k_errorTol);
#endif
                            break;
                        }


                        //
                        // Case 3: vn2 = 0 and x1 = 0
                        //
                        // vn1 = a11 * 0 + a12 * x2 + b1' 
                        //   0 = a21 * 0 + a22 * x2 + b2'
                        //
                        x.x = 0.0f;
                        x.y = -cp2.normalMass * b.y;
                        vn1 = vc.K.ey.x * x.y + b.x;
                        vn2 = 0.0f;

                        if (x.y >= 0.0f && vn1 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            b2Vec2 d = x - a;

                            // Apply incremental impulse
                            b2Vec2 P1 = d.x * normal;
                            b2Vec2 P2 = d.y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (b2Math.b2Cross(cp1.rA, P1) + b2Math.b2Cross(cp2.rA, P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (b2Math.b2Cross(cp1.rB, P1) + b2Math.b2Cross(cp2.rB, P2));

                            // Accumulate
                            cp1.normalImpulse = x.x;
                            cp2.normalImpulse = x.y;

#if B2_DEBUG_SOLVER
                    // Postconditions
                    dv2 = vB + b2Math.b2Cross(wB, cp2.rB) - vA - b2Math.b2Cross(wA, cp2.rA);

                    // Compute normal velocity
                    vn2 = b2Math.b2Dot(dv2, normal);

                    Debug.Assert(b2Abs(vn2 - cp2.velocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 4: x1 = 0 and x2 = 0
                        // 
                        // vn1 = b1
                        // vn2 = b2;
                        x.x = 0.0f;
                        x.y = 0.0f;
                        vn1 = b.x;
                        vn2 = b.y;

                        if (vn1 >= 0.0f && vn2 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            b2Vec2 d = x - a;

                            // Apply incremental impulse
                            b2Vec2 P1 = d.x * normal;
                            b2Vec2 P2 = d.y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (b2Math.b2Cross(cp1.rA, P1) + b2Math.b2Cross(cp2.rA, P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (b2Math.b2Cross(cp1.rB, P1) + b2Math.b2Cross(cp2.rB, P2));

                            // Accumulate
                            cp1.normalImpulse = x.x;
                            cp2.normalImpulse = x.y;

                            break;
                        }


                        // No solution, give up. This is hit sometimes, but it doesn't seem to matter.
                        break;
                    }
                    #endregion
                    vc.points[0] = cp1;
                    vc.points[1] = cp2;
                }

                m_velocities[indexA].v = vA;
                m_velocities[indexA].w = wA;
                m_velocities[indexB].v = vB;
                m_velocities[indexB].w = wB;
                m_velocityConstraints[i] = vc;
            }
        }

        public virtual void StoreImpulses()
        {
            for (int i = 0; i < m_count; ++i)
            {
                b2ContactVelocityConstraint vc = m_velocityConstraints[i];
                b2Manifold manifold = m_contacts[vc.contactIndex].GetManifold();

                for (int j = 0; j < vc.pointCount; ++j)
                {
                    manifold.points[j].normalImpulse = vc.points[j].normalImpulse;
                    manifold.points[j].tangentImpulse = vc.points[j].tangentImpulse;
                }
                m_contacts[vc.contactIndex].SetManifold(manifold);
            }
        }

        public class b2PositionSolverManifold
        {
            public b2PositionSolverManifold(ref b2ContactPositionConstraint pc, ref b2Transform xfA, ref b2Transform xfB, int index)
            {
                Debug.Assert(pc.pointCount > 0);

                switch (pc.type)
                {
                    case b2ManifoldType.e_circles:
                        {
                            b2Vec2 pointA = b2Math.b2Mul(xfA, pc.localPoint);
                            b2Vec2 pointB = b2Math.b2Mul(xfB, pc.localPoints[0]);
                            normal = pointB - pointA;
                            normal.Normalize();
                            point = 0.5f * (pointA + pointB);
                            separation = b2Math.b2Dot(pointB - pointA, normal) - pc.radiusA - pc.radiusB;
                        }
                        break;

                    case b2ManifoldType.e_faceA:
                        {
                            normal = b2Math.b2Mul(xfA.q, pc.localNormal);
                            b2Vec2 planePoint = b2Math.b2Mul(xfA, pc.localPoint);

                            b2Vec2 clipPoint = b2Math.b2Mul(xfB, pc.localPoints[index]);
                            separation = b2Math.b2Dot(clipPoint - planePoint, normal) - pc.radiusA - pc.radiusB;
                            point = clipPoint;
                        }
                        break;

                    case b2ManifoldType.e_faceB:
                        {
                            normal = b2Math.b2Mul(xfB.q, pc.localNormal);
                            b2Vec2 planePoint = b2Math.b2Mul(xfB, pc.localPoint);

                            b2Vec2 clipPoint = b2Math.b2Mul(xfA, pc.localPoints[index]);
                            separation = b2Math.b2Dot(clipPoint - planePoint, normal) - pc.radiusA - pc.radiusB;
                            point = clipPoint;

                            // Ensure normal points from A to B
                            normal = -normal;
                        }
                        break;
                }
            }

            public b2Vec2 normal;
            public b2Vec2 point;
            public float separation;
        }

        // Sequential solver.
        public bool SolvePositionConstraints()
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < m_count; ++i)
            {
                b2ContactPositionConstraint pc = m_positionConstraints[i];

                int indexA = pc.indexA;
                int indexB = pc.indexB;
                b2Vec2 localCenterA = pc.localCenterA;
                float mA = pc.invMassA;
                float iA = pc.invIA;
                b2Vec2 localCenterB = pc.localCenterB;
                float mB = pc.invMassB;
                float iB = pc.invIB;
                int pointCount = pc.pointCount;

                b2Vec2 cA = m_positions[indexA].c;
                float aA = m_positions[indexA].a;

                b2Vec2 cB = m_positions[indexB].c;
                float aB = m_positions[indexB].a;

                // Solve normal constraints
                for (int j = 0; j < pointCount; ++j)
                {
                    b2Transform xfA = b2Transform.Create(), xfB = b2Transform.Create();
                    xfA.q.Set(aA);
                    xfB.q.Set(aB);
                    xfA.p = cA - b2Math.b2Mul(xfA.q, localCenterA);
                    xfB.p = cB - b2Math.b2Mul(xfB.q, localCenterB);

                    b2PositionSolverManifold psm = new b2PositionSolverManifold(ref pc, ref xfA, ref xfB, j);
                    b2Vec2 normal = psm.normal;

                    b2Vec2 point = psm.point;
                    float separation = psm.separation;

                    b2Vec2 rA = point - cA;
                    b2Vec2 rB = point - cB;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = b2Math.b2Clamp(b2Settings.b2_baumgarte * (separation + b2Settings.b2_linearSlop), -b2Settings.b2_maxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = b2Math.b2Cross(rA, normal);
                    float rnB = b2Math.b2Cross(rB, normal);
                    float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = K > 0.0f ? -C / K : 0.0f;

                    b2Vec2 P = impulse * normal;

                    cA -= mA * P;
                    aA -= iA * b2Math.b2Cross(rA, P);

                    cB += mB * P;
                    aB += iB * b2Math.b2Cross(rB, P);
                }

                m_positions[indexA].c = cA;
                m_positions[indexA].a = aA;

                m_positions[indexB].c = cB;
                m_positions[indexB].a = aB;

                m_positionConstraints[i] = pc;
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -3.0f * b2Settings.b2_linearSlop;
        }

        // Sequential position solver for position constraints.
        public bool SolveTOIPositionConstraints(int toiIndexA, int toiIndexB)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < m_count; ++i)
            {
                b2ContactPositionConstraint pc = m_positionConstraints[i];

                int indexA = pc.indexA;
                int indexB = pc.indexB;
                b2Vec2 localCenterA = pc.localCenterA;
                b2Vec2 localCenterB = pc.localCenterB;
                int pointCount = pc.pointCount;

                float mA = 0.0f;
                float iA = 0.0f;
                if (indexA == toiIndexA || indexA == toiIndexB)
                {
                    mA = pc.invMassA;
                    iA = pc.invIA;
                }

                float mB = pc.invMassB;
                float iB = pc.invIB;
                if (indexB == toiIndexA || indexB == toiIndexB)
                {
                    mB = pc.invMassB;
                    iB = pc.invIB;
                }

                b2Vec2 cA = m_positions[indexA].c;
                float aA = m_positions[indexA].a;

                b2Vec2 cB = m_positions[indexB].c;
                float aB = m_positions[indexB].a;

                // Solve normal constraints
                for (int j = 0; j < pointCount; ++j)
                {
                    b2Transform xfA = b2Transform.Create(), xfB = b2Transform.Create();
                    xfA.q.Set(aA);
                    xfB.q.Set(aB);
                    xfA.p = cA - b2Math.b2Mul(xfA.q, localCenterA);
                    xfB.p = cB - b2Math.b2Mul(xfB.q, localCenterB);

                    b2PositionSolverManifold psm = new b2PositionSolverManifold(ref pc, ref xfA, ref xfB, j);
                    b2Vec2 normal = psm.normal;

                    b2Vec2 point = psm.point;
                    float separation = psm.separation;

                    b2Vec2 rA = point - cA;
                    b2Vec2 rB = point - cB;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = b2Math.b2Clamp(b2Settings.b2_toiBaugarte * (separation + b2Settings.b2_linearSlop), -b2Settings.b2_maxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = b2Math.b2Cross(rA, normal);
                    float rnB = b2Math.b2Cross(rB, normal);
                    float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = K > 0.0f ? -C / K : 0.0f;

                    b2Vec2 P = impulse * normal;

                    cA -= mA * P;
                    aA -= iA * b2Math.b2Cross(rA, P);

                    cB += mB * P;
                    aB += iB * b2Math.b2Cross(rB, P);
                }

                m_positions[indexA].c = cA;
                m_positions[indexA].a = aA;

                m_positions[indexB].c = cB;
                m_positions[indexB].a = aB;

                m_positionConstraints[i] = pc;
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -1.5f * b2Settings.b2_linearSlop;
        }
    }
}