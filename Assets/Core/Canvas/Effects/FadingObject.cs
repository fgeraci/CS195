using UnityEngine;
using System.Collections;

public class FadingObject : MonoBehaviour 
{
    public bool StartVisible = false;
    public float Delay = 0.5f;
    private Interpolator<float> fade = null;

    private void Initialize()
    {
        this.GetComponent<Renderer>().material = new Material(this.GetComponent<Renderer>().material);
        this.fade = new Interpolator<float>(0.0f, 1.0f, Mathf.Lerp);

        if (this.StartVisible == true)
            this.fade.ForceMax();
        else
            this.fade.ForceMin();
    }

    void Start()
    {
        if (this.fade == null)
            this.Initialize();
    }

    void Update()
    {
        this.UpdateFade();
    }

    protected void UpdateFade()
    {
        if (this.fade.State == InterpolationState.Min)
        {
            this.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            this.GetComponent<Renderer>().enabled = true;
            Color color = this.GetComponent<Renderer>().material.color;
            color.a = this.fade.Value;
            this.GetComponent<Renderer>().material.color = color;
        }
    }

    public void FadeOut()
    {
        if (this.fade == null)
            this.Initialize();
        this.fade.ToMin(this.Delay);
    }

    public void FadeIn()
    {
        if (this.fade == null)
            this.Initialize();
        this.fade.ToMax(this.Delay);
    }
}
