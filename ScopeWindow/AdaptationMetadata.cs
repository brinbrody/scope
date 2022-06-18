using DGScope.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeWindow
{
    public class AdaptationMetadata
    {
        [Editor(typeof(VideoMapCollectionEditor), typeof(UITypeEditor))]
        public VideoMapList VideoMaps { get; set;  }
        
    }
}
