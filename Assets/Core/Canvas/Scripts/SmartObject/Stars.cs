using UnityEngine;
using System.Collections;

public class Stars : MonoBehaviour 
{
    public float Rate = 20.0f;
    public float Offset = 0.3f;
    public Transform Target;

	void Update() 
    {
        transform.position = Target.position + (this.Offset * Vector3.up);
        transform.Rotate(Vector3.up, Time.deltaTime * Rate);
	}
}
