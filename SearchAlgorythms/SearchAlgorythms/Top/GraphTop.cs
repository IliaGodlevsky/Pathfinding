﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SearchAlgorythms.Top
{
    public class GraphTop : Button
    {
        private List<GraphTop> neighbours = new List<GraphTop>();
        public GraphTop()
        {
            IsStart = false;
            IsEnd = false;
            IsVisited = false;           
        }
        public bool IsStart { get; set; }
        public bool IsEnd { get; set; }
        public bool IsVisited { get; set; }
        public void AddNeighbour(GraphTop top)
        {
            neighbours.Add(top);
        }
        public List<GraphTop> GetNeighbours()
        {
            return neighbours;
        }
    }
}
