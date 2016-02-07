using UnityEngine;
using System.Collections;

public class SelectWallFade : MonoBehaviour 
{
    public static SelectWallFade Instance { get; private set; }

    public Transform[] FadeTargets;
    public LayerMask Mask;

    void Awake()
    {
        Instance = this;
    }

	void Update () 
    {
	    foreach (Transform target in this.FadeTargets)
        {
            if (target != null)
            {
                float distance;
                RaycastHit[] hits = this.DoRaycast(target, out distance);
                foreach (RaycastHit hit in hits)
                {
                    //Vector3 delta = hit.point - transform.position;
                    //if (delta.magnitude > distance)
                    //    break;

                    FadeWall fade = hit.collider.GetComponent<FadeWall>();
                    if (fade != null)
                        fade.FadeOut();
                }
            }
        }
	}

    private RaycastHit[] DoRaycast(
        Transform target,
        out float distance)
    {
        Vector3 direction = 
            target.transform.position - transform.position;
        distance = direction.magnitude;
        return Physics.RaycastAll(
            transform.position,
            direction,
            distance,
            this.Mask);
    }
}
