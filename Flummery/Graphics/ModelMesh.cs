﻿using System;
using System.Collections.Generic;

namespace Flummery
{
    public class ModelMesh
    {
        BoundingSphere BoundingSphere;
        List<ModelMeshPart> meshParts;
        string Name;
        ModelBone parent;
        object Tag;

        public ModelBone Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public ModelMesh()
        {
            meshParts = new List<ModelMeshPart>();
        }

        public void AddModelMeshPart(ModelMeshPart meshpart)
        {
            meshpart.Finalise();
            meshParts.Add(meshpart);
        }

        public void Draw()
        {
            foreach (ModelMeshPart meshpart in meshParts)
            {
                meshpart.Draw();
            }
        }
    }
}
