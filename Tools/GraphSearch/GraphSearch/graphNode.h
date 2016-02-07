#ifndef GRAPHNODE_H
#define GRAPHNODE_H

#define TOTAL_VERTEX 12992

struct graphNode
{
	int idx;
	int val;
	int predIdx;
	int connectedNodesIdx[TOTAL_VERTEX-1];
	int paths[TOTAL_VERTEX-1][TOTAL_VERTEX-1];
};

#endif