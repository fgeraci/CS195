#define BLOCK_SIZE 256
#include "graphNode.h"
#include <cstdlib>
#include <math.h>

__device__ int return_thread_index()
{
	int thread_idx_in_block = threadIdx.x + (threadIdx.y * blockDim.x);
	int block_idx_in_grid = blockIdx.x + (blockIdx.y * gridDim.x);
	return thread_idx_in_block + (block_idx_in_grid * blockDim.x);
}

__global__ void fillPath(graphNode* graph, int sourceIndex, int totalVertex)
{
	int threadIndex = return_thread_index();
	if (threadIndex != sourceIndex && threadIndex < totalVertex)
	{
		int vertex_location = (threadIndex > sourceIndex) ? graph[threadIndex].idx - 1 : graph[threadIndex].idx;
		int current_position = 0;
		int vertexIdx = graph[threadIndex].idx;
		while (vertexIdx != sourceIndex && graph[vertexIdx].val != -1)
		{  
			graph[sourceIndex].paths[vertex_location][current_position] = vertexIdx;
			vertexIdx = graph[vertexIdx].predIdx;
			current_position++;
		}
	}
}

//graph_dev1 -> read
//graph_dev2 -> write
__global__ void graphSearch(graphNode* graph_dev1, graphNode* graph_dev2, int totalVertex, int sourceIndex, int* flag_dev)
{
	int threadIndex = return_thread_index();

	if (threadIndex < totalVertex)
	{
		graphNode vertex = graph_dev1[threadIndex];
		
		if (vertex.val >= 0)
		{
			for (int uidIndex = 0; uidIndex < totalVertex; uidIndex++)
			{
				int neighborIdx = vertex.connectedNodesIdx[uidIndex];
				if (neighborIdx >= 0 && neighborIdx != sourceIndex)
				{
					int neighbor_predIdx = graph_dev1[neighborIdx].predIdx;
					if (neighbor_predIdx == -1) {
						graph_dev2[neighborIdx].val = vertex.val + 1;
						graph_dev2[neighborIdx].predIdx = vertex.idx;
						*flag_dev = 1;
					} else {
						graph_dev2[neighborIdx].val = graph_dev1[neighbor_predIdx].val + 1;
						graph_dev2[neighborIdx].predIdx = neighbor_predIdx;
					}
				}
			}
		}

	}
}


void init_values_for_search(graphNode* graph, int vertexIndex, int totalVertex)
{
	for(int i = 0; i < totalVertex; i++)
	{
		graph[i].val = (vertexIndex != i) ? -1 : 0;
	}
}

extern "C" graphNode* searchGraph(graphNode* graph, int totalVertex)
{
	int blockLength = (int)sqrt((double)BLOCK_SIZE); 
	int gridLength = (int)ceil((double)totalVertex / (double)BLOCK_SIZE);

	dim3 threads(blockLength, blockLength, 1);
	dim3 blocks(gridLength, gridLength, 1);

	graphNode *graph_dev1;
	graphNode *graph_dev2;
	graphNode *graph_out;
	graph_out = (graphNode*)malloc(totalVertex*sizeof(graphNode));
	cudaMalloc((void**)&graph_dev1, totalVertex*sizeof(graphNode));
	cudaMalloc((void**)&graph_dev2, totalVertex*sizeof(graphNode));

	int *flag, *flag_dev;
	cudaMalloc((void**)&flag_dev, sizeof(int));
	flag = (int*)malloc(sizeof(int));

	for (int vertexIndex = 0; vertexIndex < totalVertex; ++vertexIndex)
	{

		init_values_for_search(graph, vertexIndex, totalVertex);

		cudaMemcpy(graph_dev1, graph, totalVertex*sizeof(graphNode), cudaMemcpyHostToDevice);
		cudaMemcpy(graph_dev2, graph, totalVertex*sizeof(graphNode), cudaMemcpyHostToDevice);
		

		do {
			*flag = 0;
			cudaMemcpy(flag_dev, flag, sizeof(int), cudaMemcpyHostToDevice);
			
			graphSearch<<<blocks, threads>>>(graph_dev1, graph_dev2, totalVertex, vertexIndex, flag_dev);


			cudaMemcpy(flag, flag_dev, sizeof(int), cudaMemcpyDeviceToHost);
			
			graphNode* tmp = graph_dev1;
			graph_dev1 = graph_dev2;
			graph_dev2 = tmp;

		} while (*flag == 1);

		fillPath<<<blocks, threads>>>(graph_dev1, vertexIndex, totalVertex);
		cudaMemcpy(&graph_out[vertexIndex], &graph_dev1[vertexIndex], sizeof(graphNode), cudaMemcpyDeviceToHost);
	}

	return graph_out;
}