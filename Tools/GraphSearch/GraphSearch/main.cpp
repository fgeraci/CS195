
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include "graphNode.h"
#include <iostream>

using namespace std;
#define EXPORT __declspec(dllexport)

extern "C" graphNode* searchGraph(graphNode* graph, int totalVertex);	

int vertexIdx = 0;
graphNode graph[TOTAL_VERTEX];
graphNode* graph_out;

void print_graph(graphNode* graph)
{
	for (int i = 0; i < TOTAL_VERTEX; i++)
	{
		cout << "Node: " << graph[i].idx << endl;
		cout << "Path:" << endl;
		for (int j = 0; j < TOTAL_VERTEX-1; j++)
		{
			bool path = false;
			for (int k = 0; k < TOTAL_VERTEX-1; k++)
			{
				int id = graph[i].paths[j][k];
				if (id != - 1) 
				{
					path = true;
					cout << id << " --> ";
				}
			}
			if (path)
				cout << graph[i].idx << endl;
		}
		cout << endl;

	}
}

void init_node(int connectedNodesIds[], graphNode* node)
{
	node->idx = vertexIdx++;
	node->val = -1;
	node->predIdx = -1;

	for (int i = 0; i < TOTAL_VERTEX-1; i++)
	{
		node->connectedNodesIdx[i] = connectedNodesIds[i];
	}

	for (int i = 0; i < TOTAL_VERTEX-1; i++)
	{
		for (int j = 0; j < TOTAL_VERTEX-1; j++)
		{
			node->paths[i][j] = -1;
		}
	}

}

extern "C" EXPORT void add_node(int connectedNodes[])
{
	graphNode* node = (graphNode*)malloc(sizeof(graphNode));
	init_node(connectedNodes, node);
	graph[node->idx] = *node;
}

extern "C" EXPORT void solve_graph()
{
	graph_out = searchGraph(graph, TOTAL_VERTEX);
}

extern "C" EXPORT void get_node(int nodeIndex, graphNode* node)
{
	for (int i = 0; i < TOTAL_VERTEX-1; i++)
	{
		for (int j = 0; j < TOTAL_VERTEX-1; j++)
		{
			node->paths[i][j] = graph_out[nodeIndex].paths[i][j];
		}
	}
	
}

//Assumes uid correspond to index in array.
//Easier to search on GPU. Consider storing connectedNodeIndex instead of connectedNodeUids
void main()
{
	int connectedNodes0[TOTAL_VERTEX-1] = {1, 2, 3, -1};
	add_node(connectedNodes0);

	int connectedNodes1[TOTAL_VERTEX-1] = {4, -1, -1, -1};
	add_node(connectedNodes1);

	int connectedNodes2[TOTAL_VERTEX-1] = {4, -1, -1, -1};
	add_node(connectedNodes2);

	int connectedNodes3[TOTAL_VERTEX-1] = {2, -1, -1, -1};
	add_node(connectedNodes3);

	int connectedNodes4[TOTAL_VERTEX-1] = {-1, -1, -1, -1};
	add_node(connectedNodes4);

	graph_out = searchGraph(graph, TOTAL_VERTEX);

	print_graph(graph_out);

}

