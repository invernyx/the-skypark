using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.PathFinding
{
    class PathFinding
    {
        public class Query
        {
            public SearchMap Map = new SearchMap();
            public Query(SearchMap _Map)
            {
                Map = _Map;
            }

            public void Process()
            {
                Map.ShortestPath = new SearchEngine(Map).GetShortestPathAstart();
            }
        }

        public class SearchMap
        {
            public List<Node> Nodes { get; set; } = new List<Node>();
            public Node StartNode { get; set; }
            public Node EndNode { get; set; }
            public List<Node> ShortestPath { get; set; } = new List<Node>();
        }

        public class Node
        {
            public uint Id { get; set; }
            public NodeType Type { get; set; }
            public int Cluster { get; set; }
            public GeoLoc Location { get; set; }
            public List<Connection> Connections { get; set; } = new List<Connection>();

            public double? MinCostToStart { get; set; }
            public Node NearestToStart { get; set; }
            public bool Visited { get; set; }
            public double StraightLineDistanceToEnd { get; set; }
            
            public double StraightLineDistanceTo(Node end)
            {
                return Math.Sqrt(Math.Pow(Location.Lon - end.Location.Lat, 2) + Math.Pow(Location.Lat - end.Location.Lat, 2));
            }

            public override string ToString()
            {
                return Id.ToString() + " / " + Type.ToString();
            }
            
        }

        public class Connection
        {
            public double Length { get; set; }
            public double Cost { get; set; }
            public Node ConnectedNode { get; set; }

            public override string ToString()
            {
                return ConnectedNode.ToString();
            }
        }
        
        public enum NodeType
        {
            Path,
            Parking
        }

        public class PathFindingHelpers
        {
            public static Dictionary<Node, double> SortNodesByDistance(List<Node> Cluster, GeoLoc FromLocation)
            {
                Dictionary<Node, double> SortedNodeByDistance = new Dictionary<Node, double>();
                foreach (Node Node in Cluster)
                {
                    SortedNodeByDistance.Add(Node, Utils.MapCalcDist(FromLocation.Lat, FromLocation.Lon, Node.Location.Lat, Node.Location.Lon, Utils.DistanceUnit.Meters));
                }
                return SortedNodeByDistance.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            public static KeyValuePair<List<Node>, GeoLoc>? GetClosestSegment(List<Node> Cluster, GeoLoc FromLocation, double exclusionM)
            {
                List<Node> ProcessedNodes = new List<Node>();
                List<KeyValuePair<List<Node>, GeoLoc>> PossibleSegments = new List<KeyValuePair<List<Node>, GeoLoc>>();
                foreach(Node node in Cluster)
                {
                    if(Utils.MapCalcDist(FromLocation, new GeoLoc(node.Location.Lon, node.Location.Lat), Utils.DistanceUnit.Meters) > exclusionM)
                    {
                        foreach (Connection cnn in node.Connections)
                        {
                            if (!ProcessedNodes.Contains(cnn.ConnectedNode))
                            {
                                GeoLoc potential = Utils.ClosestPointToSegment(FromLocation, new GeoLoc(node.Location.Lon, node.Location.Lat), new GeoLoc(cnn.ConnectedNode.Location.Lon, cnn.ConnectedNode.Location.Lat));
                                if (potential != null)
                                {
                                    PossibleSegments.Add(new KeyValuePair<List<Node>, GeoLoc>(new List<Node>() { cnn.ConnectedNode, node }, potential));
                                }
                            }
                        }
                    }
                    ProcessedNodes.Add(node);
                }

                List<KeyValuePair<List<Node>, GeoLoc>> SortedPossibles = PossibleSegments.OrderBy(x => Utils.MapCalcDist(FromLocation, x.Value)).ToList();
                KeyValuePair<List<Node>, GeoLoc>? Final = null;

                if(SortedPossibles.Count > 0)
                {
                    Final = SortedPossibles[0];
                }

                return Final;
            }
        }
    }
}
