﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Flummery
{
    public class ModelMeshPart
    {
        IndexBuffer indexBuffer;
        int PrimitiveCount;
        int StartIndex;
        object Tag;
        VertexBuffer vertexBuffer;
        int VertexOffset;
        Material material;

        PrimitiveType primitiveType = PrimitiveType.TriangleStrip;
        FrontFaceDirection windingOrder = FrontFaceDirection.Ccw;

        public IndexBuffer IndexBuffer { get { return indexBuffer; } }
        public VertexBuffer VertexBuffer { get { return vertexBuffer; } }

        public PrimitiveType PrimitiveType
        {
            get { return primitiveType; }
            set { primitiveType = value; }
        }

        public FrontFaceDirection WindingOrder
        {
            get { return windingOrder; }
            set { windingOrder = value; }
        }

        public int VertexCount { get { return vertexBuffer.Length; } }

        public Material Material
        {
            get { return material; }
            set { material = value; }
        }

        public ModelMeshPart()
        {
            vertexBuffer = new VertexBuffer();
            indexBuffer = new IndexBuffer();
        }

        public void AddVertex(Vector3 position, Vector3 normal, Vector2 texcoords)
        {
            var v = new Vertex();
            v.Position = position;
            v.Normal = normal;
            v.UV = texcoords;

            int index = vertexBuffer.Data.FindIndex(vert =>
                vert.Position.X.GetHashCode() == v.Position.X.GetHashCode() &&
                vert.Position.Y.GetHashCode() == v.Position.Y.GetHashCode() &&
                vert.Position.Z.GetHashCode() == v.Position.Z.GetHashCode() &&
                vert.Normal.X.GetHashCode() == v.Normal.X.GetHashCode() &&
                vert.Normal.Y.GetHashCode() == v.Normal.Y.GetHashCode() &&
                vert.Normal.Z.GetHashCode() == v.Normal.Z.GetHashCode() &&
                vert.UV.X.GetHashCode() == v.UV.X.GetHashCode() &&
                vert.UV.Y.GetHashCode() == v.UV.Y.GetHashCode()
            );

            if (index == -1)
            {
                vertexBuffer.AddVertex(v);
                indexBuffer.AddIndex(vertexBuffer.Length - 1);
            }
            else
            {
                indexBuffer.AddIndex(index);
            }
        }

        public void AddFace(Vector3[] positions, Vector3[] normals, Vector2[] texcoords)
        {
            for (int i = 0; i < 3; i++)
            {
                var v = new Vertex();
                v.Position = positions[i];
                v.Normal = normals[i];
                v.UV = texcoords[i];

                int index = vertexBuffer.Data.FindIndex(vert =>
                    vert.Position.X.GetHashCode() == v.Position.X.GetHashCode() &&
                    vert.Position.Y.GetHashCode() == v.Position.Y.GetHashCode() &&
                    vert.Position.Z.GetHashCode() == v.Position.Z.GetHashCode() &&
                    vert.Normal.X.GetHashCode() == v.Normal.X.GetHashCode() &&
                    vert.Normal.Y.GetHashCode() == v.Normal.Y.GetHashCode() &&
                    vert.Normal.Z.GetHashCode() == v.Normal.Z.GetHashCode() &&
                    vert.UV.X.GetHashCode() == v.UV.X.GetHashCode() &&
                    vert.UV.Y.GetHashCode() == v.UV.Y.GetHashCode()
                );

                if (index == -1)
                {
                    vertexBuffer.AddVertex(v);
                    indexBuffer.AddIndex(vertexBuffer.Length - 1);
                }
                else
                {
                    indexBuffer.AddIndex(index);
                }
            }
        }

        public void Finalise()
        {
            if (SceneManager.Current.CanUseVertexBuffer)
            {
                indexBuffer.Initialise();
                vertexBuffer.Initialise();
            }
        }

        public void Draw()
        {
            var data = indexBuffer.Data;

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, (material != null && material.Texture != null ? material.Texture.ID : 0));

            GL.FrontFace(windingOrder);

            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            if (SceneManager.Current.CanUseVertexBuffer)
            {
                switch (SceneManager.Current.RenderMode)
                {
                    case SceneManager.RenderMeshMode.Solid:
                        GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                        break;

                    case SceneManager.RenderMeshMode.Wireframe:
                        GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                        break;

                    case SceneManager.RenderMeshMode.SolidWireframe:
                        GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                        vertexBuffer.Draw(indexBuffer, primitiveType);
                        GL.PolygonOffset(1.0f, 2);
                        GL.Disable(EnableCap.Texture2D);
                        GL.Color4(Color.White);
                        GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                        break;
                }

                vertexBuffer.Draw(indexBuffer, primitiveType);
            }
            else
            {
                GL.Begin(primitiveType);

                foreach (int i in indexBuffer.Data)
                {
                    var v = vertexBuffer.Data[i];

                    GL.TexCoord2(v.UV);
                    GL.Normal3(v.Normal);
                    GL.Vertex3(v.Position);
                }

                GL.End();
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);

            // Visualise normals
            //GL.Begin(BeginMode.Lines);

            //foreach (int i in indexBuffer.Data)
            //{
            //    var v = vertexBuffer.Data[i];

            //    GL.LineWidth(2f);
            //    GL.Color3(Color.White);

            //    GL.Vertex3(v.Position);
            //    GL.Vertex3(v.Position + (v.Normal * 0.01f));
            //}

            //GL.End();
        }
    }
}
