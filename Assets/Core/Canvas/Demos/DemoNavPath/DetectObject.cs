using UnityEngine;
using System.Collections;

public class DetectObject : MonoBehaviour 
{
    public GameObject Target = null;
    public LayerMask Mask;

    void OnTriggerEnter(Collider col)
    {
        if ((this.Mask.value & (1 << col.gameObject.layer)) != 0)
        {
            this.Target.SendMessage("DetectedEnter", col.gameObject);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if ((this.Mask.value & (1 << col.gameObject.layer)) != 0)
        {
            this.Target.SendMessage("DetectedExit", col.gameObject);
        }
    }
}
