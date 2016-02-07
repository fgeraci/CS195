using UnityEngine;
using System.Collections;

public class FocusLight : MonoBehaviour 
{
    public Transform Target = null;
    public bool LightOn = false;

	void Update () 
    {
        if (this.Target != null)
        {
            this.transform.position = new Vector3(
                this.Target.transform.position.x,
                this.transform.position.y,
                this.Target.transform.position.z);
        }
        this.GetComponent<Light>().enabled = this.LightOn;
	}
}
