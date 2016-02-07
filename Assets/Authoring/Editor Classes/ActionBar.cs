using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// An ActionBar consists of toggles and buttons which perform some action when clicked/toggled.
/// The toggles can also be organized as radio buttons (i.e. only one can be selected at once).
/// </summary>
public class ActionBar
{
    /// <summary>
    /// Compares the content of two GUI elements simply by their text (ignoring tooltip/image).
    /// </summary>
    private class ContentComparer : IEqualityComparer<GUIContent>
    {
        public bool Equals(GUIContent x, GUIContent y)
        {
            return x.text == y.text;
        }

        public int GetHashCode(GUIContent obj)
        {
            return obj.text.GetHashCode();
        }
    }

    //Are the toggles radio buttons?
    private bool radioButtons;

    //Should the bar be scrollable?
    private bool scrollable;

    //The current scroll position, if it is scrollable
    private Vector2 scrollPosition;

    //Should the bar be displayed horizontally?
    private bool isHorizontal;

    //The GUI elements that are currently inactive
    private HashSet<GUIContent> inactive = new HashSet<GUIContent>(new ContentComparer());

    //The actions to execute when buttons are clicked
    private Dictionary<GUIContent, Action> buttonActions = new Dictionary<GUIContent, Action>(new ContentComparer());

    //The actions to execute when toggles are toggled/untoggled and the current toggle values
    private Dictionary<GUIContent, Tuple<bool, Action<bool>>> toggleActions = new Dictionary<GUIContent, Tuple<bool, Action<bool>>>(new ContentComparer());

    /// <summary>
    /// Create a new ActionBar.
    /// </summary>
    /// <param name="isHorizontal">Is the bar displayed horizontally?</param>
    /// <param name="checkboxesAreRadioButtons">Should toggles be radio buttons?</param>
    /// <param name="isScrollable">Should the bar be scrollable?</param>
    public ActionBar(bool isHorizontal, bool checkboxesAreRadioButtons = false, bool isScrollable = true)
    {
        this.isHorizontal = isHorizontal;
        this.radioButtons = checkboxesAreRadioButtons;
        this.scrollable = isScrollable;
    }

    /// <summary>
    /// Render the action bar.
    /// </summary>
    /// <param name="enabled">Should the buttons/toggles be clickable?</param>
    /// <param name="displayDisabled">Should we display (grey out) disabled buttons/toggles or hide them?</param>
    public void Render(bool enabled, bool displayDisabled = true)
    {
        bool oldEnabled = GUI.enabled;
        GUI.enabled = enabled;
        if (scrollable)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        }
        Action buttonAction = null;
        Action<bool> toggleAction = null;
        bool argument = false;
        if (isHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        //Render all the buttons, check if any is clicked.
        foreach (GUIContent key in buttonActions.Keys)
        {
            if (inactive.Contains(key))
            {
                if (displayDisabled)
                {
                    GUI.enabled = false;
                }
                else
                {
                    continue;
                }
            }
            if (GUILayout.Button(key))
            {
                buttonAction = buttonActions[key];
            }
            GUI.enabled = enabled;
        }

        //Render all the toggles, check if any is clicked.
        foreach (GUIContent key in toggleActions.Keys)
        {
            if (inactive.Contains(key))
            {
                GUI.enabled = false;
            }
            bool newValue = GUILayout.Toggle(toggleActions[key].Item1, key);
            if (newValue != toggleActions[key].Item1)
            {
                toggleActions[key].Item1 = newValue;
                toggleAction = toggleActions[key].Item2;
                argument = toggleActions[key].Item1;
            }
            //if the toggles are supposed to be radio buttons, this makes sure that all
            //others are set to false, when one is set to true
            if (newValue == true && radioButtons)
            {
                foreach (Tuple<bool, Action<bool>> tuple in toggleActions.Values)
                {
                    if (tuple != toggleActions[key])
                    {
                        tuple.Item1 = false;
                    }
                }
            }
            GUI.enabled = enabled;
        }

        if (isHorizontal)
        {
            GUILayout.EndHorizontal();
        }
        if (scrollable)
        {
            GUILayout.EndScrollView();
        }
        //Execute the toggle/button action if any of them was clicked.
        if (buttonAction != null)
        {
            buttonAction.Invoke();
        }
        if (toggleAction != null)
        {
            toggleAction.Invoke(argument);
        }
        GUI.enabled = oldEnabled;
    }

    /// <summary>
    /// Adds a new button with the given content, and the action to be executed on click.
    /// If a button with the same text already exists, it is overwritten.
    /// </summary>
    public void AddButton(string content, Action onClick)
    {
        this.AddButton(new GUIContent(content), onClick);
    }

    /// <summary>
    /// Adds a new button with the given content, and the action to be executed on click.
    /// If a button with the same text already exists, it is overwritten.
    /// </summary>
    public void AddButton(GUIContent content, Action onClick)
    {
        this.buttonActions[content] = onClick;
    }

    /// <summary>
    /// Adds a new toggle with the given content, and the action to be executed on toggle.
    /// Can also specify the start value of the toggle, which is false by default.
    /// If a toggle with the same text already exists, it is overwritten.
    /// </summary>
    public void AddToggle(string content, Action<bool> onToggle, bool startValue = false)
    {
        this.AddToggle(new GUIContent(content), onToggle, startValue);
    }

    /// <summary>
    /// Adds a new toggle with the given content, and the action to be executed on toggle.
    /// Can also specify the start value of the toggle, which is false by default.
    /// If a toggle with the same text already exists, it is overwritten.
    /// </summary>
    public void AddToggle(GUIContent content, Action<bool> onToggle, bool startValue = false)
    {
        this.toggleActions[content] = new Tuple<bool, Action<bool>>(startValue, onToggle);
    }

    /// <summary>
    /// Removes the button with the given content.
    /// </summary>
    public void RemoveButton(string content)
    {
        this.RemoveButton(new GUIContent(content));
    }

    /// <summary>
    /// Removes the button with the given content.
    /// </summary>
    public void RemoveButton(GUIContent content)
    {
        this.buttonActions.Remove(content);
    }

    /// <summary>
    /// Removes the toggle with the given content.
    /// </summary>
    public void RemoveToggle(string content)
    {
        this.RemoveToggle(new GUIContent(content));
    }

    /// <summary>
    /// Removes the toggle with the given content.
    /// </summary>
    public void RemoveToggle(GUIContent content)
    {
        this.toggleActions.Remove(content);
    }

    /// <summary>
    /// Sets the toggle or button with the given content to being active depending
    /// on the given boolean. Inactive buttons/toggles will appear greyed out.
    /// </summary>
    public void SetActive(string content, bool active)
    {
        this.SetActive(new GUIContent(content), active);
    }

    /// <summary>
    /// Sets the toggle or button with the given content to being active depending
    /// on the given boolean. Inactive buttons/toggles will appear greyed out.
    /// </summary>
    public void SetActive(GUIContent content, bool active)
    {
        if (active)
        {
            inactive.Remove(content);
        }
        else
        {
            inactive.Add(content);
        }
    }
}
