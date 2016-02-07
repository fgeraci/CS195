using UnityEngine;
using System.Collections;

public class SetOrientation : MonoBehaviour 
{
    public float Delay = 0.5f;
    public Vector3 Euler;

    private float time;
    private bool done = false;

	// Use this for initialization
	void Start () 
    {
        this.time = Time.time + this.Delay;
        this.done = false;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (this.done == false && Time.time > this.time)
        {
            this.transform.rotation = Quaternion.Euler(this.Euler);
            this.done = true;
        }
	}
}
