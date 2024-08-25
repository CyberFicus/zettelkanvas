using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zettelkanvas
{
    internal class Canvas
    {
        public List<Node> nodes { get; set; }
        public List<Edge> edges { get; set; }

        public Canvas() { 
            nodes = new List<Node>();
            edges = new List<Edge>();
        }

        public void PrepareToSerialization()
        {
            foreach(var node in nodes)
            {
                node.X *= 600;
                node.Y *= 600;
            }
        }
    }
}
