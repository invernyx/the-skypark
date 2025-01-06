using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.PathFinding.Simple
{
    // https://www.codeproject.com/Articles/118015/Fast-A-Star-2D-Implementation-for-C
    public interface IPathNode<TUserContext>
    {
        BlockedSide[] GetBlockedSides(TUserContext inContext);
        AccessSide GetAccess(TUserContext inContext);
        Boolean IsWalkable(TUserContext inContext);
        Boolean IsLevelChange(TUserContext inContext);
    }

    public interface IIndexedObject
    {
        int Index { get; set; }
    }

    public enum BlockedSide
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
    }

    public enum AccessSide
    {
        All,
        Up,
        Right,
        Down,
        Left,
    }

    /// <summary>
    /// Uses about 50 MB for a 1024x1024 grid.
    /// </summary>
    public class SpatialAStar<TPathNode, TUserContext> where TPathNode : IPathNode<TUserContext>
    {
        private OpenCloseMap m_ClosedSet;
        private OpenCloseMap m_OpenSet;
        private PriorityQueue<PathNode> m_OrderedOpenSet;
        private PathNode[,,] m_CameFrom;
        private OpenCloseMap m_RuntimeGrid;
        private PathNode[,,] m_SearchSpace;

        public TPathNode[,,] SearchSpace { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        protected class PathNode : IPathNode<TUserContext>, IComparer<PathNode>, IIndexedObject
        {
            public static readonly PathNode Comparer = new PathNode(0, 0, 0, default(TPathNode));

            public TPathNode UserContext { get; internal set; }
            public Double G { get; internal set; }
            public Double H { get; internal set; }
            public Double F { get; internal set; }
            public int Index { get; set; }

            public BlockedSide[] GetBlockedSides(TUserContext inContext)
            {
                return UserContext.GetBlockedSides(inContext);
            }

            public AccessSide GetAccess(TUserContext inContext)
            {
                return UserContext.GetAccess(inContext);
            }

            public Boolean IsWalkable(TUserContext inContext)
            {
                return UserContext.IsWalkable(inContext);
            }

            public Boolean IsLevelChange(TUserContext inContext)
            {
                return UserContext.IsLevelChange(inContext);
            }

            public int X { get; internal set; }
            public int Y { get; internal set; }
            public int Z { get; internal set; }

            public int Compare(PathNode x, PathNode y)
            {
                if (x.F < y.F)
                    return -1;
                else if (x.F > y.F)
                    return 1;

                return 0;
            }

            public PathNode(int inX, int inY, int inZ, TPathNode inUserContext)
            {
                X = inX;
                Y = inY;
                Z = inZ;
                UserContext = inUserContext;
            }
        }

        public SpatialAStar(TPathNode[,,] inGrid)
        {
            SearchSpace = inGrid;
            Width = inGrid.GetLength(0);
            Height = inGrid.GetLength(1);
            Depth = inGrid.GetLength(2);
            m_SearchSpace = new PathNode[Width, Height, Depth];
            m_ClosedSet = new OpenCloseMap(Width, Height, Depth);
            m_OpenSet = new OpenCloseMap(Width, Height, Depth);
            m_CameFrom = new PathNode[Width, Height, Depth];
            m_RuntimeGrid = new OpenCloseMap(Width, Height, Depth);
            m_OrderedOpenSet = new PriorityQueue<PathNode>(PathNode.Comparer);

            for (int z = 0; z < Depth; z++)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (inGrid[x, y, z] == null)
                        {
                            m_SearchSpace[x, y, z] = null;
                        }
                        else
                        {
                            m_SearchSpace[x, y, z] = new PathNode(x, y, z, inGrid[x, y, z]);
                        }
                    }
                }
            }
        }

        protected virtual Double Heuristic(PathNode inStart, PathNode inEnd)
        {
            float deltaX = inStart.X - inEnd.X;
            float deltaY = inStart.Y - inEnd.Y;
            float deltaZ = (inStart.Z - inEnd.Z) * 50;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        private static readonly Double SQRT_2 = Math.Sqrt(2);

        protected virtual Double NeighborDistance(PathNode inStart, PathNode inEnd)
        {
            int diffX = Math.Abs(inStart.X - inEnd.X);
            int diffY = Math.Abs(inStart.Y - inEnd.Y);
            int diffZ = Math.Abs(inStart.Z - inEnd.Z);

            switch (diffX + diffY + diffZ)
            {
                case 1: return 1;
                case 2: return SQRT_2;
                case 0: return 0;
                default:
                    throw new ApplicationException();
            }
        }

        

        /// <summary>
        /// Returns null, if no path is found. Start- and End-Node are included in returned path. The user context
        /// is passed to IsWalkable().
        /// </summary>
        public LinkedList<TPathNode> Search(Point3D inStartNode, Point3D inEndNode, TUserContext inUserContext)
        {
            PathNode startNode = m_SearchSpace[inStartNode.X, inStartNode.Y, inStartNode.Z];
            PathNode endNode = m_SearchSpace[inEndNode.X, inEndNode.Y, inEndNode.Z];
            
            if (startNode == endNode)
                return new LinkedList<TPathNode>(new TPathNode[] { startNode.UserContext });

            PathNode[] neighborNodes = new PathNode[10];

            m_ClosedSet.Clear();
            m_OpenSet.Clear();
            m_RuntimeGrid.Clear();
            m_OrderedOpenSet.Clear();

            for (int z = 0; z < Depth; z++)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        m_CameFrom[x, y, z] = null;
                    }
                }
            }

            startNode.G = 0;
            startNode.H = Heuristic(startNode, endNode);
            startNode.F = startNode.H;

            m_OpenSet.Add(startNode);
            m_OrderedOpenSet.Push(startNode);

            m_RuntimeGrid.Add(startNode);

            int nodes = 0;


            while (!m_OpenSet.IsEmpty)
            {
                PathNode node = m_OrderedOpenSet.Pop();

                if (node == endNode)
                {
                    LinkedList<TPathNode> result = ReconstructPath(m_CameFrom, m_CameFrom[endNode.X, endNode.Y, endNode.Z]);
                    result.AddLast(endNode.UserContext);
                    return result;
                }

                m_OpenSet.Remove(node);
                m_ClosedSet.Add(node);

                StoreNeighborNodes(node, neighborNodes, inUserContext);


                for (int i = 0; i < neighborNodes.Length; i++)
                {
                    PathNode neighbor = neighborNodes[i];
                    Boolean tentative_is_better;
                    Boolean is_stairs = false;

                    if (neighbor == null)
                        continue;

                    if (!neighbor.UserContext.IsWalkable(inUserContext))
                        continue;

                    if (i > 7 && !neighbor.UserContext.IsLevelChange(inUserContext))
                        continue;

                    if (m_ClosedSet.Contains(neighbor))
                        continue;

                    if (node.UserContext.IsLevelChange(inUserContext))
                        is_stairs = true;

                    nodes++;

                    Double tentative_g_score = m_RuntimeGrid[node].G + NeighborDistance(node, neighbor) + (is_stairs ? 100 : (Utils.GetRandom(0, 10) == 0 ? Utils.GetRandom(0, 3) : 0));
                    Boolean wasAdded = false;

                    if (!m_OpenSet.Contains(neighbor))
                    {
                        m_OpenSet.Add(neighbor);
                        tentative_is_better = true;
                        wasAdded = true;
                    }
                    else if (tentative_g_score < m_RuntimeGrid[neighbor].G)
                    {
                        tentative_is_better = true;
                    }
                    else
                    {
                        tentative_is_better = false;
                    }

                    if (tentative_is_better)
                    {
                        m_CameFrom[neighbor.X, neighbor.Y, neighbor.Z] = node;

                        if (!m_RuntimeGrid.Contains(neighbor))
                            m_RuntimeGrid.Add(neighbor);

                        m_RuntimeGrid[neighbor].G = tentative_g_score;
                        m_RuntimeGrid[neighbor].H = Heuristic(neighbor, endNode);
                        m_RuntimeGrid[neighbor].F = m_RuntimeGrid[neighbor].G + m_RuntimeGrid[neighbor].H;

                        if (wasAdded)
                            m_OrderedOpenSet.Push(neighbor);
                        else
                            m_OrderedOpenSet.Update(neighbor);
                    }
                }
            }

            return null;
        }

        private LinkedList<TPathNode> ReconstructPath(PathNode[,,] came_from, PathNode current_node)
        {
            LinkedList<TPathNode> result = new LinkedList<TPathNode>();

            ReconstructPathRecursive(came_from, current_node, result);

            return result;
        }

        private void ReconstructPathRecursive(PathNode[,,] came_from, PathNode current_node, LinkedList<TPathNode> result)
        {
            PathNode item = came_from[current_node.X, current_node.Y, current_node.Z];

            if (item != null)
            {
                ReconstructPathRecursive(came_from, item, result);

                result.AddLast(current_node.UserContext);
            }
            else
                result.AddLast(current_node.UserContext);
        }

        private void StoreNeighborNodes(PathNode current, PathNode[] neighbors, TUserContext inUserContext)
        {
            // Get the coordinates and access of the current node
            int x = current.X;
            int y = current.Y;
            int z = current.Z;
            var acc = current.GetAccess(inUserContext);

            // Define a processing function to set neighbor nodes based on their access and blocked sides
            Action<int, int, int, int, AccessSide, AccessSide> proc = (index, x1, y1, z1, access_in, access_out) =>
            {

                // Check if the neighbor node is walkable
                Func<int, bool> is_neighbor_walkable = (i) =>
                {
                    return neighbors[i] != null ? (neighbors[i].IsWalkable(inUserContext)) : false;
                };

                // Get the matching path node in the search space
                var matching_path_node = m_SearchSpace[x1, y1, z1];
                if(matching_path_node != null)
                {
                    // Check if the access of the neighbor node matches the access of the current node
                    var n_acc = matching_path_node.GetAccess(inUserContext);
                    if ((n_acc == AccessSide.All || n_acc == access_in) && (acc == AccessSide.All || acc == access_out))
                    {
                        // Set the neighbor node based on its index and surrounding nodes
                        switch (index)
                        {
                            case 0:
                                {
                                    if(is_neighbor_walkable(3) && is_neighbor_walkable(1))
                                    {
                                        neighbors[0] = matching_path_node;
                                        return;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (is_neighbor_walkable(4) && is_neighbor_walkable(1))
                                    {
                                        neighbors[2] = matching_path_node;
                                        return;
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    if (is_neighbor_walkable(3) && is_neighbor_walkable(6))
                                    {
                                        neighbors[5] = matching_path_node;
                                        return;
                                    }
                                    break;
                                }
                            case 7:
                                {
                                    if (is_neighbor_walkable(4) && is_neighbor_walkable(6))
                                    {
                                        neighbors[7] = matching_path_node;
                                        return;
                                    }
                                    break;
                                }
                            default:
                                {
                                    neighbors[index] = matching_path_node;
                                    return;
                                }
                        }
                    }
                }

                // Set the neighbor node to null if it's not walkable or if it doesn't match the access of the current node
                neighbors[index] = null;
            };



            // Check if the node to the upper-left of the current node is a neighbor
            if ((x > 0) && (y > 0))                                 // Left/Up
                proc(0, x - 1, y - 1, z, AccessSide.All, AccessSide.All);
            else
                neighbors[0] = null;

            // Check if the node above the current node is a neighbor
            if (y > 0)                                              // Up
                proc(1, x, y - 1, z, AccessSide.Down, AccessSide.Up);
            else
                neighbors[1] = null;

            // Check if the node to the upper-right of the current node is a neighbor
            if ((x < Width - 1) && (y > 0))                         // Right/Up
                proc(2, x + 1, y - 1, z, AccessSide.All, AccessSide.All);
            else
                neighbors[2] = null;

            // Check if the node to the left of the current node is a neighbor
            if (x > 0)                                              // Left
                proc(3, x - 1, y, z, AccessSide.Right, AccessSide.Left);
            else
                neighbors[3] = null;

            // Check if the node to the right of the current node is a neighbor
            if (x < Width - 1)                                      // Right
                proc(4, x + 1, y, z, AccessSide.Left, AccessSide.Right);
            else
                neighbors[4] = null;

            // Check if the node to the lower-left of the current node is a neighbor
            if ((x > 0) && (y < Height - 1))                        // Left/Down
                proc(5, x - 1, y + 1, z, AccessSide.All, AccessSide.All);
            else
                neighbors[5] = null;

            // Check if the node below the current node is a neighbor
            if (y < Height - 1)                                     // Down
                proc(6, x, y + 1, z, AccessSide.Up, AccessSide.Down);
            else
                neighbors[6] = null;

            // Check if the node to the lower-right of the current node is a neighbor
            if ((x < Width - 1) && (y < Height - 1))                // Right/Down
                proc(7, x + 1, y + 1, z, AccessSide.All, AccessSide.All);
            else
                neighbors[7] = null;

            if(current.X == 1 && current.Y == 4)
            {

            }

            // Loop through each blocked side and remove the neighbor node if it's blocked
            var blocked = current.GetBlockedSides(inUserContext);
            foreach (var b in blocked)
            {
                switch (b)
                {
                    case BlockedSide.UpLeft:
                        neighbors[0] = null;
                        break;
                    case BlockedSide.Up:
                        neighbors[0] = null;
                        neighbors[1] = null;
                        neighbors[2] = null;
                        break;
                    case BlockedSide.UpRight:
                        neighbors[2] = null;
                        break;
                    case BlockedSide.Left:
                        neighbors[0] = null;
                        neighbors[3] = null;
                        neighbors[5] = null;
                        break;
                    case BlockedSide.Right:
                        neighbors[2] = null;
                        neighbors[4] = null;
                        neighbors[7] = null;
                        break;
                    case BlockedSide.DownLeft:
                        neighbors[5] = null;
                        break;
                    case BlockedSide.Down:
                        neighbors[5] = null;
                        neighbors[6] = null;
                        neighbors[7] = null;
                        break;
                    case BlockedSide.DownRight:
                        neighbors[7] = null;
                        break;

                }
            }


            // Check if there are any neighbors on lower levels
            int zl = z;
            neighbors[8] = null;
            while (zl > 0)
            {
                var zn = m_SearchSpace[x, y, zl - 1];
                if (zn != null && current.UserContext.IsLevelChange(inUserContext))
                {
                    if (zn.UserContext.IsLevelChange(inUserContext))
                    {
                        neighbors[8] = zn;
                        break;
                    }
                }
                zl--;
            }

            // Check if there are any neighbors on higher levels
            zl = z;
            neighbors[9] = null;
            while (zl < Depth - 1)
            {
                var zn = m_SearchSpace[x, y, zl + 1];
                if (zn != null && current.UserContext.IsLevelChange(inUserContext))
                {
                    if (zn.UserContext.IsLevelChange(inUserContext))
                    {
                        neighbors[9] = zn;
                        break;
                    }
                }
                zl++;
            }
        }

        private class OpenCloseMap
        {
            private PathNode[,,] m_Map;
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Depth { get; private set; }
            public int Count { get; private set; }

            public PathNode this[Int32 x, Int32 y, Int32 z]
            {
                get
                {
                    return m_Map[x, y, z];
                }
            }

            public PathNode this[PathNode Node]
            {
                get
                {
                    return m_Map[Node.X, Node.Y, Node.Z];
                }

            }

            public bool IsEmpty
            {
                get
                {
                    return Count == 0;
                }
            }

            public OpenCloseMap(int inWidth, int inHeight, int inDepth)
            {
                m_Map = new PathNode[inWidth, inHeight, inDepth];
                Width = inWidth;
                Height = inHeight;
                Depth = inDepth;
            }

            public void Add(PathNode inValue)
            {
                PathNode item = m_Map[inValue.X, inValue.Y, inValue.Z];

#if DEBUG
                if (item != null)
                    throw new ApplicationException();
#endif

                Count++;
                m_Map[inValue.X, inValue.Y, inValue.Z] = inValue;
            }

            public bool Contains(PathNode inValue)
            {
                PathNode item = m_Map[inValue.X, inValue.Y, inValue.Z];

                if (item == null)
                    return false;

#if DEBUG
                if (!inValue.Equals(item))
                    throw new ApplicationException();
#endif

                return true;
            }

            public void Remove(PathNode inValue)
            {
                PathNode item = m_Map[inValue.X, inValue.Y, inValue.Z];

#if DEBUG
                if (!inValue.Equals(item))
                    throw new ApplicationException();
#endif

                Count--;
                m_Map[inValue.X, inValue.Y, inValue.Z] = null;
            }

            public void Clear()
            {
                Count = 0;

                for (int z = 0; z < Depth; z++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            m_Map[x, y, z] = null;
                        }
                    }
                }
            }
        }
    }

    public class MyPathNode : IPathNode<Object>
    {
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
        public Int32 Z { get; set; }
        public Boolean IsWall { get; set; }
        public Boolean IsStairs { get; set; }
        public AccessSide Access { get; set; }
        public BlockedSide[] BlockedSides { get; set; }

        public BlockedSide[] GetBlockedSides(Object unused)
        {
            return BlockedSides;
        }

        public AccessSide GetAccess(Object unused)
        {
            return Access;
        }

        public bool IsWalkable(Object unused)
        {
            return !IsWall;
        }

        public bool IsLevelChange(Object unused)
        {
            return IsStairs;
        }
    }

    public class Point3D
    {
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
        public Int32 Z { get; set; }

        public Point3D(Int32 X, Int32 Y, Int32 Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }
}
