using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2D.Common;
using Box2D.Collision.Shapes;

namespace Box2D.Collision
{

    /// This is used for determining the state of contact points.
    public enum b2PointState
    {
        b2_nullState,        //< point does not exist
        b2_addState,        //< point was added in the update
        b2_persistState,    //< point persisted across the update
        b2_removeState        //< point was removed in the update
    }

    /// Used for computing contact manifolds.
    public struct b2ClipVertex
    {
        public b2Vec2 v;
        public b2ContactFeature id;

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine("b2ClipVertex {{ v={0},{1} - feature={4}@{2},{5}@{3} }}", v.x, v.y, id.indexA, id.indexB, id.typeA, id.typeB);
        }
    };

    /// Ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
    public struct b2RayCastInput
    {
        public b2Vec2 p1, p2;
        public float maxFraction;
    };

    /// Ray-cast output data. The ray hits at p1 + fraction * (p2 - p1), where p1 and p2
    /// come from b2RayCastInput.
    public struct b2RayCastOutput
    {
        public b2Vec2 normal;
        public float fraction;

        public b2RayCastOutput(b2Vec2 b, float f)
        {
            normal = b;
            fraction = f;
        }
        public static b2RayCastOutput Zero = new b2RayCastOutput(b2Vec2.Zero, 0f);
    }

    public abstract class b2Collision
    {
        public static byte b2_nullFeature = byte.MaxValue;

        /// Compute the point states given two manifolds. The states pertain to the transition from manifold1
        /// to manifold2. So state1 is either persist or remove while state2 is either add or persist.
        public static void b2GetPointStates(b2PointState[] state1, b2PointState[] state2, b2Manifold manifold1, b2Manifold manifold2)
        {
            for (int i = 0; i < b2Settings.b2_maxManifoldPoints; ++i)
            {
                state1[i] = b2PointState.b2_nullState;
                state2[i] = b2PointState.b2_nullState;
            }

            // Detect persists and removes.
            for (int i = 0; i < manifold1.pointCount; ++i)
            {
                b2ContactFeature id = manifold1.points[i].id;

                state1[i] = b2PointState.b2_removeState;

                for (int j = 0; j < manifold2.pointCount; ++j)
                {
                    if (manifold2.points[j].id.Equals(ref id))
                    {
                        state1[i] = b2PointState.b2_persistState;
                        break;
                    }
                }
            }

            // Detect persists and adds.
            for (int i = 0; i < manifold2.pointCount; ++i)
            {
                b2ContactFeature id = manifold2.points[i].id;

                state2[i] = b2PointState.b2_addState;

                for (int j = 0; j < manifold1.pointCount; ++j)
                {
                    if (manifold1.points[j].id.Equals(ref id))
                    {
                        state2[i] = b2PointState.b2_persistState;
                        break;
                    }
                }
            }
        }

        /// Compute the collision manifold between two circles.
        public static void b2CollideCircles(b2Manifold manifold, b2CircleShape circleA, ref b2Transform xfA, b2CircleShape circleB, ref b2Transform xfB)
        {
            manifold.pointCount = 0;

            float pAx = (xfA.q.c * circleA.Position.x - xfA.q.s * circleA.Position.y) + xfA.p.x;
            float pAy = (xfA.q.s * circleA.Position.x + xfA.q.c * circleA.Position.y) + xfA.p.y;

            float pBx = (xfB.q.c * circleB.Position.x - xfB.q.s * circleB.Position.y) + xfB.p.x;
            float pBy = (xfB.q.s * circleB.Position.x + xfB.q.c * circleB.Position.y) + xfB.p.y;

            float dx = pBx - pAx;
            float dy = pBy - pAy;

            float distSqr = dx * dx + dy * dy;

            float rA = circleA.Radius, rB = circleB.Radius;
            float radius = rA + rB;
            
            if (distSqr > radius * radius)
            {
                return;
            }

            manifold.type = b2ManifoldType.e_circles;
            manifold.localPoint = circleA.Position;
            manifold.localNormal = b2Vec2.Zero;
            manifold.pointCount = 1;

            manifold.points[0].localPoint = circleB.Position;
            manifold.points[0].id.key = 0;
        }

        /// Compute the collision manifold between a polygon and a circle.
        public static void b2CollidePolygonAndCircle(b2Manifold manifold, b2PolygonShape polygonA, ref b2Transform xfA, b2CircleShape circleB, ref b2Transform xfB)
        {
            manifold.pointCount = 0;

            // Compute circle position in the frame of the polygon.
            b2Vec2 c;
            c.x = (xfB.q.c * circleB.Position.x - xfB.q.s * circleB.Position.y) + xfB.p.x;
            c.y = (xfB.q.s * circleB.Position.x + xfB.q.c * circleB.Position.y) + xfB.p.y;

            b2Vec2 cLocal;
            float px = c.x - xfA.p.x;
            float py = c.y - xfA.p.y;
            cLocal.x = (xfA.q.c * px + xfA.q.s * py);
            cLocal.y = (-xfA.q.s * px + xfA.q.c * py);

            // Find the min separating edge.
            int normalIndex = 0;
            float separation = -b2Settings.b2_maxFloat;
            float radius = polygonA.Radius + circleB.Radius;
            int vertexCount = polygonA.m_vertexCount;
            b2Vec2[] vertices = polygonA.Vertices;
            b2Vec2[] normals = polygonA.Normals;

            for (int i = 0; i < vertexCount; ++i)
            {
                b2Vec2 tmp;
                tmp.x = cLocal.x - vertices[i].x;
                tmp.y = cLocal.y - vertices[i].y;
                float s = normals[i].x * tmp.x + normals[i].y * tmp.y; // b2Math.b2Dot(normals[i], cLocal - vertices[i]);

                if (s > radius)
                {
                    // Early out.
                    return;
                }

                if (s > separation)
                {
                    separation = s;
                    normalIndex = i;
                }
            }

            // Vertices that subtend the incident face.
            int vertIndex1 = normalIndex;
            int vertIndex2 = vertIndex1 + 1 < vertexCount ? vertIndex1 + 1 : 0;
            b2Vec2 v1 = vertices[vertIndex1];
            b2Vec2 v2 = vertices[vertIndex2];

            // If the center is inside the polygon ...
            if (separation < b2Settings.b2_epsilon)
            {
                manifold.pointCount = 1;
                manifold.type = b2ManifoldType.e_faceA;
                manifold.localNormal = normals[normalIndex];
                manifold.localPoint.x = 0.5f * (v1.x + v2.x);
                manifold.localPoint.y = 0.5f * (v1.y + v2.y);
                manifold.points[0].localPoint = circleB.Position;
                manifold.points[0].id.key = 0;
                return;
            }

            // Compute barycentric coordinates
            float ax = cLocal.x - v1.x;
            float ay = cLocal.y - v1.y;
            float bx = v2.x - v1.x;
            float by = v2.y - v1.y;
            float u1 = ax * bx + ay * by;

            ax = cLocal.x - v2.x;
            ay = cLocal.y - v2.y;
            bx = v1.x - v2.x;
            by = v1.y - v2.y;
            float u2 = ax * bx + ay * by;
            
            if (u1 <= 0.0f)
            {
                if (b2Math.b2DistanceSquared(ref cLocal, ref v1) > radius * radius)
                {
                    return;
                }

                manifold.pointCount = 1;
                manifold.type = b2ManifoldType.e_faceA;
                manifold.localNormal = cLocal - v1;
                manifold.localNormal.Normalize();
                manifold.localPoint = v1;
                manifold.points[0].localPoint = circleB.Position;
                manifold.points[0].id.key = 0;
            }
            else if (u2 <= 0.0f)
            {
                if (b2Math.b2DistanceSquared(ref cLocal, ref v2) > radius * radius)
                {
                    return;
                }

                manifold.pointCount = 1;
                manifold.type = b2ManifoldType.e_faceA;
                manifold.localNormal.x = cLocal.x - v2.x;
                manifold.localNormal.y = cLocal.y - v2.y;
                manifold.localNormal.Normalize();
                manifold.localPoint = v2;
                manifold.points[0].localPoint = circleB.Position;
                manifold.points[0].id.key = 0;
            }
            else
            {
                b2Vec2 faceCenter;
                faceCenter.x = 0.5f * (v1.x + v2.x);
                faceCenter.y = 0.5f * (v1.y + v2.y);

                b2Vec2 a;
                a.x = cLocal.x - faceCenter.x;
                a.y = cLocal.y - faceCenter.y;

                separation = b2Math.b2Dot(ref a, ref normals[vertIndex1]);
                
                if (separation > radius)
                {
                    return;
                }

                manifold.pointCount = 1;
                manifold.type = b2ManifoldType.e_faceA;
                manifold.localNormal = normals[vertIndex1];
                manifold.localPoint = faceCenter;
                manifold.points[0].localPoint = circleB.Position;
                manifold.points[0].id.key = 0;
            }
        }

        /// Compute the collision manifold between two polygons.
        // Find edge normal of max separation on A - return if separating axis is found
        // Find edge normal of max separation on B - return if separation axis is found
        // Choose reference edge as min(minA, minB)
        // Find incident edge
        // Clip
        // The normal points from 1 to 2

        //Memory Oprimization
        private static b2ClipVertex[] _incidentEdge = new b2ClipVertex[2];
        private static b2ClipVertex[] _clipPoints1 = new b2ClipVertex[2];
        private static b2ClipVertex[] _clipPoints2 = new b2ClipVertex[2];

        public static void b2CollidePolygons(b2Manifold manifold, b2PolygonShape polyA, ref b2Transform xfA, b2PolygonShape polyB, ref b2Transform xfB)
        {
            manifold.pointCount = 0;
            float totalRadius = polyA.Radius + polyB.Radius;

            int edgeA = 0;
            float separationA = b2FindMaxSeparation(out edgeA, polyA, ref xfA, polyB, ref xfB);
            if (separationA > totalRadius)
                return;

            int edgeB = 0;
            float separationB = b2FindMaxSeparation(out edgeB, polyB, ref xfB, polyA, ref xfA);
            if (separationB > totalRadius)
                return;

            b2PolygonShape poly1;	// reference polygon
            b2PolygonShape poly2;	// incident polygon
            b2Transform xf1, xf2;
            int edge1;		// reference edge
            byte flip;
            const float k_relativeTol = 0.98f;
            const float k_absoluteTol = 0.001f;

            if (separationB > k_relativeTol * separationA + k_absoluteTol)
            {
                poly1 = polyB;
                poly2 = polyA;
                xf1 = xfB;
                xf2 = xfA;
                edge1 = edgeB;
                manifold.type = b2ManifoldType.e_faceB;
                flip = 1;
            }
            else
            {
                poly1 = polyA;
                poly2 = polyB;
                xf1 = xfA;
                xf2 = xfB;
                edge1 = edgeA;
                manifold.type = b2ManifoldType.e_faceA;
                flip = 0;
            }

            b2ClipVertex[] incidentEdge = _incidentEdge;
            b2FindIncidentEdge(incidentEdge, poly1, ref xf1, edge1, poly2, ref xf2);

            int count1 = poly1.VertexCount;
            b2Vec2[] vertices1 = poly1.Vertices;

            int iv1 = edge1;
            int iv2 = edge1 + 1 < count1 ? edge1 + 1 : 0;

            b2Vec2 v11 = vertices1[iv1];
            b2Vec2 v12 = vertices1[iv2];

            b2Vec2 localTangent;
            localTangent.x = v12.x - v11.x;
            localTangent.y = v12.y - v11.y;
            
            localTangent.Normalize();

            b2Vec2 localNormal;
            localNormal.x = localTangent.y; //.UnitCross(); // b2Math.b2Cross(localTangent, 1.0f);
            localNormal.y = -localTangent.x;
            
            b2Vec2 planePoint;
            planePoint.x = 0.5f * (v11.x + v12.x);
            planePoint.y = 0.5f * (v11.y + v12.y);

            b2Vec2 tangent;
            tangent.x = xf1.q.c * localTangent.x - xf1.q.s * localTangent.y;
            tangent.y = xf1.q.s * localTangent.x + xf1.q.c * localTangent.y;
            
            float normalx = tangent.y; //UnitCross(); //  b2Math.b2Cross(tangent, 1.0f);
            float normaly = -tangent.x;

            float v11x = (xf1.q.c * v11.x - xf1.q.s * v11.y) + xf1.p.x;
            float v11y = (xf1.q.s * v11.x + xf1.q.c * v11.y) + xf1.p.y;
            float v12x = (xf1.q.c * v12.x - xf1.q.s * v12.y) + xf1.p.x;
            float v12y = (xf1.q.s * v12.x + xf1.q.c * v12.y) + xf1.p.y;

            // Face offset.
            float frontOffset = normalx * v11x + normaly * v11y;

            // Side offsets, extended by polytope skin thickness.
            float sideOffset1 = -(tangent.x * v11x + tangent.y * v11y) + totalRadius;
            float sideOffset2 = tangent.x * v12x + tangent.y * v12y + totalRadius;

            // Clip incident edge against extruded edge1 side edges.
            b2ClipVertex[] clipPoints1 = _clipPoints1;
            int np;

            // Clip to box side 1
            b2Vec2 t;
            t.x = -tangent.x;
            t.y = -tangent.y;
            np = b2ClipSegmentToLine(clipPoints1, incidentEdge, ref t, sideOffset1, (byte)iv1);

            if (np < 2)
            {
                return;
            }

            b2ClipVertex[] clipPoints2 = _clipPoints2;

            // Clip to negative box side 1
            np = b2ClipSegmentToLine(clipPoints2, clipPoints1, ref tangent, sideOffset2, (byte)iv2);

            if (np < 2)
            {
                return;
            }

            // Now clipPoints2 contains the clipped points.
            manifold.localNormal = localNormal;
            manifold.localPoint = planePoint;

            int pointCount = 0;
            for (int i = 0; i < b2Settings.b2_maxManifoldPoints; ++i)
            {
                var v = clipPoints2[i].v;
                //float separation = b2Math.b2Dot(ref normal, ref v) - frontOffset;
                float separation = normalx * v.x + normaly * v.y - frontOffset;

                if (separation <= totalRadius)
                {
                    b2ManifoldPoint cp = manifold.points[pointCount];
                    
                    //cp.localPoint = b2Math.b2MulT(ref xf2, ref v);
                    float px = v.x - xf2.p.x;
                    float py = v.y - xf2.p.y;
                    cp.localPoint.x = (xf2.q.c * px + xf2.q.s * py);
                    cp.localPoint.y = (-xf2.q.s * px + xf2.q.c * py);

                    cp.id = clipPoints2[i].id;
                    if (flip != 0)
                    {
                        // Swap features
                        b2ContactFeature cf = cp.id;
                        cp.id.indexA = cf.indexB;
                        cp.id.indexB = cf.indexA;
                        cp.id.typeA = cf.typeB;
                        cp.id.typeB = cf.typeA;
                    }
                    ++pointCount;
                }
            }

            manifold.pointCount = pointCount;
        }

        public static float b2EdgeSeparation(b2PolygonShape poly1, ref b2Transform xf1, int edge1,
                                      b2PolygonShape poly2, ref b2Transform xf2)
        {
            b2Vec2 normal = poly1.Normals[edge1];

            int count2 = poly2.m_vertexCount;
            b2Vec2[] vertices2 = poly2.Vertices;

            // Convert normal from poly1's frame into poly2's frame.
            float normal1Worldx = xf1.q.c * normal.x - xf1.q.s * normal.y;
            float normal1Worldy = xf1.q.s * normal.x + xf1.q.c * normal.y;

            float normal1x = xf2.q.c * normal1Worldx + xf2.q.s * normal1Worldy;
            float normal1y = -xf2.q.s * normal1Worldx + xf2.q.c * normal1Worldy;

            // Find support vertex on poly2 for -normal.
            int index = 0;
            float minDot = b2Settings.b2_maxFloat;

            for (int i = 0; i < count2; ++i)
            {
                var vert = vertices2[i];
                
                float dot = vert.x * normal1x + vert.y * normal1y;
                
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            var v1e1 = poly1.Vertices[edge1];
            var v2i = vertices2[index];

            float v1x = (xf1.q.c * v1e1.x - xf1.q.s * v1e1.y) + xf1.p.x;
            float v1y = (xf1.q.s * v1e1.x + xf1.q.c * v1e1.y) + xf1.p.y;

            float v2x = (xf2.q.c * v2i.x - xf2.q.s * v2i.y) + xf2.p.x;
            float v2y = (xf2.q.s * v2i.x + xf2.q.c * v2i.y) + xf2.p.y;

            v2x -= v1x;
            v2y -= v1y;

            float separation = v2x * normal1Worldx + v2y * normal1Worldy;

            return separation;
        }

        /// Compute the collision manifold between an edge and a circle.
        public static void b2CollideEdgeAndCircle(b2Manifold manifold, b2EdgeShape edgeA, ref b2Transform xfA, b2CircleShape circleB, ref b2Transform xfB)
        {
            manifold.pointCount = 0;

            // Compute circle in frame of edge
            b2Vec2 Q = b2Math.b2MulT(xfA, b2Math.b2Mul(ref xfB, ref circleB.Position));

            b2Vec2 A = edgeA.Vertex1, B = edgeA.Vertex2;
            b2Vec2 e = B - A;
            b2Vec2 diff;

            // Barycentric coordinates
            diff = B - Q;
            float u = b2Math.b2Dot(ref e, ref diff); // B - Q);
            diff = Q - A;
            float v = b2Math.b2Dot(ref e, ref diff); // Q - A);

            float radius = edgeA.Radius + circleB.Radius;

            b2ContactFeature cf = b2ContactFeature.Zero;
            cf.indexB = 0;
            cf.typeB = b2ContactFeatureType.e_vertex;

            // Region A
            if (v <= 0.0f)
            {
                b2Vec2 P = A;
                b2Vec2 d = Q - P;
                float dd = d.LengthSquared; //  b2Math.b2Dot(d, d);
                if (dd > radius * radius)
                {
                    return;
                }

                // Is there an edge connected to A?
                if (edgeA.HasVertex0)
                {
                    b2Vec2 A1 = edgeA.Vertex0;
                    b2Vec2 B1 = A;
                    b2Vec2 e1 = B1 - A1;
                    diff = B1 - Q;
                    float u1 = b2Math.b2Dot(ref e1, ref diff);

                    // Is the circle in Region AB of the previous edge?
                    if (u1 > 0.0f)
                    {
                        return;
                    }
                }

                cf.indexA = 0;
                cf.typeA = b2ContactFeatureType.e_vertex;
                manifold.pointCount = 1;
                manifold.type = b2ManifoldType.e_circles;
                manifold.localNormal.SetZero();
                manifold.localPoint = P;
                manifold.points[0].id.key = 0;
                manifold.points[0].id.Set(cf);
                manifold.points[0].localPoint = circleB.Position;
                return;
            }

            // Region B
            if (u <= 0.0f)
            {
                b2Vec2 P = B;
                b2Vec2 d = Q - P;
                float dd = d.LengthSquared; //  b2Math.b2Dot(d, d);
                if (dd > radius * radius)
                {
                    return;
                }

                // Is there an edge connected to B?
                if (edgeA.HasVertex3)
                {
                    b2Vec2 B2 = edgeA.Vertex3;
                    b2Vec2 A2 = B;
                    b2Vec2 e2 = B2 - A2;
                    diff = Q - A2;
                    float v2 = b2Math.b2Dot(ref e2, ref diff);

                    // Is the circle in Region AB of the next edge?
                    if (v2 > 0.0f)
                    {
                        return;
                    }
                }

                cf.indexA = 1;
                cf.typeA = b2ContactFeatureType.e_vertex;
                manifold.pointCount = 1;
                manifold.type = b2ManifoldType.e_circles;
                manifold.localNormal.SetZero();
                manifold.localPoint = P;
                manifold.points[0].id.key = 0;
                manifold.points[0].id.Set(cf);
                manifold.points[0].localPoint = circleB.Position;
                return;
            }

            // Region AB
            float den = e.LengthSquared; // b2Math.b2Dot(e, e);
            System.Diagnostics.Debug.Assert(den > 0.0f);
            b2Vec2 xP = (1.0f / den) * (u * A + v * B);
            b2Vec2 xd = Q - xP;
            float xdd = xd.LengthSquared; //  b2Math.b2Dot(xd, xd);
            if (xdd > radius * radius)
            {
                return;
            }

            b2Vec2 n = b2Vec2.Zero; // new b2Vec2(-e.y, e.x); 
            n.x = -e.y;
            n.y = e.x;
            diff = Q - A;
            if (b2Math.b2Dot(ref n, ref diff) < 0.0f)
            {
                // n.Set(-n.x, -n.y);
                n.Set(-n.x, -n.y);
            }
            n.Normalize();

            cf.indexA = 0;
            cf.typeA = b2ContactFeatureType.e_face;
            manifold.pointCount = 1;
            manifold.type = b2ManifoldType.e_faceA;
            manifold.localNormal = n;
            manifold.localPoint = A;
            manifold.points[0].id.key = 0;
            manifold.points[0].id.Set(cf);
            manifold.points[0].localPoint = circleB.Position;
        }

        /// Compute the collision manifold between an edge and a circle.
        public static void b2CollideEdgeAndPolygon(b2Manifold manifold, b2EdgeShape edgeA, ref b2Transform xfA, b2PolygonShape polygonB, ref b2Transform xfB)
        {
            b2EPCollider b = b2EPCollider.Create();
            b.Collide(manifold, edgeA, ref xfA, polygonB, ref xfB);
            b.Free();
        }

        /// Clipping for contact manifolds.
        public static int b2ClipSegmentToLine(b2ClipVertex[] vOut, b2ClipVertex[] vIn,
                                     ref b2Vec2 normal, float offset, byte vertexIndexA)
        {
            // Start with no output points
            int numOut = 0;

            var v0 = vIn[0].v;
            var v1 = vIn[1].v;

            // Calculate the distance of end points to the line
            float distance0 = normal.x * v0.x + normal.y * v0.y - offset;
            float distance1 = normal.x * v1.x + normal.y * v1.y - offset;

            // If the points are behind the plane
            if (distance0 <= 0.0f) vOut[numOut++] = vIn[0];
            if (distance1 <= 0.0f) vOut[numOut++] = vIn[1];

            // If the points are on different sides of the plane
            if (distance0 * distance1 < 0.0f)
            {
                // Find intersection point of edge and plane
                float interp = distance0 / (distance0 - distance1);

                b2ClipVertex o;
                o.v.x = v0.x + interp * (v1.x - v0.x);
                o.v.y = v0.y + interp * (v1.y - v0.y);

                // VertexA is hitting edgeB.
                o.id.indexA = vertexIndexA;
                o.id.indexB = vIn[0].id.indexB;
                o.id.typeA = b2ContactFeatureType.e_vertex;
                o.id.typeB = b2ContactFeatureType.e_face;

                vOut[numOut] = o;

                ++numOut;
            }

            return numOut;
        }

        public static bool b2TestOverlap(ref b2AABB a, ref b2AABB b)
        {
            // No operator overloading here - do direct computation to reduce time complexity
            if (b.LowerBound.x - a.UpperBound.x > 0.0f || b.LowerBound.y - a.UpperBound.y > 0.0f)
                return false;

            if (a.LowerBound.x - b.UpperBound.x > 0.0f || a.LowerBound.y - b.UpperBound.y > 0.0f)
                return false;

            return true;
        }

        /// Determine if two generic shapes overlap.
        public static bool b2TestOverlap(b2Shape shapeA, int indexA,
                             b2Shape shapeB, int indexB,
                            ref b2Transform xfA, ref b2Transform xfB)
        {
            b2DistanceInput input = b2DistanceInput.Create();
            input.proxyA = b2DistanceProxy.Create(shapeA, indexA);
            input.proxyB = b2DistanceProxy.Create(shapeB, indexB);
            input.transformA = xfA;
            input.transformB = xfB;
            input.useRadii = true;

            b2SimplexCache cache = b2SimplexCache.Create();

            b2DistanceOutput output;
            b2Simplex.b2Distance(out output, ref cache, ref input);

//            Console.WriteLine("{2} vs {3}: distance={0} after {1} iters", output.distance, output.iterations, shapeA.ShapeType, shapeB.ShapeType);

            return output.distance < 10.0f * b2Settings.b2_epsilon;
        }

        // Find the max separation between poly1 and poly2 using edge normals from poly1.
        public static float b2FindMaxSeparation(out int edgeIndex,
                                         b2PolygonShape poly1, ref b2Transform xf1,
                                         b2PolygonShape poly2, ref b2Transform xf2)
        {
            int count1 = poly1.VertexCount;
            b2Vec2[] normals1 = poly1.Normals;

            // Vector pointing from the centroid of poly1 to the centroid of poly2.
            //b2Vec2 d = b2Math.b2Mul(ref xf2, ref poly2.Centroid) - b2Math.b2Mul(ref xf1, ref poly1.Centroid);
            //b2Vec2 dLocal1 = b2Math.b2MulT(ref xf1.q, ref d);
            float dx = (xf2.q.c * poly2.Centroid.x - xf2.q.s * poly2.Centroid.y) + xf2.p.x - ((xf1.q.c * poly1.Centroid.x - xf1.q.s * poly1.Centroid.y) + xf1.p.x);
            float dy = (xf2.q.s * poly2.Centroid.x + xf2.q.c * poly2.Centroid.y) + xf2.p.y - ((xf1.q.s * poly1.Centroid.x + xf1.q.c * poly1.Centroid.y) + xf1.p.y);
            float dLocal1x = xf1.q.c * dx + xf1.q.s * dy;
            float dLocal1y = -xf1.q.s * dx + xf1.q.c * dy;

            // Find edge normal on poly1 that has the largest projection onto d.
            int edge = 0;
            float maxDot = -b2Settings.b2_maxFloat;
            for (int i = 0; i < count1; ++i)
            {
                var normal = normals1[i];
                float dot = normal.x * dLocal1x + normal.y * dLocal1y;
                if (dot > maxDot)
                {
                    maxDot = dot;
                    edge = i;
                }
            }

            // Get the separation for the edge normal.
            float s = b2EdgeSeparation(poly1, ref xf1, edge, poly2, ref xf2);

            // Check the separation for the previous edge normal.
            int prevEdge = edge - 1 >= 0 ? edge - 1 : count1 - 1;
            float sPrev = b2EdgeSeparation(poly1, ref xf1, prevEdge, poly2, ref xf2);

            // Check the separation for the next edge normal.
            int nextEdge = edge + 1 < count1 ? edge + 1 : 0;
            float sNext = b2EdgeSeparation(poly1, ref xf1, nextEdge, poly2, ref xf2);

            // Find the best edge and the search direction.
            int bestEdge;
            float bestSeparation;
            int increment;
            if (sPrev > s && sPrev > sNext)
            {
                increment = -1;
                bestEdge = prevEdge;
                bestSeparation = sPrev;
            }
            else if (sNext > s)
            {
                increment = 1;
                bestEdge = nextEdge;
                bestSeparation = sNext;
            }
            else
            {
                edgeIndex = edge;
                return s;
            }

            // Perform a local search for the best edge normal.
            for (; ; )
            {
                if (increment == -1)
                    edge = bestEdge - 1 >= 0 ? bestEdge - 1 : count1 - 1;
                else
                    edge = bestEdge + 1 < count1 ? bestEdge + 1 : 0;

                s = b2EdgeSeparation(poly1, ref xf1, edge, poly2, ref xf2);

                if (s > bestSeparation)
                {
                    bestEdge = edge;
                    bestSeparation = s;
                }
                else
                {
                    break;
                }
            }

            edgeIndex = bestEdge;
            return bestSeparation;
        }

        public static void b2FindIncidentEdge(b2ClipVertex[] c,
                                     b2PolygonShape poly1, ref b2Transform xf1, int edge1,
                                     b2PolygonShape poly2, ref b2Transform xf2)
        {
#if false
            b2Vec2[] normals1 = poly1.Normals;

            int count2 = poly2.VertexCount;
            b2Vec2[] vertices2 = poly2.Vertices;
            b2Vec2[] normals2 = poly2.Normals;

            // Get the normal of the reference edge in poly2's frame.
            b2Vec2 normal1 = b2Math.b2MulT(xf2.q, b2Math.b2Mul(xf1.q, normals1[edge1]));

            // Find the incident edge on poly2.
            int index = 0;
            float minDot = b2Settings.b2_maxFloat;
            for (int i = 0; i < count2; ++i)
            {
                float dot = b2Math.b2Dot(ref normal1, ref normals2[i]);
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            // Build the clip vertices for the incident edge.
            int i1 = index;
            int i2 = i1 + 1 < count2 ? i1 + 1 : 0;

            c[0].v = b2Math.b2Mul(ref xf2, ref vertices2[i1]);
            c[0].id.indexA = (byte)edge1;
            c[0].id.indexB = (byte)i1;
            c[0].id.typeA = b2ContactFeatureType.e_face;
            c[0].id.typeB = b2ContactFeatureType.e_vertex;

            c[1].v = b2Math.b2Mul(ref xf2, ref vertices2[i2]);
            c[1].id.indexA = (byte)edge1;
            c[1].id.indexB = (byte)i2;
            c[1].id.typeA = b2ContactFeatureType.e_face;
            c[1].id.typeB = b2ContactFeatureType.e_vertex;
#else
            var edge = poly1.Normals[edge1];

            int count2 = poly2.VertexCount;
            b2Vec2[] vertices2 = poly2.Vertices;
            b2Vec2[] normals2 = poly2.Normals;

            // Get the normal of the reference edge in poly2's frame.
            float bx = xf1.q.c * edge.x - xf1.q.s * edge.y;
            float by = xf1.q.s * edge.x + xf1.q.c * edge.y;

            float normal1x = xf2.q.c * bx + xf2.q.s * by;
            float normal1y = -xf2.q.s * bx + xf2.q.c * by;

            // Find the incident edge on poly2.
            int index = 0;
            float minDot = b2Settings.b2_maxFloat;
            for (int i = 0; i < count2; ++i)
            {
                var normal = normals2[i];
                float dot = normal1x * normal.x + normal1y * normal.y;
                if (dot < minDot)
                {
                    minDot = dot;
                    index = i;
                }
            }

            // Build the clip vertices for the incident edge.
            int i1 = index;
            int i2 = i1 + 1 < count2 ? i1 + 1 : 0;

            b2ClipVertex vertex;
            var vi1 = vertices2[i1];
            var vi2 = vertices2[i2];

            vertex.v.x = (xf2.q.c * vi1.x - xf2.q.s * vi1.y) + xf2.p.x;
            vertex.v.y = (xf2.q.s * vi1.x + xf2.q.c * vi1.y) + xf2.p.y;
            vertex.id.indexA = (byte)edge1;
            vertex.id.indexB = (byte)i1;
            vertex.id.typeA = b2ContactFeatureType.e_face;
            vertex.id.typeB = b2ContactFeatureType.e_vertex;

            c[0] = vertex;

            vertex.v.x = (xf2.q.c * vi2.x - xf2.q.s * vi2.y) + xf2.p.x;
            vertex.v.y = (xf2.q.s * vi2.x + xf2.q.c * vi2.y) + xf2.p.y;
            vertex.id.indexA = (byte)edge1;
            vertex.id.indexB = (byte)i2;
            vertex.id.typeA = b2ContactFeatureType.e_face;
            vertex.id.typeB = b2ContactFeatureType.e_vertex;

            c[1] = vertex;
#endif
        }
    }
}
