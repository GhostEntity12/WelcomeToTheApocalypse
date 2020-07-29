using System.Collections.Generic;

namespace Ghost
{
    public static class BFS
    {
        /// <summary>
        /// Returns and highlights an area based on a radius and start node. Uses BFS.
        /// </summary>
        /// <param name="radius">How large the area to return is</param>
        /// <param name="startNode">The center node</param>
        /// <returns>The list of nodes to return</returns>
        public static List<Node> GetNodesWithinRadius(int radius, Node startNode, bool canSelectObstacles = false)
        {
            List<Node> nodesInRadius = new List<Node>();

            //Breadth First Search
            Queue<Node> process = new Queue<Node>();

            process.Enqueue(startNode);
            startNode.visited = true;

            while (process.Count > 0)
            {
                Node n = process.Dequeue();

                if (!n.isWalkable && !canSelectObstacles) continue;

                nodesInRadius.Add(n);

                if (n.distance < radius)
                {
                    foreach (Node node in n.adjacentNodes)
                    {
                        if (!node.visited)
                        {
                            node.parentNode = n;
                            node.visited = true;
                            node.distance = 1 + n.distance;
                            process.Enqueue(node);
                        }
                    }
                }
            }

            foreach (Node node in nodesInRadius)
            {
                node.Reset();
            }

            return nodesInRadius;
        }
    }
}
