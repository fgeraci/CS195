using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MirrorBones : MonoBehaviour 
{
    public Transform source;
    public Transform axis;
    public Transform destination;

	void Start () 
    {
        Dictionary<Transform,Transform> parents = new Dictionary<Transform,Transform>();
        List<Transform> sources = new List<Transform>();
        List<Transform> destinations = new List<Transform>();

        sources.Add(source);
        foreach (Transform child1 in source)
        {
            sources.Add(child1);
            foreach (Transform child2 in child1)
            {
                sources.Add(child2);
                foreach (Transform child3 in child2)
                {
                    sources.Add(child3);
                }
            }
        }

        destinations.Add(destination);
        foreach (Transform child1 in destination)
        {
            destinations.Add(child1);
            foreach (Transform child2 in child1)
            {
                destinations.Add(child2);
                foreach (Transform child3 in child2)
                {
                    destinations.Add(child3);
                }
            }
        }

        foreach (Transform t in destinations)
        {
            parents.Add(t, t.parent);
            t.parent = null;
        }

        for (int i = 0; i < sources.Count; i++)
        {
            Transform sourceT = sources[i];
            Transform destinationT = destinations[i];

            float xOffset = axis.transform.position.x - sourceT.position.x;
            destinationT.position = sourceT.position + new Vector3(xOffset * 2.0f, 0.0f, 0.0f);

            destinationT.Rotate(axis.right, 180);
        }

        foreach (var kv in parents)
        {
            kv.Key.parent = kv.Value;
        }
	}
}
