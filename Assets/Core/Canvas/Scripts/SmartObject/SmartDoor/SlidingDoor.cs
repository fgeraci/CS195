using UnityEngine;
using TreeSharpPlus;

public class SlidingDoor : MonoBehaviour
{
    public bool Locked = false;

    public float OpenXOffset = -3.0f;
    public float Speed = 1.0f;
    public Transform Door = null;

    private Vector3 start;
    private Vector3 target;

    private Interpolator<Vector3> interpolator;

    private int count = 0;

    void Start()
    {
        this.start = this.Door.transform.localPosition;
        this.target =
            new Vector3(
                this.Door.transform.localPosition.x + this.OpenXOffset,
                this.Door.transform.localPosition.y,
                this.Door.transform.localPosition.z);
        this.interpolator =
            new Interpolator<Vector3>(start, target, Vector3.Lerp);

        if (this.Door.GetComponent<Renderer>() != null)
            this.Door.GetComponent<Renderer>().material = 
                new Material(this.Door.GetComponent<Renderer>().material);
    }

    void DetectedEnter(GameObject obj)
    {
        this.count++;
    }

    void DetectedExit(GameObject obj)
    {
        this.count--;
    }

    private void OpenDoor() { this.interpolator.ToMax(this.Speed); }
    private void CloseDoor() { this.interpolator.ToMin(this.Speed); }

    void Update()
    {
        this.Door.transform.localPosition = this.interpolator.Value;

        if (this.Locked == true)
        {
            if (this.Door.GetComponent<Renderer>() != null)
                this.Door.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            this.CloseDoor();
        }
        if (this.Locked == false)
        {
            if (this.Door.GetComponent<Renderer>() != null)
                this.Door.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
            if (this.count > 0)
                this.OpenDoor();
            else
                this.CloseDoor();
        }
    }
}
