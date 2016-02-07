using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ContextObject : MonoBehaviour 
{
    public bool HasEntries { get; private set; }
    public SmartObject ParentObject = null;
    public bool IsCharacter = false;

    private Highlight highlight;

    private bool show = false;
    private readonly float width = 150.0f;
    private readonly float height = 20.0f;
    private string[] entries = null;

    private Vector3 mousePosition = Vector3.zero;

    void Start()
    {
        if (MousePick.instance != null)
        {
            MousePick.instance.AddReceiver(this);
        }
        if (this.ParentObject == null)
            this.ParentObject = transform.parent.GetComponent<SmartObject>();
        this.highlight = this.ParentObject.GetComponent<Highlight>();
    }

    public bool HasMouse()
    {
        if (this.show == true)
        {
            Vector3 position =
                MousePick.instance.GetComponent<Camera>().WorldToScreenPoint(
                    this.mousePosition)
                + MousePick.instance.mouseOffset;
            float left = position.x;
            float top = Screen.height - position.y;

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.y = Screen.height - mousePosition.y;

            Rect rect = 
                new Rect(
                    left, top, 
                    this.width, this.entries.Length * this.height);
            return rect.Contains(mousePosition);
        }
        return false;
    }

    void OnDeactivate()
    {
        if (this.show == true)
            MousePick.instance.PlayDeny();
        this.show = false;
    }

    void OnActivate(Vector3 position)
    {
        SmartObject user = MousePick.instance.CurrentUser;
        if (user != null)
        {
            this.show = true;
            this.mousePosition = position;
            List<string> affordances =
                new List<string>(
                    this.ParentObject.GetAffordances(user));
            if (affordances.Count > 0)
            {
                this.entries = affordances.ToArray();
                this.HasEntries = true;
            }
            else
            {
                this.entries = new string[] { "(No Available Actions)" };
                this.HasEntries = false;
            }
            MousePick.instance.PlayPick();
        }
    }

    void OnSelect()
    {
        if (this.IsCharacter == true)
        {
            MousePick.instance.Select(this.ParentObject);
            if (this.highlight != null)
                this.highlight.SelectOn();
        }
    }

    void OnDeselect()
    {
        if (this.highlight != null)
            this.highlight.SelectOff();
    }

    private void OnClick(string name)
    {
        MousePick.instance.OnAffordance(this, name);
        MousePick.instance.PlayActivate();
        this.show = false;
    }

    void OnGUI()
    {
        if (this.show == true && this.entries != null)
        {
            Vector3 position =
                MousePick.instance.GetComponent<Camera>().WorldToScreenPoint(
                    this.mousePosition)
                + MousePick.instance.mouseOffset;
            float left = position.x;
            float top = Screen.height - position.y;
            for (int i = 0; i < entries.Length; i++)
            {
                if (GUI.Button(new Rect(left, top, this.width, this.height),
                    entries[i]) == true)
                    this.OnClick(entries[i]);
                top += this.height;
            }
        }
    }
}
