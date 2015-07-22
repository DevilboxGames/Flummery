﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Flummery.ContentPipeline.Core;
using Flummery.ContentPipeline.Stainless;
using Flummery.Util;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using ToxicRagers.Carmageddon2.Formats;

using WeifenLuo.WinFormsUI.Docking;

namespace Flummery
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        public DockPanel DockPanel { get { return dockPanel; } }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text += " v" + FlummeryApplication.Version;

            var inputManager = new InputManager();

            var overview = new pnlOverview();
            var viewport = new pnlViewport();
            var materials = new pnlMaterialList();
            var details = new pnlDetails();

            viewport.Show(dockPanel, DockState.Document);
            materials.Show(dockPanel, DockState.DockBottom);
            overview.Show(dockPanel, DockState.DockLeft);
            details.Show(dockPanel, DockState.DockRight);

            var extensions = new List<string>(GL.GetString(StringName.Extensions).Split(' '));
            SceneManager.Create(extensions.Contains("GL_ARB_vertex_buffer_object"));

            dockPanel.DockLeftPortion = 300;
            dockPanel.DockRightPortion = 240;
            dockPanel.DockBottomPortion = 105;

            viewport.RegisterEventHandlers();
            overview.RegisterEventHandlers();
            materials.RegisterEventHandlers();
            details.RegisterEventHandlers();

            ToxicRagers.Helpers.Logger.ResetLog();

            SetActionScalingText("Action Scaling: 1.000");

            this.KeyPreview = true;
            this.KeyPress += new KeyPressEventHandler(frmMain_KeyPress);

            SceneManager.Current.OnProgress += scene_OnProgress;
            SceneManager.Current.OnError += scene_OnError;
            SceneManager.Current.SetCoordinateSystem(SceneManager.CoordinateSystem.LeftHanded);

            if (Properties.Settings.Default.CheckForUpdates) { checkUpdate(); }

            FlummeryApplication.UI = this;
        }

        public void checkUpdate()
        {
            new Updater().Check(FlummeryApplication.Version, finishRequest);
        }

        private void finishRequest(bool result, Updater.Update[] updates)
        {
            if (result == true && updates.Count() > 0)
            {
                frmUpdater updateForm = new frmUpdater();
                updateForm.Updates = updates;
                updateForm.ShowDialog();
            }
        }

        void scene_OnProgress(object sender, ProgressEventArgs e)
        {
            tsslProgress.Text = e.Status;
            tsslProgress.Owner.Refresh();
            Application.DoEvents();
        }

        void scene_OnError(object sender, ErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        void frmMain_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            e.Handled = InputManager.Current.HandleInput(sender, e);
        }

        private void menuClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "&New":
                    SceneManager.Current.Reset();
                    break;

                case "E&xit":
                    Application.Exit();
                    break;

                case "About Flummery":
                    (new frmAbout()).ShowDialog();
                    break;
            }
        }

        private void menuImportClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "Autodesk FBX File...":
                    ofdBrowse.Filter = "Autodesk FBX files (*.fbx)|*.fbx";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.SetCoordinateSystem(SceneManager.CoordinateSystem.LeftHanded);
                        var m = SceneManager.Current.Content.Load<Model, ContentPipeline.Core.FBXImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);
                        //if (m.Root.Mesh != null) { ModelManipulator.FlipAxis(m.Root.Mesh, Axis.Z, true); }

                        SceneManager.Current.UpdateProgress(string.Format("Imported {0}", Path.GetFileName(ofdBrowse.FileName)));
                    }
                    break;

                case "BRender ACT File...":
                    ofdBrowse.Filter = "BRender ACT files (*.act)|*.act";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, ACTImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);

                        SceneManager.Current.UpdateProgress(string.Format("Imported {0}", Path.GetFileName(ofdBrowse.FileName)));
                    }
                    break;

                case "BRender DAT File...":
                    ofdBrowse.Filter = "BRender DAT files (*.dat)|*.dat";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, DATImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);

                        SceneManager.Current.UpdateProgress(string.Format("Imported {0}", Path.GetFileName(ofdBrowse.FileName)));
                    }
                    break;

                case "Stainless CNT File...":
                    ofdBrowse.Filter = "Stainless CNT files (*.cnt)|*.cnt";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, CNTImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);

                        SceneManager.Current.UpdateProgress(string.Format("Imported {0}", Path.GetFileName(ofdBrowse.FileName)));
                    }
                    break;

                case "Stainless MDL File...":
                    ofdBrowse.Filter = "Stainless MDL files (*.mdl)|*.mdl";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, MDLImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);

                        SceneManager.Current.UpdateProgress(string.Format("Imported {0}", Path.GetFileName(ofdBrowse.FileName)));
                    }
                    break;

                case "Stainless LIGHT File...":
                    ofdBrowse.Filter = "Stainless LIGHT files (*.light)|*.light";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, LIGHTImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);

                        SceneManager.Current.UpdateProgress(string.Format("Imported {0}", Path.GetFileName(ofdBrowse.FileName)));
                    }
                    break;

                case "Torus MSHS File...":
                    ofdBrowse.Filter = "Torus MSHS files (*.mshs)|*.mshs";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, ContentPipeline.TDR2000.MSHSImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);

                        SceneManager.Current.UpdateProgress(string.Format("Imported {0}", Path.GetFileName(ofdBrowse.FileName)));
                    }
                    break;
            }
        }

        private void menuExportClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "Autodesk FBX File...":
                    sfdBrowse.Filter = "Autodesk FBX files (*.fbx)|*.fbx";
                    if (sfdBrowse.ShowDialog() == DialogResult.OK)
                    {
                        var fx = new ContentPipeline.Core.FBXExporter();
                        fx.Export(SceneManager.Current.Models[0], sfdBrowse.FileName);
                        SceneManager.Current.UpdateProgress(string.Format("Saved {0}", Path.GetFileName(sfdBrowse.FileName)));
                    }
                    break;

                case "Stainless CNT File...":
                    sfdBrowse.Filter = "Stainless CNT files (*.cnt)|*.cnt";
                    if (sfdBrowse.ShowDialog() == DialogResult.OK)
                    {
                        var cx = new CNTExporter();
                        cx.Export(SceneManager.Current.Models[0], sfdBrowse.FileName);
                    }
                    break;
            }
        }

        private void menuCarmageddonClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            SceneManager.Current.SetCoordinateSystem(SceneManager.CoordinateSystem.RightHanded);

            switch (mi.Text)
            {
                case "Actor":
                    ofdBrowse.Filter = "Carmageddon ACTOR (*.act)|*.act";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, ACTImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);
                    }
                    break;

                case "Car":
                    ofdBrowse.Filter = "Carmageddon CAR (*.txt)|*.txt";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, C1CarImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);
                    }
                    break;

                case "TXT files (C1 Car)":
                    processAll(mi.Text);
                    break;
            }
        }

        private void menuCarmageddon2Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            SceneManager.Current.SetCoordinateSystem(SceneManager.CoordinateSystem.RightHanded);

            switch (mi.Text)
            {
                case "Actor":
                    ofdBrowse.Filter = "Carmageddon 2 ACTOR (*.act)|*.act";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Content.Load<Model, ACTImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);
                    }
                    break;

                case "Process for Carmageddon Reincarnation":
                    if (SceneManager.Current.Models.Count == 0) { return; }

                    var bones = SceneManager.Current.Models[0].Bones[0].AllChildren();

                    SceneManager.Current.UpdateProgress("Applying Carmageddon Reincarnation scale");

                    ModelManipulator.Scale(bones, Matrix4.CreateScale(6.9f, 6.9f, -6.9f), true);
                    ModelManipulator.FlipFaces(bones, true);

                    SceneManager.Current.UpdateProgress("Fixing material names");

                    foreach (var material in SceneManager.Current.Materials)
                    {
                        if (material.Name.Contains(".")) { material.Name = material.Name.Substring(0, material.Name.IndexOf(".")); }
                        material.Name = material.Name.Replace("\\", "");
                    }

                    SceneManager.Current.UpdateProgress("Processing powerups and accessories");

                    for (int i = bones.Count - 1; i >= 0; i--)
                    {
                        var bone = bones[i];

                        if (bone.Name.StartsWith("&"))
                        {
                            var entity = new Entity();

                            if (bone.Name.StartsWith("&£"))
                            {
                                string key = bone.Name.Substring(2, 2);

                                entity.UniqueIdentifier = "errol_B00BIE" + key + "_" + i.ToString("000");
                                entity.EntityType = EntityType.Powerup;

                                var pup = ToxicRagers.Carmageddon2.Powerups.LookupID(int.Parse(key));

                                if (pup.InCR)
                                {
                                    entity.Name = "pup_" + pup.Name;
                                    entity.Tag = pup.Model;
                                }
                                else
                                {
                                    entity.Name = "pup_Credits";
                                    entity.Tag = pup.Model;
                                }
                            }
                            else
                            {
                                // accessory
                                entity.UniqueIdentifier = "errol_HEAD00" + bone.Name.Substring(1, 2) + "_" + i.ToString("000");
                                entity.EntityType = EntityType.Accessory;
                                entity.Name = "C2_" + bone.Mesh.Name.Substring(3);
                            }

                            entity.Transform = bone.CombinedTransform;
                            entity.AssetType = AssetType.Sprite;
                            SceneManager.Current.Entities.Add(entity);

                            SceneManager.Current.Models[0].RemoveBone(bone.Index);
                        }
                    }

                    SceneManager.Current.UpdateProgress("Processing complete!");

                    SceneManager.Current.Change(ChangeType.Munge, -1);
                    break;
            }
        }

        private void menuCarmageddonMobileClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "Vehicle":
                    openContent("Carmageddon Mobile Vehicles (carbody.cnt)|carbody.cnt");
                    break;
            }
        }
        

        private void openContent(string filter)
        {
            ofdBrowse.Filter = filter;

            if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
            {
                SceneManager.Current.Reset();
                SceneManager.Current.Content.Load<Model, CNTImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);
            }
        }

        private void menuCarmageddonReincarnationClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            SceneManager.Current.SetCoordinateSystem(SceneManager.CoordinateSystem.LeftHanded);

            switch (mi.Text)
            {
                case "Accessory":
                    ofdBrowse.Filter = "Carmageddon ReinCARnation Accessory files (accessory.cnt)|accessory.cnt";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Reset();
                        var accessory = SceneManager.Current.Add(SceneManager.Current.Content.Load<Model, CNTImporter>(Path.GetFileName(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName)));

                        string accessorytxt = ofdBrowse.FileName.Replace(".cnt", ".txt", StringComparison.OrdinalIgnoreCase);

                        if (File.Exists(accessorytxt))
                        {
                            accessory.SupportingDocuments["Accessory"] = ToxicRagers.CarmageddonReincarnation.Formats.Accessory.Load(accessorytxt);
                        }
                    }
                    break;

                case "Environment":
                    openContent("Carmageddon ReinCARnation Environment files (level.cnt)|level.cnt");
                    break;

                case "Pedestrian":
                    openContent("Carmageddon ReinCARnation Pedestrians (bodyform.cnt)|bodyform.cnt");
                    break;

                case "Vehicle":
                    ofdBrowse.Filter = "Carmageddon ReinCARnation Vehicles (car.cnt)|car.cnt";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Reset();

                        string assetFolder = Path.GetDirectoryName(ofdBrowse.FileName) + "\\";
                        var vehicle = (Model)SceneManager.Current.Add(SceneManager.Current.Content.Load<Model, CNTImporter>(Path.GetFileName(ofdBrowse.FileName), assetFolder));

                        // Load supporting documents
                        if (File.Exists(assetFolder + "setup.lol")) { vehicle.SupportingDocuments["Setup"] = ToxicRagers.CarmageddonReincarnation.Formats.Setup.Load(assetFolder + "setup.lol"); }
                        if (File.Exists(assetFolder + "Structure.xml")) { vehicle.SupportingDocuments["Structure"] = ToxicRagers.CarmageddonReincarnation.Formats.Structure.Load(assetFolder + "Structure.xml"); }
                        if (File.Exists(assetFolder + "SystemsDamage.xml")) { vehicle.SupportingDocuments["SystemsDamage"] = ToxicRagers.CarmageddonReincarnation.Formats.SystemsDamage.Load(assetFolder + "SystemsDamage.xml"); }
                        if (File.Exists(assetFolder + "vehicle_setup.cfg")) { vehicle.SupportingDocuments["VehicleSetupConfig"] = ToxicRagers.CarmageddonReincarnation.Formats.VehicleSetupConfig.Load(assetFolder + "vehicle_setup.cfg"); }

                        foreach (var bone in vehicle.Bones)
                        {
                            string boneName = bone.Name.ToLower();

                            if (boneName.StartsWith("wheel_") || boneName.StartsWith("vfx_") || boneName.StartsWith("driver"))
                            {
                                var entity = new Entity
                                {
                                    Name = bone.Name,
                                    EntityType = (boneName.StartsWith("driver") ? EntityType.Driver : (boneName.StartsWith("wheel_") ? EntityType.Wheel : EntityType.VFX)),
                                    AssetType = AssetType.Sprite
                                };
                                entity.LinkWith(bone);

                                SceneManager.Current.Entities.Add(entity);
                            }
                        }
                    }
                    break;

                case "CNT files":
                case "MDL files":
                case "MTL files":
                case "TDX files":
                case "LIGHT files":
                case "Accessory.txt files":
                case "Routes.txt files":
                case "vehicle_setup.cfg files":
                case "Structure.xml files":
                case "SystemsDamage.xml files":
                case "Setup.lol files":
                case "ZAD files":
                    processAll(mi.Text);
                    break;

                case "Wheel Preview":
                    var preview = new frmReincarnationWheelPreview();

                    var result = preview.ShowDialog();

                    switch (result)
                    {
                        case DialogResult.OK:
                        case DialogResult.Abort:
                            foreach (var entity in SceneManager.Current.Entities)
                            {
                                if (entity.EntityType == EntityType.Wheel)
                                {
                                    entity.Asset = (result == DialogResult.OK ? preview.Wheel : null);
                                    entity.AssetType = (result == DialogResult.OK ? AssetType.Model : AssetType.Sprite);
                                }
                            }
                            break;
                    }
                    break;

                case "Bulk UnZAD":
                    if (MessageBox.Show(string.Format("Are you entirely sure?  This will extra ALL ZAD files in and under\r\n{0}\r\nThis will require at least 30gb of free space", Properties.Settings.Default.PathCarmageddonReincarnation), "Totes sure?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (Directory.Exists(Properties.Settings.Default.PathCarmageddonReincarnation))
                        {
                            int success = 0;
                            int fail = 0;

                            ToxicRagers.Helpers.IO.LoopDirectoriesIn(Properties.Settings.Default.PathCarmageddonReincarnation, (d) =>
                            {
                                foreach (FileInfo fi in d.GetFiles("*.zad"))
                                {
                                    var zad = ToxicRagers.Stainless.Formats.ZAD.Load(fi.FullName);
                                    int i = 0;

                                    if (zad != null)
                                    {
                                        if (!zad.IsVT)
                                        {
                                            foreach (var entry in zad.Contents)
                                            {
                                                i++;

                                                zad.Extract(entry, Properties.Settings.Default.PathCarmageddonReincarnation);

                                                if (i % 25 == 0)
                                                {
                                                    tsslProgress.Text = string.Format("[{0}/{1}] {2} -> {3}", success, fail, fi.Name, entry.Name);
                                                    Application.DoEvents();
                                                }
                                            }

                                            success++;
                                        }
                                    }
                                    else
                                    {
                                        fail++; 
                                    }

                                    tsslProgress.Text = string.Format("[{0}/{1}] {2}", success, fail, fi.FullName.Replace(Properties.Settings.Default.PathCarmageddonReincarnation, ""));
                                    Application.DoEvents();
                                }
                            }
                            );

                            tsslProgress.Text = string.Format("unZADing complete. {0} success {1} fail", success, fail);
                        }
                    }
                    break;
            }
        }

        private void menuNovadromeClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "Environment":
                    openContent("Novadrome Environments (level-*.cnt)|level-*.cnt");
                    break;

                case "Vehicle":
                    openContent("Novadrome Vehicles (carbody.cnt)|carbody.cnt");
                    break;

                case "XT2 files":
                    processAll(mi.Text);
                    break;
            }
        }

        private void processAll(string fileType)
        {
            string extension = fileType.Substring(0, fileType.IndexOf(' ')).ToLower();
            fbdBrowse.SelectedPath = (Properties.Settings.Default.LastBrowsedFolder != null ? Properties.Settings.Default.LastBrowsedFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));

            int success = 0;
            int fail = 0;

            if (fbdBrowse.ShowDialog() == DialogResult.OK && Directory.Exists(fbdBrowse.SelectedPath))
            {
                Properties.Settings.Default.LastBrowsedFolder = fbdBrowse.SelectedPath;
                Properties.Settings.Default.Save();

                ToxicRagers.Helpers.IO.LoopDirectoriesIn(fbdBrowse.SelectedPath, (d) =>
                {
                    foreach (FileInfo fi in d.GetFiles((extension.Contains(".") ? extension : "*." + extension)))
                    {
                        object result = null;

                        switch (extension)
                        {
                            case "anm":
                                // Content\Peds\animations
                                break;

                            case "bin":
                                // Content\UI\assets\h1080_default\animation_binaries
                                // Content\Vehicles\*\ui_assets
                                break;

                            case "bzn":
                                // Content\Environments\
                                break;

                            case "rba":
                                break;

                            case "shp":
                                break;

                            case "cnt":
                                result = ToxicRagers.Stainless.Formats.CNT.Load(fi.FullName);
                                break;

                            case "mdl":
                                result = ToxicRagers.Stainless.Formats.MDL.Load(fi.FullName);
                                break;

                            case "mtl":
                                result = ToxicRagers.Stainless.Formats.MTL.Load(fi.FullName);
                                break;

                            case "tdx":
                                result = ToxicRagers.CarmageddonReincarnation.Formats.TDX.Load(fi.FullName);
                                break;

                            case "light":
                                result = ToxicRagers.CarmageddonReincarnation.Formats.LIGHT.Load(fi.FullName);
                                break;

                            case "xt2":
                                result = ToxicRagers.Novadrome.Formats.XT2.Load(fi.FullName);
                                break;

                            case "accessory.txt":
                                result = ToxicRagers.CarmageddonReincarnation.Formats.Accessory.Load(fi.FullName);
                                break;

                            case "routes.txt":
                                result = ToxicRagers.CarmageddonReincarnation.Formats.Routes.Load(fi.FullName);
                                break;

                            case "vehicle_setup.cfg":
                                result = ToxicRagers.CarmageddonReincarnation.Formats.VehicleSetupConfig.Load(fi.FullName);
                                break;

                            case "structure.xml":
                                result = ToxicRagers.CarmageddonReincarnation.Formats.Structure.Load(fi.FullName);
                                break;

                            case "systemsdamage.xml":
                                result = ToxicRagers.CarmageddonReincarnation.Formats.SystemsDamage.Load(fi.FullName);
                                break;

                            case "setup.lol":
                                result = ToxicRagers.CarmageddonReincarnation.Formats.Setup.Load(fi.FullName);
                                break;

                            case "txt":
                                result = ToxicRagers.Carmageddon.Formats.Car.Load(fi.FullName);
                                break;

                            case "zad":
                                result = ToxicRagers.Stainless.Formats.ZAD.Load(fi.FullName);
                                break;
                        }

                        if (result != null) { success++; } else { fail++; }

                        tsslProgress.Text = string.Format("[{0}/{1}] {2}", success, fail, fi.FullName.Replace(fbdBrowse.SelectedPath, ""));
                        Application.DoEvents();
                    }
                }
                );

                tsslProgress.Text = string.Format("{0} processing complete. {1} success {2} fail", extension, success, fail);
            }
        }

        private void menuTDR2000Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "Hierarchy":
                    ofdBrowse.Filter = "TDR2000 Hierarchy (*.hie)|*.hie";

                    if (ofdBrowse.ShowDialog() == DialogResult.OK && File.Exists(ofdBrowse.FileName))
                    {
                        SceneManager.Current.Reset();
                        SceneManager.Current.Content.Load<Model, ContentPipeline.TDR2000.HIEImporter>(Path.GetFileNameWithoutExtension(ofdBrowse.FileName), Path.GetDirectoryName(ofdBrowse.FileName), true);
                    }
                    break;

                case "Remove LOD from Vehicle":
                    if (SceneManager.Current.Models.Count == 0) { return; }

                    for (int i = SceneManager.Current.Models[0].Bones.Count - 1; i >= 0; i--)
                    {
                        var bone = SceneManager.Current.Models[0].Bones[i];

                        if (bone.Name.Contains("LOD"))
                        {
                            string name = bone.Name.Replace("_", "");
                            if (name.Substring(name.IndexOf("LOD") + 3, 1) != "1")
                            {
                                SceneManager.Current.Models[0].RemoveBone(bone.Index);
                            }
                        }
                    }

                    SceneManager.Current.Change(ChangeType.Munge, -1);
                    break;
            }
        }

        private void menuSaveForClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "Carmageddon 2":
                    sfdBrowse.Filter = "BRender ACT files (*.act)|*.act";

                    if (sfdBrowse.ShowDialog() == DialogResult.OK)
                    {
                        string directory = Path.GetDirectoryName(sfdBrowse.FileName) + "\\";
                        var textures = new HashSet<string>();
                        if (!Directory.Exists(directory + "tiffrgb")) { Directory.CreateDirectory(directory + "tiffrgb"); }

                        var ax = new ACTExporter();
                        ax.Export(SceneManager.Current.Models[0], sfdBrowse.FileName);

                        var dx = new DATExporter();
                        dx.Export(SceneManager.Current.Models[0], directory + Path.GetFileNameWithoutExtension(sfdBrowse.FileName) + ".dat");

                        var mx = new MATExporter();
                        mx.Export(SceneManager.Current.Materials, directory + Path.GetFileNameWithoutExtension(sfdBrowse.FileName) + ".mat");

                        foreach (var material in SceneManager.Current.Materials)
                        {
                            if (material.Texture.Name != null && textures.Add(material.Texture.Name))
                            {
                                var tx = new TIFExporter();
                                tx.Export(material.Texture, directory + "tiffrgb\\" + material.Texture.Name + ".tif");
                            }
                        }

                        SceneManager.Current.UpdateProgress(Path.GetFileName(sfdBrowse.FileName) + " saved successfully");
                    }
                    break;

                case "Carmageddon Reincarnation":
                    sfdBrowse.Filter = "Stainless CNT files (*.cnt)|*.cnt";
                    if (sfdBrowse.ShowDialog() == DialogResult.OK)
                    {
                        var cx = new CNTExporter();
                        cx.Export(SceneManager.Current.Models[0], sfdBrowse.FileName);

                        var mx = new MDLExporter();
                        mx.Export(SceneManager.Current.Models[0], Path.GetDirectoryName(sfdBrowse.FileName) + "\\");
                    }
                    break;
            }
        }

        private void menuSaveAsClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "Level":
                    (new frmSaveAsLevel()).ShowDialog(this);
                    break;

                case "Vehicle":
                    (new frmSaveAsVehicle()).ShowDialog(this);
                    break;
            }
        }

        private void menuViewClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "Preferences":
                    var prefs = new frmPreferences();
                    if (prefs.ShowDialog(this) == DialogResult.OK)
                    {
                        InputManager.Current.ReloadBindings();
                    }
                    break;
            }
        }

        private void menuObjectClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            int boneIndex = SceneManager.Current.SelectedBoneIndex;
            int modelIndex = SceneManager.Current.SelectedModelIndex;
            int modelBoneKey = ModelBone.GetModelBoneKey(modelIndex, boneIndex);

            switch (mi.Text)
            {
                case "New...":
                    var addNew = new frmNewObject();
                    addNew.SetParentNode(modelIndex, boneIndex);

                    if (addNew.ShowDialog(this) == DialogResult.OK)
                    {
                        SceneManager.Current.Change(ChangeType.Add, addNew.NewBoneKey, modelBoneKey);
                    }
                    break;

                case "Remove...":
                    var removeObject = new frmRemoveObject();
                    removeObject.SetParentNode(modelIndex, boneIndex);

                    if (removeObject.ShowDialog(this) == DialogResult.OK)
                    {
                        SceneManager.Current.Change(ChangeType.Delete, modelBoneKey, removeObject.RemovedBone);
                    }
                    break;

                case "Modify geometry...":
                    var geometry = new frmModifyModel();
                    geometry.SetParentNode(modelIndex, boneIndex);

                    if (geometry.ShowDialog(this) == DialogResult.OK)
                    {
                        SceneManager.Current.Change(ChangeType.Transform, modelBoneKey);
                    }
                    break;

                case "Modify actor...":
                    var transform = new frmModifyActor();
                    transform.SetParentNode(modelIndex, boneIndex);

                    if (transform.ShowDialog(this) == DialogResult.OK)
                    {
                        SceneManager.Current.Change(ChangeType.Transform, modelBoneKey);
                    }
                    break;

                case "Rename":
                    var rename = new frmRename();
                    rename.SetParentNode(modelIndex, boneIndex);

                    if (rename.ShowDialog(this) == DialogResult.OK)
                    {
                        SceneManager.Current.Change(ChangeType.Rename, modelBoneKey, rename.NewName);
                    }
                    break;

                case "Add/Change Type...":
                    var changeType = new frmChangeNodeType();
                    changeType.SetParentNode(modelIndex, boneIndex);

                    if (changeType.ShowDialog(this) == DialogResult.OK)
                    {
                        SceneManager.Current.Change(ChangeType.ChangeType, modelBoneKey);
                    }
                    break;

                case "Remove":
                    break;

                case "Optimise":
                    foreach (ModelMesh mesh in SceneManager.Current.SelectedModel.Meshes)
                    {
                        foreach (ModelMeshPart part in mesh.MeshParts)
                        {
                            part.Optimise();
                        }
                    }
                    break;

                case "Flatten hierarchy...":
                    SceneManager.Current.UpdateProgress("TODO: Code \"Flatten hierarchy...\"");
                    break;

                case "Invert texture 'v' coordinates":
                    ModelManipulator.FlipUVs(SceneManager.Current.SelectedModel.Bones[SceneManager.Current.SelectedBoneIndex].Mesh);
                    break;
            }
        }

        private void menuToolsClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;

            switch (mi.Text)
            {
                case "TDX Convertor":
                    var tdx = new frmTDXConvert();
                    tdx.Show(this);
                    break;
            }
        }

        public void SetActionScalingText(string scale)
        {
            tsslActionScaling.Text = scale;
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            FlummeryApplication.Active = true;
        }

        private void frmMain_Deactivate(object sender, EventArgs e)
        {
            FlummeryApplication.Active = false;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            FlummeryApplication.Active = (WindowState != FormWindowState.Minimized);
        }
    }
}
 