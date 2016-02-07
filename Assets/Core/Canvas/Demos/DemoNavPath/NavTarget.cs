using UnityEngine;
using System.Collections;

using TreeSharpPlus;

public class NavTarget : MonoBehaviour 
{
    public SmartCharacter Walker = null;
    public LayerMask mask;

    private Node goNode = null;

	// Update is called once per frame
	void Update () 
    {
        if (this.goNode != null)
        {
            RunStatus result = this.goNode.Tick();
            if (this.corners == null)
                this.TracePath();
            if (result == RunStatus.Success)
            {
                this.goNode = null;
                this.corners = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (this.goNode == null)
            {
                this.goNode = this.Walker.Node_GoTo(transform.position);
                this.goNode.Start();
            }
        }
	}





    private Vector3[] corners;

    private void TracePath()
    {
        NavMeshAgent nav = this.Walker.GetComponent<NavMeshAgent>();
        if (nav.hasPath == true)
        {
            NavMeshPath path = nav.path;
            this.corners = path.corners;

            for (int i = 0; i < this.corners.Length - 1; i++)
            {
                Vector3 cornerFrom = this.corners[i];
                Vector3 cornerTo = this.corners[i + 1];
                Vector3 direction = cornerTo - cornerFrom;

                RaycastHit[] hits =
                    Physics.RaycastAll(
                        cornerFrom,
                        direction,
                        direction.magnitude,
                        this.mask);
                
                Debug.Log(hits.Length);
            }
        }
        else
        {
            this.corners = null;
        }
    }

    void OnDrawGizmos()
    {
        if (this.corners != null)
        {
            foreach (Vector3 corner in this.corners)
                Gizmos.DrawSphere(corner, 1.0f);
        }
    }
}
