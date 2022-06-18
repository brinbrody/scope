using DGScope.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ScopeWindow.ScopeWindow;

namespace ScopeWindow
{
    public class ScopeWindowSettings
    {
        public string AdaptationFileName { get; set; }
        public List<PrefSet> PrefSets { get; set; }
        public WSType WSType { get; set; } = WSType.TCW;
    }
}
