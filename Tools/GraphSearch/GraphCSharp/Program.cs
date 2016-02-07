using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;



namespace GraphCSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct graphNode
    {

        public int idx;
        public int val;
        public int predIdx;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (GraphSearch.NUM_VERTEX - 1))]
        public int[] connectedNodexIdx;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ((GraphSearch.NUM_VERTEX - 1) * (GraphSearch.NUM_VERTEX - 1)))]
        public int[] paths;
    }

    
    public static class GraphSearch
    {
      public const int NUM_VERTEX = 12992;

        [DllImport("GraphSearch.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void add_node(int[] connectedNodes);

        [DllImport("GraphSearch.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void solve_graph();

        [DllImport("GraphSearch.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_node(int nodeIndex, out graphNode node);

        //static void Main(string[] args)
        //{
        //    int[] connected1 = new int[4] { 1, 2, 3, -1};
        //    int[] connected2 = new int[4] { 4, -1, -1, -1 };
        //    int[] connected3 = new int[4] { 4, -1, -1, -1 };
        //    int[] connected4 = new int[4] { 4, 2, -1, -1 };
        //    int[] connected5 = new int[4] { -1, -1, -1, -1 };

        //    add_node(connected1);
        //    add_node(connected2);
        //    add_node(connected3);
        //    add_node(connected4);
        //    add_node(connected5);

        //    solve_graph();

        //    graphNode node0 = new graphNode();
        //    graphNode node1 = new graphNode();
        //    graphNode node2 = new graphNode();
        //    graphNode node3 = new graphNode();
        //    graphNode node4 = new graphNode();

        //    get_node(0, out node0);
        //    get_node(1, out node1);
        //    get_node(2, out node2);
        //    get_node(3, out node3);
        //    get_node(4, out node4);
        //}
    }
}
