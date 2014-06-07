﻿using System;
using System.Collections.Generic;
using System.IO;

using Flummery.ContentPipeline.Core;
using ToxicRagers.Carmageddon2.Formats;
using ToxicRagers.CarmageddonReincarnation.Formats;
using ToxicRagers.Helpers;
using ToxicRagers.Stainless.Formats;
using OpenTK;

namespace Flummery.ContentPipeline.Stainless
{
    class MaterialImporter : ContentImporter
    {
        public override string GetExtension() { return "mt2;mtl;mat"; }

        public override string GetHints(string currentPath)
        {
            string hints = (currentPath != null ? currentPath + ";" : "");

            if (Properties.Settings.Default.PathCarmageddonReincarnation != null)
            {
                if (Directory.Exists(Properties.Settings.Default.PathCarmageddonReincarnation + "Data_Core\\Content\\Textures\\")) { hints = Properties.Settings.Default.PathCarmageddonReincarnation + "Data_Core\\Content\\Textures\\"; }

                return hints;
            }

            return null;
        }

        public override Asset Import(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            ToxicRagers.Helpers.Material m = null;

            switch (Path.GetExtension(path).ToLower())
            {
                case ".mt2":
                    m = MT2.Load(path);
                    break;

                case ".mtl":
                    m = MTL.Load(path);
                    break;
            }

            if (m != null)
            {
                var mat = (m as MT2);
                string fileName = (mat != null ? mat.DiffuseColour : (m as MTL).Textures[0]);

                if (fileName == null || fileName == "")
                {
                    return new Material() { Name = name, Texture = new Texture() { Name = fileName } };
                }
                else
                {
                    path = path.Substring(0, path.LastIndexOf("\\") + 1);
                    return new Material() { Name = name, Texture = SceneManager.Current.Content.Load<Texture, TDXImporter>(fileName, path) };
                }
            }
            else
            {
                return new Material();
            }
        }

        public override AssetList ImportMany(string path)
        {
            MaterialList materials = new MaterialList();
            MAT mat = MAT.Load(path);

            foreach (var material in mat.Materials)
            {
                materials.Entries.Add(
                    new Material
                    {
                        Name = material.Name,
                        Texture = SceneManager.Current.Content.Load<Texture, TIFImporter>(material.Texture, Path.GetDirectoryName(path))
                    }
                );
            }

            return materials;
        }
    }
}
