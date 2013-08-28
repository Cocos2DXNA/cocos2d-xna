﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Box2D.Common;

namespace Box2D.Collision.Shapes
{
    public class b2PolygonShape : b2Shape
    {
        public b2Vec2 Centroid;
        public b2Vec2[] Vertices = new b2Vec2[b2Settings.b2_maxPolygonVertices];
        public b2Vec2[] Normals = new b2Vec2[b2Settings.b2_maxPolygonVertices];
        internal int m_vertexCount;

        public b2PolygonShape()
        {
            ShapeType = b2ShapeType.e_polygon;
            Radius = b2Settings.b2_polygonRadius;
            m_vertexCount = 0;
            Centroid.SetZero();
        }

        /// Get the vertex count.
        public int VertexCount
        {
            get
            {
                return m_vertexCount;
            }
        }

        public b2PolygonShape(b2PolygonShape copy)
            : base((b2Shape)copy)
        {
            Centroid = copy.Centroid;
            Array.Copy(copy.Vertices, Vertices, copy.Vertices.Length);
            Array.Copy(copy.Normals, Normals, copy.Normals.Length);
            m_vertexCount = copy.m_vertexCount;
        }

        public override b2Shape Clone()
        {
            b2PolygonShape clone = new b2PolygonShape(this);
            return clone;
        }

        public void SetAsBox(float hx, float hy)
        {
            m_vertexCount = 4;
            Vertices[0].Set(-hx, -hy);
            Vertices[1].Set(hx, -hy);
            Vertices[2].Set(hx, hy);
            Vertices[3].Set(-hx, hy);
            Normals[0].Set(0.0f, -1.0f);
            Normals[1].Set(1.0f, 0.0f);
            Normals[2].Set(0.0f, 1.0f);
            Normals[3].Set(-1.0f, 0.0f);
            Centroid.SetZero();
        }

        public void SetAsBox(float hx, float hy, b2Vec2 center, float angle)
        {
            m_vertexCount = 4;
            Vertices[0].Set(-hx, -hy);
            Vertices[1].Set(hx, -hy);
            Vertices[2].Set(hx, hy);
            Vertices[3].Set(-hx, hy);
            Normals[0].Set(0.0f, -1.0f);
            Normals[1].Set(1.0f, 0.0f);
            Normals[2].Set(0.0f, 1.0f);
            Normals[3].Set(-1.0f, 0.0f);
            Centroid = center;

            b2Transform xf = b2Transform.Identity;
            xf.p = center;
            xf.q.Set(angle);

            // Transform vertices and normals.
            for (int i = 0; i < m_vertexCount; ++i)
            {
                Vertices[i] = b2Math.b2Mul(ref xf, ref Vertices[i]);
                Normals[i] = b2Math.b2Mul(ref xf.q, ref Normals[i]);
            }
        }

        public override int GetChildCount()
        {
            return 1;
        }

        public static b2Vec2 ComputeCentroid(b2Vec2[] vs, int count)
        {
            b2Vec2 c = new b2Vec2();
            c.Set(0.0f, 0.0f);
            float area = 0.0f;

            // pRef is the reference point for forming triangles.
            // It's location doesn't change the result (except for rounding error).
            b2Vec2 pRef = new b2Vec2(0.0f, 0.0f);

            float inv3 = 1.0f / 3.0f;

            for (int i = 0; i < count; ++i)
            {
                // Triangle vertices.
                b2Vec2 p1 = pRef;
                b2Vec2 p2 = vs[i];
                b2Vec2 p3 = i + 1 < count ? vs[i + 1] : vs[0];

                b2Vec2 e1 = p2 - p1;
                b2Vec2 e2 = p3 - p1;

                float D = b2Math.b2Cross(ref e1, ref e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                c += triangleArea * inv3 * (p1 + p2 + p3);
            }

            // Centroid
            if (area <= b2Settings.b2_epsilon)
            {
#if NETFX_CORE
                throw (new OverflowException("Centroid is not defined, area is zero."));
#else
                throw (new NotFiniteNumberException("Centroid is not defined, area is zero."));
#endif
            }
            c *= 1.0f / area;
            return c;
        }

        public virtual void Set(b2Vec2[] vertices, int count)
        {
            m_vertexCount = count;

            // Copy vertices.
            for (int i = 0; i < m_vertexCount; ++i)
            {
                Vertices[i] = vertices[i];
            }

            // Compute normals. Ensure the edges have non-zero length.
            for (int i = 0; i < m_vertexCount; ++i)
            {
                int i1 = i;
                int i2 = i + 1 < m_vertexCount ? i + 1 : 0;
                b2Vec2 edge = Vertices[i2] - Vertices[i1];
                 Debug.Assert(edge.LengthSquared > b2Settings.b2_epsilon * b2Settings.b2_epsilon);
                 Normals[i] = edge.UnitCross(); // b2Math.b2Cross(edge, 1.0f);
                Normals[i].Normalize();
            }

#if DEBUG
            // Ensure the polygon is convex and the interior
            // is to the left of each edge.
            for (int i = 0; i < m_vertexCount; ++i)
            {
                int i1 = i;
                int i2 = i + 1 < m_vertexCount ? i + 1 : 0;
                b2Vec2 edge = Vertices[i2] -Vertices[i1];

                for (int j = 0; j < m_vertexCount; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i1 || j == i2)
                    {
                        continue;
                    }

                    b2Vec2 r = Vertices[j] - Vertices[i1];

                    // If this crashes, your polygon is non-convex, has colinear edges,
                    // or the winding order is wrong.
                    float s = b2Math.b2Cross(edge, r);
                    if (s < 0f)
                    {
                        throw (new InvalidOperationException("ERROR: Please ensure your polygon is convex and has a CCW winding order"));
                    }
                }
            }
#endif

            // Compute the polygon centroid.
            Centroid = ComputeCentroid(Vertices, m_vertexCount);
        }

        public override bool TestPoint(ref b2Transform xf, b2Vec2 p)
        {
            b2Vec2 pLocal = b2Math.b2MulT(xf.q, p - xf.p);

            for (int i = 0; i < m_vertexCount; ++i)
            {
                float dot = b2Math.b2Dot(Normals[i], pLocal - Vertices[i]);
                if (dot > 0.0f)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool RayCast(out b2RayCastOutput output, b2RayCastInput input, ref b2Transform xf, int childIndex)
        {
            output = b2RayCastOutput.Zero;
            // Put the ray into the polygon's frame of reference.
            b2Vec2 p1 = b2Math.b2MulT(xf.q, input.p1 - xf.p);
            b2Vec2 p2 = b2Math.b2MulT(xf.q, input.p2 - xf.p);
            b2Vec2 d = p2 - p1;

            float lower = 0.0f, upper = input.maxFraction;

            int index = -1;

            for (int i = 0; i < m_vertexCount; ++i)
            {
                // p = p1 + a * d
                // dot(normal, p - v) = 0
                // dot(normal, p1 - v) + a * dot(normal, d) = 0
                float numerator = b2Math.b2Dot(Normals[i], Vertices[i] - p1);
                float denominator = b2Math.b2Dot(Normals[i], d);

                if (denominator == 0.0f)
                {
                    if (numerator < 0.0f)
                    {
                        return false;
                    }
                }
                else
                {
                    // Note: we want this predicate without division:
                    // lower < numerator / denominator, where denominator < 0
                    // Since denominator < 0, we have to flip the inequality:
                    // lower < numerator / denominator <==> denominator * lower > numerator.
                    if (denominator < 0.0f && numerator < lower * denominator)
                    {
                        // Increase lower.
                        // The segment enters this half-space.
                        lower = numerator / denominator;
                        index = i;
                    }
                    else if (denominator > 0.0f && numerator < upper * denominator)
                    {
                        // Decrease upper.
                        // The segment exits this half-space.
                        upper = numerator / denominator;
                    }
                }

                // The use of epsilon here causes the assert on lower to trip
                // in some cases. Apparently the use of epsilon was to make edge
                // shapes work, but now those are handled separately.
                //if (upper < lower - b2_epsilon)
                if (upper < lower)
                {
                    return false;
                }
            }

            //    Debug.Assert(0.0f <= lower && lower <= input.maxFraction);

            if (index >= 0)
            {
                output.fraction = lower;
                output.normal = b2Math.b2Mul(xf.q, Normals[index]);
                return true;
            }

            return false;
        }

        public override void ComputeAABB(out b2AABB output, ref b2Transform xf, int childIndex)
        {
#if false
            b2Vec2 lower = b2Math.b2Mul(ref xf, ref Vertices[0]);
            b2Vec2 upper = lower;

            for (int i = 1; i < m_vertexCount; ++i)
            {
                b2Vec2 v = b2Math.b2Mul(ref xf, ref Vertices[i]);
                lower = b2Math.b2Min(lower, v);
                upper = b2Math.b2Max(upper, v);
            }

            b2AABB aabb = b2AABB.Default;
            aabb.Set(lower, upper);
            aabb.Fatten(Radius);
            return(aabb);
#else
            var vert = Vertices[0];
            b2Vec2 lower;
            lower.x = (xf.q.c * vert.x - xf.q.s * vert.y) + xf.p.x;
            lower.y = (xf.q.s * vert.x + xf.q.c * vert.y) + xf.p.y;
            b2Vec2 upper = lower;

            for (int i = 1; i < m_vertexCount; ++i)
            {
                var vetr2 = Vertices[i];
                b2Vec2 v;
                v.x = (xf.q.c * vetr2.x - xf.q.s * vetr2.y) + xf.p.x;
                v.y = (xf.q.s * vetr2.x + xf.q.c * vetr2.y) + xf.p.y;

                lower.x = Math.Min(lower.x, v.x);
                lower.y = Math.Min(lower.y, v.y);
                upper.x = Math.Max(upper.x, v.x);
                upper.y = Math.Max(upper.y, v.y);
            }

            output.LowerBound = lower;
            output.UpperBound = upper;
            output.Fatten(Radius);
#endif
        }

        public override b2MassData ComputeMass(float density)
        {
            // Polygon mass, centroid, and inertia.
            // Let rho be the polygon density in mass per unit area.
            // Then:
            // mass = rho * int(dA)
            // centroid.x = (1/mass) * rho * int(x * dA)
            // centroid.y = (1/mass) * rho * int(y * dA)
            // I = rho * int((x*x + y*y) * dA)
            //
            // We can compute these integrals by summing all the integrals
            // for each triangle of the polygon. To evaluate the integral
            // for a single triangle, we make a change of variables to
            // the (u,v) coordinates of the triangle:
            // x = x0 + e1x * u + e2x * v
            // y = y0 + e1y * u + e2y * v
            // where 0 <= u && 0 <= v && u + v <= 1.
            //
            // We integrate u from [0,1-v] and then v from [0,1].
            // We also need to use the Jacobian of the transformation:
            // D = cross(e1, e2)
            //
            // Simplification: triangle centroid = (1/3) * (p1 + p2 + p3)
            //
            // The rest of the derivation is handled by computer algebra.

            b2Vec2 center = new b2Vec2();
            center.Set(0.0f, 0.0f);
            float area = 0.0f;
            float I = 0.0f;

            // s is the reference point for forming triangles.
            // It's location doesn't change the result (except for rounding error).
            b2Vec2 s = new b2Vec2(0.0f, 0.0f);

            // This code would put the reference point inside the polygon.
            for (int i = 0; i < m_vertexCount; ++i)
            {
                s += Vertices[i];
            }
            s *= 1.0f / m_vertexCount;

            float k_inv3 = 1.0f / 3.0f;

            for (int i = 0; i < m_vertexCount; ++i)
            {
                // Triangle vertices.
                b2Vec2 e1 = Vertices[i] - s;
                b2Vec2 e2 = i + 1 < m_vertexCount ? Vertices[i + 1] - s : Vertices[0] - s;

                float D = b2Math.b2Cross(ref e1, ref e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                center += triangleArea * k_inv3 * (e1 + e2);

                float ex1 = e1.x, ey1 = e1.y;
                float ex2 = e2.x, ey2 = e2.y;

                float intx2 = ex1 * ex1 + ex2 * ex1 + ex2 * ex2;
                float inty2 = ey1 * ey1 + ey2 * ey1 + ey2 * ey2;

                I += (0.25f * k_inv3 * D) * (intx2 + inty2);
            }

            // Total mass
            b2MassData massData = new b2MassData();

            massData.mass = density * area;

            // Center of mass
            if (area <= b2Settings.b2_epsilon)
            {
#if NETFX_CORE
                throw (new OverflowException("Area is zero in mass calculation."));
#else
                throw (new NotFiniteNumberException("Area is zero in mass calculation."));
#endif
            }
            center *= 1.0f / area;
            massData.center = center + s;

            // Inertia tensor relative to the local origin (point s).
            massData.I = density * I;

            // Shift to center of mass then to original body origin.
            massData.I += massData.mass * (b2Math.b2Dot(ref massData.center, ref massData.center) - b2Math.b2Dot(ref center, ref center));
            return (massData);
        }
    }
}
