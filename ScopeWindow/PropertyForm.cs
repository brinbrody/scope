using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScopeWindow
{
    public partial class PropertyForm : Form
    {
        public PropertyForm(object selectedObject)
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = selectedObject;
        }

        public PropertyForm(object[] selectedObjects)
        {
            InitializeComponent();
            propertyGrid1.SelectedObjects = selectedObjects;
        }
    }
}
