using System.Collections.Generic;
using System.Linq;

namespace Ghost
{
    /// <summary>
    /// Old BFS implementation. Shouldn't be used - call Grid.m_Instance.GetNodesWithinRadius() instead.
    /// </summary>
    public static class BFS
    {
        /// <summary>
        /// Returns and highlights an area based on a radius and start node. Uses BFS.
        /// </summary>
        /// <param name="radius">How large the area to return is</param>
        /// <param name="startNode">The center node</param>
        /// <returns>The list of nodes to return</returns>
        public static List<Node> GetNodesWithinRadius(int radius, Node startNode, bool allowBlockedNodes = false)
        {
            List<Node> nodesInRadius = new List<Node>();

            List<Node> nodesToReset = new List<Node>();

            //Breadth First Search
            Queue<Node> process = new Queue<Node>();

            process.Enqueue(startNode);
            startNode.visited = true;

            while (process.Count > 0)
            {
                Node n = process.Dequeue();

                if (!n.m_isOnMap) continue;

                if (n != startNode && !allowBlockedNodes && n.m_isBlocked) continue;


                nodesInRadius.Add(n);

                if (n.distance < radius)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Node adjacentNode = n.adjacentNodes[i];
                        if (adjacentNode == null) continue;

                        if (!adjacentNode.visited)
                        {
                            adjacentNode.parentNode = n;
                            adjacentNode.visited = true;
                            adjacentNode.distance = 1 + n.distance;
                            process.Enqueue(adjacentNode);
                            nodesToReset.Add(adjacentNode);
                        }
                    }
                }
            }

            nodesToReset.AddRange(nodesInRadius);

            foreach (Node node in nodesToReset.Distinct().Where(n => n.m_isOnMap))
            {
                node.Reset();
            }

            return nodesInRadius;
        }
    }
}
