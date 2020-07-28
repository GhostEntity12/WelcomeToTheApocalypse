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
        public static List<Node> GetTilesWithinRadius(int radius, Node startNode, bool canSelectObstacles = false, bool resetsTiles = true, bool resetsColor = true)
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
                    foreach (Node tile in n.adjacentNodes)
                    {
                        if (!tile.visited)
                        {
							tile.tile.SetActive(true);
                            tile.parentNode = n;
                            tile.visited = true;
                            tile.distance = 1 + n.distance;
                            process.Enqueue(tile);
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
