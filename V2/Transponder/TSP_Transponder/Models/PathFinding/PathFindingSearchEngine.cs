using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Models.PathFinding.PathFinding;

namespace TSP_Transponder.Models.PathFinding
{
    class SearchEngine
    {
        public event EventHandler Updated;
        private void OnUpdated()
        {
            Updated?.Invoke(null, EventArgs.Empty);
        }
        public SearchMap Map { get; set; }
        public Node Start { get; set; }
        public Node End { get; set; }
        public int NodeVisits { get; private set; }
        public double ShortestPathLength { get; set; }
        public double ShortestPathCost { get; private set; }

        public SearchEngine(SearchMap map)
        {
            Map = map;
            End = map.EndNode;
            Start = map.StartNode;
        }
        
        private void BuildShortestPath(List<Node> list, Node node)
        {
            if (node.NearestToStart == null || node.Connections.Count == 0)
            {
                return;
            }
            list.Add(node.NearestToStart);
            ShortestPathLength += node.Connections.Single(x => x.ConnectedNode == node.NearestToStart).Length;
            ShortestPathCost += node.Connections.Single(x => x.ConnectedNode == node.NearestToStart).Cost;
            BuildShortestPath(list, node.NearestToStart);
        }
        
        public List<Node> GetShortestPathAstart()
        {
            foreach (var node in Map.Nodes)
            {
                node.StraightLineDistanceToEnd = node.StraightLineDistanceTo(End);
            }
            AstarSearch();
            List<Node> shortestPath = new List<Node>();
            shortestPath.Add(End);
            BuildShortestPath(shortestPath, End);
            shortestPath.Reverse();
            return shortestPath;
        }

        private void AstarSearch()
        {
            NodeVisits = 0;
            Start.MinCostToStart = 0;
            List<Node> prioQueue = new List<Node>();
            prioQueue.Add(Start);
            while (prioQueue.Any())
            {
                prioQueue = prioQueue.OrderBy(x => x.MinCostToStart + x.StraightLineDistanceToEnd).ToList();
                var node = prioQueue.First();
                prioQueue.Remove(node);
                NodeVisits++;
                foreach (var cnn in node.Connections.OrderBy(x => x.Cost))
                {
                    Node childNode = cnn.ConnectedNode;
                    if (childNode.Visited)
                    {
                        continue;
                    }
                    if (childNode.MinCostToStart == null || node.MinCostToStart + cnn.Cost < childNode.MinCostToStart)
                    {
                        childNode.MinCostToStart = node.MinCostToStart + cnn.Cost;
                        childNode.NearestToStart = node;
                        if (!prioQueue.Contains(childNode))
                        {
                            prioQueue.Add(childNode);
                        }
                    }
                }
                node.Visited = true;
                if (node == End)
                {
                    return;
                }
            };
        }
    }
}
