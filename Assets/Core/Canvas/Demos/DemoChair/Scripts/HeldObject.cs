using UnityEngine;
using System.Collections;

public class HeldObject : MonoBehaviour 
{
    private Transform target = null;
    public Vector3 tOffset;
    public Quaternion rOffset;
    public bool isEdible = false;

    private bool offset;

    void Awake()
    {
       // this.ResetOffset();
    }

	void Update ()
    {
        if (target != null)
        {
            transform.position = this.target.position + (offset ? (this.target.rotation * this.tOffset) : Vector3.zero);
            transform.rotation =
                this.target.rotation * Quaternion.Inverse(this.rOffset);
        }
	}

    public void ResetOffset()
    {
        this.tOffset = Vector3.zero;
        this.rOffset = Quaternion.identity;
    }

    /*public void Attach(Transform target, bool offset)
    {
        if (offset == true)
        {
            this.tOffset = transform.position - target.transform.position;
            this.tOffset =
                Quaternion.Inverse(target.transform.rotation) * this.tOffset;
            this.rOffset = target.transform.rotation * transform.rotation;
        }
        this.target = target;
    }*/

    public void Attach(Transform target, bool offset)
    {
        this.offset = offset;
        /*if (offset == true)
        {
            this.tOffset = Vector3.zero;// -target.transform.position;
            this.tOffset =
                Quaternion.Inverse(target.transform.rotation) * this.tOffset;
            this.rOffset = target.transform.rotation * transform.rotation;
        }*/
        this.target = target;
    }
}
