using System.IO;
using System.Windows.Media.Media3D;
using System.Linq;
using static LaquaiLib.Miscellaneous;

namespace LaquaiLib;

public static partial class Math
{
    public static class Topology
    {
        public class Node
        {
            public string Name { get; init; }

            public Node(string name) => Name = name;

            public override bool Equals(object? obj)
            {
                if (obj is null or not Node)
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                return this == (Node)obj;
            }

            public override int GetHashCode() => Name.GetHashCode();

            public static bool operator ==(Node left, Node right) => left.Name == right.Name;
            public static bool operator !=(Node left, Node right) => !(left == right);

        }

        public class NodeGrid
        {
            private readonly List<Node> Nodes = new();
            private readonly List<List<double>> Grid = new();

            public NodeGrid(params Node[] nodes)
            {
                Nodes = nodes.ToList();
                foreach (Node node in Nodes)
                {
                    List<Node> remaining = new(Nodes);
                    remaining.Remove(node);
                    if (remaining.Any(rem => rem == node))
                    {
                        throw new ArgumentException("Attemped to add nodes with equal name.");
                    }
                }

                for (int i = 0; i < Nodes.Count; i++)
                {
                    Grid.Add(new());
                    for (int j = 0; j < Nodes.Count; j++)
                    {
                        Grid[i].Add(0);
                    }
                }
            }

            public NodeGrid(IEnumerable<Node> nodes)
            {
                Nodes = nodes.ToList();

                for (int i = 0; i < Nodes.Count; i++)
                {
                    Grid.Add(new());
                    for (int j = 0; j < Nodes.Count; j++)
                    {
                        Grid[i].Add(0);
                    }
                }
            }

            public void SetWeight(int node1, int node2, double weight, bool bidirectional = true)
            {
                Grid[node1][node2] = weight;
                if (bidirectional)
                {
                    Grid[node2][node1] = weight;
                }
            }

            public void SetWeight(Node node1, Node node2, double weight, bool bidirectional = true) => SetWeight(Nodes.IndexOf(node1), Nodes.IndexOf(node2), weight, bidirectional);
            public void SetWeight(string node1, string node2, double weight, bool bidirectional = true) => SetWeight(Nodes.IndexOf(Nodes.Where(node => node.Name == node1).First()), Nodes.IndexOf(Nodes.Where(node => node.Name == node2).First()), weight, bidirectional);

            public double GetWeight(int node1, int node2) => Grid[node1][node2];
            public double GetWeight(Node node1, Node node2) => GetWeight(Nodes.IndexOf(node1), Nodes.IndexOf(node2));
            public double GetWeight(string node1, string node2) => GetWeight(Nodes.IndexOf(Nodes.Where(node => node.Name == node1).First()), Nodes.IndexOf(Nodes.Where(node => node.Name == node2).First()));

            public IEnumerable<int> GetNeighbors(int node) => Nodes.Where(n => Grid[node][Nodes.IndexOf(n)] > 0 || Grid[node][Nodes.IndexOf(n)] > 0).Select(n => Nodes.IndexOf(n));
            public IEnumerable<int> GetNeighbors(Node node) => GetNeighbors(Nodes.IndexOf(node));
            public IEnumerable<int> GetNeighbors(string node) => GetNeighbors(Nodes.IndexOf(Nodes.Where(n => n.Name == node).First()));

            public (double Total, List<int> Path) GetPath(int start, int end)
            {
                List<int> path = new();

                List<double> weights = new();
                List<int> prevNodes = new();

                Dijkstra(start, weights, prevNodes);

                if (weights.Count == 0 || prevNodes.Count == 0)
                {
                    throw new Exception($"Dijkstra algorithm failed to return valid lists.");
                }

                int u = end;
                path.Insert(0, u);

                while (prevNodes[u] != -1)
                {
                    u = prevNodes[u];
                    path.Insert(0, u);
                }

                return (weights[path.Last()], path);
            }
            public (double Total, List<int> Path) GetPath(Node start, Node end) => GetPath(Nodes.IndexOf(start), Nodes.IndexOf(end));
            public (double Total, List<int> Path) GetPath(string start, string end) => GetPath(Nodes.IndexOf(Nodes.Where(node => node.Name == start).First()), Nodes.IndexOf(Nodes.Where(node => node.Name == end).First()));

            private void Dijkstra(int start, List<double> weights, List<int>? prevNodes)
            {
                int u;

                List<int> Q = new(0.Repeat(Nodes.Count).Cast<int>().ToArray());
                for (int i = 0; i < Nodes.Count; i++)
                {
                    Q[i] = i;
                }

                for (int i = 0; i < Nodes.Count; i++)
                {
                    weights.Insert(i, int.MaxValue);
                }
                weights[start] = 0;

                if (prevNodes is not null)
                {
                    for (int i = 0; i < Nodes.Count; i++)
                    {
                        prevNodes.Insert(i, -1);
                    }
                }

                while (Q.Count > 0)
                {
                    u = int.MaxValue;

                    foreach (int v in Q)
                    {
                        if (weights[v] < u)
                        {
                            u = v;
                        }
                    }

                    Q.RemoveAt(Q.LastIndexOf(u));

                    List<int> neighbors = GetNeighbors(u).ToList();
                    if (neighbors.Count == 0)
                    {
                        throw new Exception($"Start node {u} has no neighbor nodes.");
                    }

                    foreach (int v in neighbors)
                    {
                        if (Q.Contains(v))
                        {
                            double alt = weights[u] + Grid[u][v];
                            if (alt < weights[v])
                            {
                                weights[v] = alt;
                                if (prevNodes is not null)
                                {
                                    prevNodes[v] = u;
                                }
                            }
                        }
                    }
                }
            }

            // https://www.geeksforgeeks.org/traveling-salesman-problem-tsp-implementation/

            public (double Total, List<int> Path) Bus()
            {
                return (0, new());
            }

            public (double Total, List<int> Path) Ring()
            {
                return (0, new());
            }

            public double Star(int start)
            {
                List<double> weights = new();
                Dijkstra(start, weights, null);

                return weights.Sum();
            }
            public double Star(Node start) => Star(Nodes.IndexOf(start));
            public double Star(string start) => Star(Nodes.IndexOf(Nodes.Where(node => node.Name == start).First()));

            public double FullMesh()
            {
                double total = 0;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    List<double> weights = new();
                    Dijkstra(i, weights, null);

                    weights = weights.Skip(i).ToList();

                    total += weights.Sum();
                }
                return total;
            }
        }
    }
}