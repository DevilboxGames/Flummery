using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK;
using ToxicRagers.Helpers;
using WeifenLuo.WinFormsUI.Docking;

namespace Flummery
{
    public partial class widgetStructure : DockContent
    {
        public widgetStructure()
        {
            InitializeComponent();

            resetWidget();
        }

        public void RegisterEventHandlers()
        {
            SceneManager.Current.OnSelect += scene_OnSelect;
        }

        void scene_OnSelect(object sender, SelectEventArgs e)
        {
            resetWidget();
        }

        private void resetWidget()
        {
        }
    }
}
