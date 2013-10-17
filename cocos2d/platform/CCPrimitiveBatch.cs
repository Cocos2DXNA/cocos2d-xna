using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cocos2D
{
    public class CCPrimitiveBatch
    {
        private const int DefaultBufferSize = 500;

        private readonly Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture[] _lineVertices;
        private readonly Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture[] _triangleVertices;

        // hasBegun is flipped to true once Begin is called, and is used to make
        // sure users don't call End before Begin is called.
        private bool _hasBegun;

        private int _lineVertsCount;
        private int _triangleVertsCount;

        /// <summary>
        /// the constructor creates a new PrimitiveBatch and sets up all of the internals
        /// that PrimitiveBatch will need.
        /// </summary>
        public CCPrimitiveBatch()
            : this(DefaultBufferSize)
        {
        }

        public CCPrimitiveBatch(int bufferSize)
            {
            _triangleVertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture[bufferSize - bufferSize % 3];
            _lineVertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture[bufferSize - bufferSize % 2];
        }

        /// <summary>
        /// Begin is called to tell the PrimitiveBatch what kind of primitives will be
        /// drawn, and to prepare the graphics card to render those primitives.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="view">The view.</param>
        public void Begin()
        {
            if (_hasBegun)
            {
                throw new InvalidOperationException("End must be called before Begin can be called again.");
            }

            // flip the error checking boolean. It's now ok to call AddVertex, Flush,
            // and End.
            _hasBegun = true;
        }

        public bool IsReady()
        {
            return _hasBegun;
        }

        public void AddVertex(Vector2 vertex, Color color, PrimitiveType primitiveType)
        {
            AddVertex(ref vertex, color, primitiveType);
        }

        public void AddVertex(ref Vector2 vertex, Color color, PrimitiveType primitiveType)
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before AddVertex can be called.");
            }

            if (primitiveType == PrimitiveType.LineStrip || primitiveType == PrimitiveType.TriangleStrip)
            {
                throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
            }

            if (primitiveType == PrimitiveType.TriangleList)
            {
                if (_triangleVertsCount >= _triangleVertices.Length)
                {
                    FlushTriangles();
                }
                _triangleVertices[_triangleVertsCount].Position = new Vector3(vertex, -0.1f);
                _triangleVertices[_triangleVertsCount].Color = color;
                _triangleVertsCount++;
            }

            if (primitiveType == PrimitiveType.LineList)
            {
                if (_lineVertsCount >= _lineVertices.Length)
                {
                    FlushLines();
                }
                _lineVertices[_lineVertsCount].Position = new Vector3(vertex, 0f);
                _lineVertices[_lineVertsCount].Color = color;
                _lineVertsCount++;
            }
        }


        /// <summary>
        /// End is called once all the primitives have been drawn using AddVertex.
        /// it will call Flush to actually submit the draw call to the graphics card, and
        /// then tell the basic effect to end.
        /// </summary>
        public void End()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before End can be called.");
            }

            // Draw whatever the user wanted us to draw
            FlushTriangles();
            FlushLines();

            _hasBegun = false;
        }

        private void FlushTriangles()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }

            if (_triangleVertsCount >= 3)
            {
                int primitiveCount = _triangleVertsCount / 3;

                // submit the draw call to the graphics card
                CCDrawManager.TextureEnabled = false;
                CCDrawManager.DrawPrimitives(PrimitiveType.TriangleList, _triangleVertices, 0, primitiveCount);

                _triangleVertsCount -= primitiveCount * 3;
            }
        }

        private void FlushLines()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }

            if (_lineVertsCount >= 2)
            {
                int primitiveCount = _lineVertsCount / 2;

                // submit the draw call to the graphics card
                CCDrawManager.TextureEnabled = false;
                CCDrawManager.DrawPrimitives(PrimitiveType.LineList, _lineVertices, 0, primitiveCount);

                _lineVertsCount -= primitiveCount * 2;
            }
        }
    }
}