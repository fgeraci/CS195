using UnityEngine;
using System.Collections;

/// <summary>
/// A holder for props (objects that can be held by SmartObjects).
/// </summary>
public class PropHolder : MonoBehaviour 
{
    [System.Serializable]
    public class PropAttachment
    {
        public PropType PropType;
        public Transform SnapPoint;
    }

    /// <summary>
    /// The current prop
    /// </summary>
    public Prop CurrentProp;

    /// <summary>
    /// Which prop types can be attached, and how?
    /// </summary>
    public PropAttachment[] Attachments;

    private Transform current = null;

    void Awake()
    {
        if (this.CurrentProp != null)
            this.Attach(this.CurrentProp);
    }

    void Update()
    {
        if (this.CurrentProp != null)
        {
            this.CurrentProp.transform.position = 
                this.current.transform.position;
            this.CurrentProp.transform.rotation = 
                this.current.transform.rotation;
        }
    }

    /// <summary>
    /// Attach the given prop. If a prop already exists, it is replaced.
    /// </summary>
    public void Attach(Prop prop)
    {
        this.CurrentProp = prop;
        this.current = this.transform;

        foreach (PropAttachment attachment in this.Attachments)
        {
            if (prop.PropType == attachment.PropType)
                this.current = attachment.SnapPoint;
        }
    }

    /// <summary>
    /// Release the current prop.
    /// </summary>
    /// <returns>A reference to the released prop.</returns>
    public Prop Release()
    {
        Prop temp = this.CurrentProp;
        this.CurrentProp = null;
        this.current = null;
        return temp;
    }
}