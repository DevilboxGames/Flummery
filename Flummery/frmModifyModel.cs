﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using ToxicRagers.Helpers;

namespace Flummery
{
    public partial class frmModifyModel : Form
    {
        int modelIndex;
        int boneIndex;

        public frmModifyModel()
        {
            InitializeComponent();
        }

        public void SetParentNode(int modelID, int boneID)
        {
            modelIndex = modelID;
            boneIndex = boneID;
        }

        private void frmModifyModel_Load(object sender, EventArgs e)
        {
            cboInvertAxis.SelectedIndex = 0;
        }

        private void rdo_CheckedChanged(object sender, EventArgs e)
        {
            string name = ((RadioButton)sender).Name.Substring(3);

            foreach (Control c in this.Controls)
            {
                if (c is GroupBox) { c.Visible = false; }
            }

            var groupBox = this.Controls.Find("gb" + name, true);
            if (groupBox.Length > 0) { groupBox[0].Visible = !groupBox[0].Visible; }
        }

        // Scale START
        private void rdoScale_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control c in this.gbScaling.Controls) { if (c is TextBox) { c.Enabled = false; } }

            switch (((RadioButton)sender).Name)
            {
                case "rdoScaleWholeModel":
                    this.txtScaleWholeModel.Enabled = true;
                    break;

                case "rdoScaleByAxis":
                    this.txtScaleAxisX.Enabled = true;
                    this.txtScaleAxisY.Enabled = true;
                    this.txtScaleAxisZ.Enabled = true;
                    break;

                case "rdoScaleRadius":
                    this.txtScaleRadius.Enabled = true;
                    break;
            }
        }
        // Scale END

        private void btnOK_Click(object sender, EventArgs e)
        {
            applyTransforms();
            this.Close();
        }

        private void applyTransforms()
        {
            var bones = (chkHierarchy.Checked ? SceneManager.Current.Models[modelIndex].Bones[boneIndex].AllChildren() : new ModelBoneCollection { SceneManager.Current.Models[modelIndex].Bones[boneIndex] });

            if (rdoScaling.Checked)
            {
                OpenTK.Matrix4 scaleMatrix = OpenTK.Matrix4.Identity;

                if (rdoScaleWholeModel.Checked)
                {
                    scaleMatrix = OpenTK.Matrix4.CreateScale(
                        txtScaleWholeModel.Text.ToSingle(), 
                        txtScaleWholeModel.Text.ToSingle(), 
                        txtScaleWholeModel.Text.ToSingle()
                    );
                }
                else if (rdoScaleByAxis.Checked)
                {
                    scaleMatrix = OpenTK.Matrix4.CreateScale(
                        txtScaleAxisX.Text.ToSingle(), 
                        txtScaleAxisY.Text.ToSingle(), 
                        txtScaleAxisZ.Text.ToSingle()
                    );
                }

                ModelManipulator.Scale(bones, scaleMatrix, chkHierarchy.Checked);
            }
            else if (rdoMunging.Checked)
            {
                if (rdoInvert.Checked)
                {
                    ModelManipulator.FlipAxis(SceneManager.Current.Models[modelIndex].Bones[boneIndex].Mesh, cboInvertAxis.SelectedItem.ToString().ToEnum<Axis>(), chkHierarchy.Checked);
                }
                else if (rdoMeshBoneSwap.Checked)
                {
                    ModelManipulator.MungeMeshWithBone(bones);
                }
                else if (rdoFlipWindingOrder.Checked)
                {
                    ModelManipulator.FlipFaces(bones, chkHierarchy.Checked);
                }
            }
        }
    }
}
