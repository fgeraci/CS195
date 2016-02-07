using UnityEngine;
using System.Collections;

public class FadeWall : MonoBehaviour 
{
    public Transform WallGroup;
    public float Delay = 0.5f;
    public float FadeInDelay = 5.0f;

    private Interpolator<float> fade;

    private float fadeOutTime = -1.0f;

    void Start()
    {
        // Clone all of the materials
        foreach (Transform child in WallGroup)
            child.GetComponent<Renderer>().material =
                new Material(child.GetComponent<Renderer>().material);

        this.fade = new Interpolator<float>(0.0f, 1.0f, Mathf.Lerp);
        this.fade.ForceMax();
    }

    void Update()
    {
        foreach (Transform child in WallGroup)
        {
            Color color = child.GetComponent<Renderer>().material.color;
            color.a = this.fade.Value;
            child.GetComponent<Renderer>().material.color = color;
        }

        if (Time.time > (this.fadeOutTime + this.FadeInDelay))
            this.FadeIn();
    }

    public void FadeOut()
    {
        this.fade.ToMin(this.Delay);
        this.fadeOutTime = Time.time;
    }

    public void FadeIn()
    {
        this.fade.ToMax(this.Delay);
    }
}
