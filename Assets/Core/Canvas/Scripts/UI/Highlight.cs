using UnityEngine;
using System.Collections;

public class Highlight : MonoBehaviour 
{
    protected HighlightableObject ho;

    private bool highlighted;
    private bool selected;

    void Awake()
    {
        this.highlighted = this.selected = false;
        ho = gameObject.AddComponent<HighlightableObject>();
    }

    public void HighlightOn()
    {
        this.highlighted = true;
        if (this.selected == false)
            this.ho.ConstantOn(this.ComputeColor());
    }

    public void HighlightOff()
    {
        this.highlighted = false;
        if (this.selected == false)
            this.ho.ConstantOff();
    }

    public void SelectOn()
    {
        this.selected = true;
        this.ho.ConstantOnImmediate(this.ComputeColor());
    }

    public void SelectOff()
    {
        this.selected = false;
        if (this.highlighted == false)
            this.ho.ConstantOffImmediate();
        else
            this.ho.ConstantParams(this.ComputeColor());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            HighlightOn();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            HighlightOff();
        }
    }

    private Color ComputeColor()
    {
        if (this.selected == true)
            return Color.yellow;
        return Color.white;
    }
}
