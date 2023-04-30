using LaquaiLib.Extensions;

namespace LaquaiLib;

public static partial class RandomMath
{
    public static class Topology
    {
        public class Node
        {
            public string Name
            {
                get; init;
            }

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
            private readonly List<Node> _nodes = new();
            private readonly List<List<double>> _grid = new();

            public IReadOnlyList<Node> Nodes => _nodes.ToList();

            public NodeGrid(params Node[] nodes)
            {
                _nodes = nodes.ToList();
                foreach (var node in _nodes)
                {
                    List<Node> remaining = new(_nodes);
                    remaining.Remove(node);
                    if (remaining.Any(rem => rem == node))
                    {
                        throw new ArgumentException("Attemped to add nodes with equal name.");
                    }
                }

                for (var i = 0; i < _nodes.Count; i++)
                {
                    _grid.Add(new());
                    for (var j = 0; j < _nodes.Count; j++)
                    {
                        _grid[i].Add(0);
                    }
                }
            }
            public NodeGrid(IEnumerable<Node> nodes)
            {
                _nodes = nodes.ToList();

                for (var i = 0; i < _nodes.Count; i++)
                {
                    _grid.Add(new());
                    for (var j = 0; j < _nodes.Count; j++)
                    {
                        _grid[i].Add(0);
                    }
                }
            }

            public void SetWeight(int node1, int node2, double weight, bool bidirectional = true)
            {
                _grid[node1][node2] = weight;
                if (bidirectional)
                {
                    _grid[node2][node1] = weight;
                }
            }
            public void SetWeight(Node node1, Node node2, double weight, bool bidirectional = true) => SetWeight(_nodes.IndexOf(node1), _nodes.IndexOf(node2), weight, bidirectional);
            public void SetWeight(string node1, string node2, double weight, bool bidirectional = true) => SetWeight(_nodes.IndexOf(_nodes.Where(node => node.Name == node1).First()), _nodes.IndexOf(_nodes.Where(node => node.Name == node2).First()), weight, bidirectional);

            public double GetWeight(int node1, int node2) => _grid[node1][node2];
            public double GetWeight(Node node1, Node node2) => GetWeight(_nodes.IndexOf(node1), _nodes.IndexOf(node2));
            public double GetWeight(string node1, string node2) => GetWeight(_nodes.IndexOf(_nodes.Where(node => node.Name == node1).First()), _nodes.IndexOf(_nodes.Where(node => node.Name == node2).First()));

            public IEnumerable<int> GetNeighbors(int node) => _nodes.Where(n => _grid[node][_nodes.IndexOf(n)] > 0).Select(n => _nodes.IndexOf(n));
            public IEnumerable<int> GetNeighbors(Node node) => GetNeighbors(_nodes.IndexOf(node));
            public IEnumerable<int> GetNeighbors(string node) => GetNeighbors(_nodes.IndexOf(_nodes.Where(n => n.Name == node).First()));

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

                var u = end;
                path.Insert(0, u);

                while (prevNodes[u] != -1)
                {
                    u = prevNodes[u];
                    path.Insert(0, u);
                }

                return (weights[path.Last()], path);
            }
            public (double Total, List<int> Path) GetPath(Node start, Node end) => GetPath(_nodes.IndexOf(start), _nodes.IndexOf(end));
            public (double Total, List<int> Path) GetPath(string start, string end) => GetPath(_nodes.IndexOf(_nodes.Where(node => node.Name == start).First()), _nodes.IndexOf(_nodes.Where(node => node.Name == end).First()));

            private void Dijkstra(int start, List<double> weights, List<int>? prevNodes)
            {
                int u;

                var Q = Enumerable.Repeat(0, _nodes.Count).ToList();
                for (var i = 0; i < _nodes.Count; i++)
                {
                    Q[i] = i;
                }

                for (var i = 0; i < _nodes.Count; i++)
                {
                    weights.Insert(i, int.MaxValue);
                }
                weights[start] = 0;

                if (prevNodes is not null)
                {
                    for (var i = 0; i < _nodes.Count; i++)
                    {
                        prevNodes.Insert(i, -1);
                    }
                }

                while (Q.Count > 0)
                {
                    u = int.MaxValue;

                    foreach (var v in Q)
                    {
                        if (weights[v] < u)
                        {
                            u = v;
                        }
                    }

                    Q.RemoveAt(Q.LastIndexOf(u));

                    var neighbors = GetNeighbors(u).ToList();
                    if (neighbors.Count == 0)
                    {
                        throw new Exception($"Start node {u} has no neighbor nodes.");
                    }

                    foreach (var v in neighbors)
                    {
                        if (Q.Contains(v))
                        {
                            var alt = weights[u] + _grid[u][v];
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

            public (double Total, List<int> Path) TravellingSalesman(int start, bool returnHome)
            {
                static bool findNextPermutation(List<int> data)
                {
                    if (data.Count < 2)
                    {
                        return false;
                    }
                    var last = data.Count - 2;

                    while (last >= 0)
                    {
                        if (data[last] < data[last + 1])
                        {
                            break;
                        }
                        last--;
                    }

                    if (last < 0)
                    {
                        return false;
                    }
                    var next = data.Count - 1;

                    for (var i = data.Count - 1; i > last; i--)
                    {
                        if (data[i] > data[last])
                        {
                            next = i;
                            break;
                        }
                    }

                    (data[next], data[last]) = (data[last], data[next]);
                    data.Reverse(last + 1, (data.Count - 1) - (last + 1) + 1);

                    return true;
                }

                var nodes = Enumerable.Range(0, _nodes.Count).ToList();
                if (!returnHome)
                {
                    nodes.Remove(start);
                }

                List<int> visitednodes = new();
                double min = int.MaxValue;
                do
                {
                    double current = 0;
                    var k = start;

                    for (var i = 0; i < nodes.Count; i++)
                    {
                        current += _grid[k][nodes[i]];
                        k = nodes[i];
                    }

                    current += _grid[k][start];
                    if (current < min)
                    {
                        min = current;
                        visitednodes = nodes.ToList();
                        visitednodes.Insert(0, start);
                    }
                } while (findNextPermutation(nodes));

                return (min, visitednodes);
            }
            public (double Total, List<int> Path) TravellingSalesman(Node start, bool returnHome) => TravellingSalesman(_nodes.IndexOf(start), returnHome);
            public (double Total, List<int> Path) TravellingSalesman(string start, bool returnHome) => TravellingSalesman(_nodes.IndexOf(_nodes.Where(node => node.Name == start).First()), returnHome);

            // https://www.geeksforgeeks.org/traveling-salesman-problem-tsp-implementation/

            public (double Total, List<int> Path) Bus()
            {
                List<(double, List<int>)> possible = new();
                for (var i = 0; i < _nodes.Count; i++)
                {
                    possible.Add(TravellingSalesman(i, false));
                }
                return possible.MinBy(possible => possible.Item1);
            }

            public (double Total, List<int> Path) Ring()
            {
                List<(double, List<int>)> possible = new();
                for (var i = 0; i < _nodes.Count; i++)
                {
                    possible.Add(TravellingSalesman(i, false));
                }
                return possible.MinBy(possible => possible.Item1);
            }

            public double Star(int start)
            {
                List<double> weights = new();
                Dijkstra(start, weights, null);

                return weights.Sum();
            }
            public double Star(Node start) => Star(_nodes.IndexOf(start));
            public double Star(string start) => Star(_nodes.IndexOf(_nodes.Where(node => node.Name == start).First()));

            public (double Total, int CableCount) FullMesh()
            {
                double total = 0;
                for (var i = 0; i < _nodes.Count; i++)
                {
                    List<double> weights = new();
                    Dijkstra(i, weights, null);

                    weights = weights.Skip(i).ToList();

                    total += weights.Sum();
                }
                return (total, CablesFromNodes(_nodes.Count));
            }

            public static int CablesFromNodes(int nodes) => nodes * (nodes - 1) / 2;

            public static int NodesFromCables(int cables)
            {
                try
                {
                    return (int)((1 + Math.Sqrt(1 + 8 * cables)) / 2);
                }
                catch
                {
                    try
                    {
                        return (int)((1 - Math.Sqrt(1 + 8 * cables)) / 2);
                    }
                    catch
                    {
                        return -1;
                    }
                }
            }
        }
    }
}
