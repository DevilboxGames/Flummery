﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using OpenTK;

using ToxicRagers.Stainless.Formats;

namespace Flummery.ContentPipeline.Stainless
{
    class MDLImporter : ContentImporter
    {
        public override string GetExtension() { return "mdl"; }

        public override Asset Import(string path)
        {
            MDL mdl = MDL.Load(path);
            Model model = new Model();
            ModelMesh mesh = new ModelMesh();

            // 2015-07-12 : Commenting out SupportingDocuments["Source"] to see if anything breaks
            // model.SupportingDocuments["Source"] = mdl;

            bool bUsePrepData = true;

            for (int i = 0; i < mdl.Meshes.Count; i++)
            {
                Dictionary<int, int> newIndex = new Dictionary<int, int>();
                ModelMeshPart meshpart = new ModelMeshPart();

                var mdlmesh = mdl.GetMesh(i);

                meshpart.Material = SceneManager.Current.Content.Load<Material, MaterialImporter>(mdlmesh.Name, Path.GetDirectoryName(path), true);

                if (bUsePrepData)
                {
                    foreach (var f in mdl.Faces.Where(f => f.MaterialID == i))
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (!newIndex.ContainsKey(f.Verts[j]))
                            {
                                var v = mdl.Vertices[f.Verts[j]];
                                int index = meshpart.AddVertex(new Vector3(v.Position.X, v.Position.Y, v.Position.Z), new Vector3(v.Normal.X, v.Normal.Y, v.Normal.Z), new Vector2(v.UV.X, v.UV.Y), new Vector2(v.UV2.X, v.UV2.Y), v.Colour, false);
                                newIndex.Add(f.Verts[j], index);
                            }
                        }

                        meshpart.AddFace(
                            newIndex[f.Verts[0]],
                            newIndex[f.Verts[1]],
                            newIndex[f.Verts[2]]
                        );
                    }
                }
                else
                {
                    int[] verts = new int[3];

                    for (int j = 0; j < mdlmesh.StripList.Count - 2; j++)
                    {
                        if (mdlmesh.StripList[j + 2].Degenerate) { continue; }

                        verts[0] = mdlmesh.StripList[j + 0].Index;

                        if (j % 2 == 0)
                        {
                            verts[1] = mdlmesh.StripList[j + 1].Index;
                            verts[2] = mdlmesh.StripList[j + 2].Index;
                        }
                        else
                        {
                            verts[1] = mdlmesh.StripList[j + 2].Index;
                            verts[2] = mdlmesh.StripList[j + 1].Index;
                        }

                        for (int k = 0; k < 3; k++)
                        {
                            if (!newIndex.ContainsKey(verts[k]))
                            {
                                var v = mdl.Vertices[verts[k]];
                                int index = meshpart.AddVertex(new Vector3(v.Position.X, v.Position.Y, v.Position.Z), new Vector3(v.Normal.X, v.Normal.Y, v.Normal.Z), new Vector2(v.UV.X, v.UV.Y), new Vector2(v.UV2.X, v.UV2.Y), v.Colour, false);
                                newIndex.Add(verts[k], index);
                            }
                        }

                        meshpart.AddFace(
                            newIndex[verts[0]],
                            newIndex[verts[1]],
                            newIndex[verts[2]]
                        );
                    }

                    // Process patch list
                    for (int j = 0; j < mdlmesh.TriList.Count; j += 3)
                    {
                        verts[0] = mdlmesh.TriList[j + 0].Index;
                        verts[1] = mdlmesh.TriList[j + 1].Index;
                        verts[2] = mdlmesh.TriList[j + 2].Index;

                        for (int k = 0; k < 3; k++)
                        {
                            if (!newIndex.ContainsKey(verts[k]))
                            {
                                var v = mdl.Vertices[verts[k]];
                                int index = meshpart.AddVertex(new Vector3(v.Position.X, v.Position.Y, v.Position.Z), new Vector3(v.Normal.X, v.Normal.Y, v.Normal.Z), new Vector2(v.UV.X, v.UV.Y), new Vector2(v.UV2.X, v.UV2.Y), v.Colour, false);
                                newIndex.Add(verts[k], index);
                            }
                        }

                        meshpart.AddFace(
                            newIndex[verts[0]],
                            newIndex[verts[1]],
                            newIndex[verts[2]]
                        );
                    }
                }

                mesh.AddModelMeshPart(meshpart);

                Console.WriteLine(meshpart.VertexCount / 3);
            }

            mesh.Name = mdl.Name;
            model.SetName(mdl.Name, model.AddMesh(mesh));

            return model;
        }
    }
}
