using System;
using System.Collections.Generic;

namespace DSDLogitSUE
{
    class Node
    {
        double distance;
        int id;
        double xcord, ycord;
        string name;
        List<double> weights;
        List<Node> adjacency;

        public Node()
        {

        }
        public Node(double distance, int id, int xcord, int ycord, string name)
        {
            this.distance = distance;
            this.id = id;
            this.xcord = xcord;
            this.ycord = ycord;
            this.name = name;
            weights = new List<double>();
            adjacency = new List<Node>();
        }

        public double Distance { get { return distance; } set { distance = value; } }
        public int Id { get { return id; } set { id = value; } }
        public double Xcord { get { return xcord; } set { xcord = value; } }
        public double Ycord { get { return ycord; } set { ycord = value; } }
        public string Name { get { return name; } set { name = value; } }
        public List<Node> Adjacency { get { return adjacency; } set { adjacency = value; } }
        public List<double> Weights { get { return weights; } set { weights = value; } }

    }
}
