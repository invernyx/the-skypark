using Orbx.DataManager.Core.Esp;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class TaxiPath
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int RunwayDesignator { get; set; }
        public TaxiwayPathType Type { get; set; }
        public bool DrawSurface { get; set; }
        public bool DrawDetail { get; set; }
        public int RunwayNumberNameIndex { get; set; }
        public bool CentreLine { get; set; }
        public bool CentreLineLit { get; set; }
        public TaxiwayEdgeMarking LeftEdgeType { get; set; }
        public bool LeftEdgeLit { get; set; }
        public TaxiwayEdgeMarking RightEdgeType { get; set; }
        public bool RightEdgeLit { get; set; }
        public Surface Surface { get; set; }
        public double Width { get; set; }
        public double WeightLimit { get; set; }
    }
}
