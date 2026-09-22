using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nika_Vizard_Station.MainWindow;

namespace Nika_Vizard_Station
{
    public class PixelSortingSettings
    {
        public SortBy SortFilter { get; set; }
        public SortBy MaskFilter { get; set; }
        public int MaskStart { get; set; }
        public int MaskEnd { get; set; }
        public bool[][] Mask { get; set; }
        public bool IsHorizontal { get; set; } = true;
    }
}
