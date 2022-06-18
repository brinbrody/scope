using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DGScope.Library;

namespace ScopeWindow
{
    public partial class BriteForm : Form
    {
        BrightnessSettings settings;
        public BriteForm(BrightnessSettings brightnessSettings)
        {
            settings = brightnessSettings;
            InitializeComponent();
            trackBar1.Value = settings.DCB;
            trackBar2.Value = settings.BKC;
            trackBar3.Value = settings.MPA;
            trackBar4.Value = settings.MPB;
            trackBar5.Value = settings.FDB;
            trackBar6.Value = settings.LST;
            trackBar7.Value = settings.POS;
            trackBar8.Value = settings.LDB;
            trackBar10.Value = settings.OTH;
            trackBar9.Value = settings.TLS;
            trackBar18.Value = settings.RR;
            trackBar17.Value = settings.CMP;
            trackBar16.Value = settings.BCN;
            trackBar15.Value = settings.PRI;
            trackBar14.Value = settings.HST;
            trackBar13.Value = settings.WX;
            trackBar12.Value = settings.WXC;
        }

        private void TrackBar_Scroll(object sender, EventArgs e)
        {
            settings.DCB = trackBar1.Value;
            settings.BKC = trackBar2.Value;
            settings.MPA = trackBar3.Value;
            settings.MPB = trackBar4.Value;
            settings.FDB = trackBar5.Value;
            settings.LST = trackBar6.Value;
            settings.POS = trackBar7.Value;
            settings.LDB = trackBar8.Value;
            settings.OTH = trackBar10.Value;
            settings.TLS = trackBar9.Value;
            settings.RR = trackBar18.Value;
            settings.CMP = trackBar17.Value;
            settings.BCN = trackBar16.Value;
            settings.PRI = trackBar15.Value;
            settings.HST = trackBar14.Value;
            settings.WX = trackBar13.Value;
            settings.WXC = trackBar12.Value;
        } 

    }
}
