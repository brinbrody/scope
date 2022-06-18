using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DGScope.Library;


namespace ScopeWindow
{
    public partial class AdaptationForm : Form
    {
        bool changed = false;
        string filename = "";
        Adaptation adaptation;
        ScopeWindow window;
        public AdaptationForm(Adaptation adaptation = null)
        {
            this.adaptation = adaptation;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeComponent();
            var color_provider = new AssociatedMetadataTypeTypeDescriptionProvider(
                typeof(ScopeColor), typeof(ScopeColorMetadata));
            var colorset_provider = new AssociatedMetadataTypeTypeDescriptionProvider(
                typeof(ColorSet), typeof(ColorsetMetadata));
            var adaptation_provider = new AssociatedMetadataTypeTypeDescriptionProvider(
                typeof(Adaptation), typeof(AdaptationMetadata));
            TypeDescriptor.AddProvider(color_provider, typeof(ScopeColor));
            TypeDescriptor.AddProvider(colorset_provider, typeof(ColorSet));
            TypeDescriptor.AddProvider(adaptation_provider, typeof(Adaptation));
            propertyGrid1.SelectedObject = adaptation;
            propertyGrid1.PropertyValueChanged += PropertyGrid1_PropertyValueChanged;
            FormClosing += AdaptationForm_FormClosing;
        }
        public AdaptationForm(ScopeWindow window)
        {
            this.window = window;
            adaptation = window.Facility.Adaptation;   
        }

        private void AdaptationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckChange())
                e.Cancel = true;
        }

        private void PropertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            changed = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            propertyGrid1.Invalidate();
        }

        private bool CheckChange()
        {
            if (changed)
            {
                switch (MessageBox.Show("Do you want to save your changes?", "Unsaved changes", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel:
                        return false;
                    case DialogResult.Yes:
                        SaveToFile();
                        break;
                    case DialogResult.No:
                        return true;
                }
            }
            return !changed;
        }

        private void SaveToFile(bool saveas = false)
        {
            string filename = this.filename;
            if (filename == null || filename == "" || saveas)
            {
                using (SaveFileDialog s = new SaveFileDialog())
                {
                    s.Filter = "Adaptation (*.adaptjson)|*.adaptjson";
                    s.FilterIndex = 1;
                    s.RestoreDirectory = true;

                    if (s.ShowDialog() == DialogResult.OK)
                    {
                        filename = s.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            if (filename == null || filename == "")
                return;
            try
            {
                Adaptation.SerializeToJsonFile(adaptation, filename);
                changed = false;
                this.filename = filename;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckChange();
            if (!CheckChange())
                return;
            filename = "";
            adaptation = new Adaptation();
            if (window != null)
                window.Facility.Adaptation = adaptation;
            propertyGrid1.SelectedObject = adaptation;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToFile(true);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckChange())
                return;
            string filename;
            using (OpenFileDialog s = new OpenFileDialog())
            {
                s.Filter = "Adaptation (*.adaptjson)|*.adaptjson";
                s.FilterIndex = 1;
                s.RestoreDirectory = true;

                if (s.ShowDialog() == DialogResult.OK)
                {
                    filename = s.FileName;
                }
                else
                {
                    return;
                }
            }
            try
            {
                adaptation = Adaptation.DeserializeFromJsonFile(filename); 
                if (window != null)
                    window.Facility.Adaptation = adaptation;
                this.filename = filename;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            propertyGrid1.SelectedObject = adaptation;
        }
    }
}
