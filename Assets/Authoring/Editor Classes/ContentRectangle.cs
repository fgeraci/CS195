using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// A ContentRectangle is essentially a button displaying some content for an element, and offering
/// actions to be executed when left clicked, right clicked or hovered.
/// </summary>
public abstract class ContentRectangle 
{
    //Whether we use GUILayout to render the button.
    public bool useGUILayout { get; protected set; }

    //The style to be used for rendering the button.
    public GUIStyle Style;

    //The button's position (applicable only if useGUILAyout is false)
    public Rect Position { get; protected set; }

    /// <summary>
    /// Set left and top part of the position.
    /// </summary>
    /// <param name="left">The new left edge.</param>
    /// <param name="top">The new top edge.</param>
    public void SetLeftAndTop(float left, float top)
    {
        Position = new Rect(left, top, Position.width, Position.height);
    }
    
    /// <summary>
    /// Set the position.
    /// </summary>
    public void SetPosition(Rect position)
    {
        this.Position = position;
    }

    /// <summary>
    /// Render the button.
    /// </summary>
    public abstract void Render();

    /// <summary>
    /// Get the displayed text.
    /// </summary>
    public abstract string GetText();

    /// <summary>
    /// Get the entire displayed content (i.e. text, tooltip and image).
    /// </summary>
    /// <returns></returns>
    public abstract GUIContent GetContent();

    /// <summary>
    /// Highlight the rectangle in a default color (NOTE: does not work with all styles).
    /// </summary>
    public abstract void Highlight();

    /// <summary>
    /// Highlight the rectangle in the given color (NOTE: does not work with all styles).
    /// </summary>
    /// <param name="color"></param>
    public abstract void Highlight(Color color);

    /// <summary>
    /// Unhighlights the rectangle, reverting its color to the last one used by Highlight if exists
    /// or entirely removing any highlighting else.
    /// </summary>
    public abstract void Unhighlight();
}

/// <summary>
/// Generic ContentRectangle version to support having various types of objects stored within.
/// </summary>
/// <typeparam name="T">The type of object to be stored.</typeparam>
public class ContentRectangle<T> : ContentRectangle
{
    //Whether content is static
    private bool isContentStatic;

    //As we use RepeatButton, this makes sure click only registers once
    private bool alreadyPressed;

    //The function to compute the content to display.
    private Func<T, GUIContent> contentFunction;

    //Currently displayed content
    private GUIContent content;

    //The texture of the button (changed on highlight)
    private Texture2D buttonTexture;

    //Are we highlighting?
    private bool highlighting;

    //Did we already adapt the texture for highlight?
    private bool textureChanged;

    //The target color for highlighting.
    private Color targetColor;

    //Stack of highlight colors, when calling Highlight multiple times.
    private Stack<Color> highlightColors = new Stack<Color>();

    //Is the mouse over the button?
    private bool mouseOver;

    //The contained object.
    public T containedObject { get; protected set; }

    //What happens on left click
    public HashSet<Action<T>> onLeftClick = new HashSet<Action<T>>();

    //What happens on right click
    public HashSet<Action<T>> onRightClick = new HashSet<Action<T>>();

    //What happens on mouse over (only if useGUILayout == false)
    public HashSet<Action<T>> onMouseOver = new HashSet<Action<T>>();

    //What happens on mouse out (only if useGUILayout == false)
    public HashSet<Action<T>> onMouseOut = new HashSet<Action<T>>();
    
    /// <summary>
    /// Create a ContentRectangle with the given object, using GUILayout and default ToString for content
    /// and dynamic content.
    /// </summary>
    /// <param name="containedObject">The contained object.</param>
    public ContentRectangle(T containedObject) 
        : this(containedObject, true, default(Rect)) { }

    /// <summary>
    /// Create a ContentRectangle with the given object, using default ToString for content
    /// and dynamic content.
    /// </summary>
    /// <param name="containedObject">The contained object.</param>
    /// <param name="useGUILayout">Use GUILayout for rendering?</param>
    /// <param name="position">The position (ignored if useGUILayout == true)</param>
    public ContentRectangle(T containedObject, bool useGUILayout, Rect position) 
        : this(containedObject, useGUILayout, position, false) { }

    /// <summary>
    /// Create a ContentRectangle with the given object, using default ToString for content.
    /// </summary>
    /// <param name="containedObject">The contained object.</param>
    /// <param name="useGUILayout">Use GUILayout for rendering?</param>
    /// <param name="position">The position (ignored if useGUILayout == true)</param>
    /// <param name="isContentStatic">Should content be static?</param>
    public ContentRectangle(T containedObject, bool useGUILayout, Rect position, bool isContentStatic) 
        : this(containedObject, useGUILayout, position, isContentStatic, (T obj) => new GUIContent(obj.ToString())) { }

    /// <summary>
    /// Create a ContentRectangle with the given object.
    /// </summary>
    /// <param name="containedObject">The contained object.</param>
    /// <param name="useGUILayout">Use GUILayout for rendering?</param>
    /// <param name="position">The position (ignored if useGUILayout == true)</param>
    /// <param name="isContentStatic">Should content be static?</param>
    /// <param name="contentFunction">The function to compute content</param>
    public ContentRectangle(T containedObject, bool useGUILayout, Rect position, bool isContentStatic, Func<T, GUIContent> contentFunction)
    {
        this.containedObject = containedObject;
        this.useGUILayout = useGUILayout;
        this.Position = position;
        this.isContentStatic = isContentStatic;
        this.contentFunction = contentFunction;
        this.content = contentFunction.Invoke(containedObject);
        this.mouseOver = false;

        Texture2D skinTexture = GUI.skin.button.normal.background;
        this.buttonTexture = new Texture2D(skinTexture.width, skinTexture.height, TextureFormat.ARGB32, false);
        this.buttonTexture.SetPixels(skinTexture.GetPixels());
        this.Style = GUI.skin.button;
    }

    /// <summary>
    /// Highlight with default color.
    /// </summary>
    public override void Highlight()
    {
        Highlight(Color.red);
    }

    /// <summary>
    /// Highlight with the given color.
    /// </summary>
    public override void Highlight(Color color)
    {
        if (this.targetColor != color)
        {
            this.targetColor = color;
            this.textureChanged = false;
            this.highlightColors.Push(color);
        }
    }

    /// <summary>
    /// Unhighlight the button.
    /// </summary>
    public override void Unhighlight()
    {
        if (this.highlightColors.Count > 0)
        {
            highlightColors.Pop();
            if (this.highlightColors.Count > 0)
            {
                this.targetColor = highlightColors.Peek();
                this.textureChanged = false;
                return;
            }
        }
        this.targetColor = new Color();
    }

    /// <summary>
    /// Render the button.
    /// </summary>
    public override void Render()
    {
        Texture2D oldNormal = Style.normal.background;
        Texture2D oldHover = Style.hover.background;

        if (highlightColors.Count > 0)
        {
            //highlights if needed. Makes sure new texture is only computed once per color.
            if (!textureChanged)
            {
                if (GUI.skin.button == Style)
                {
                    for (int i = 1; i < buttonTexture.width - 1; i++)
                    {
                        for (int j = 1; j < buttonTexture.height - 1; j++)
                        {
                            buttonTexture.SetPixel(i, j, this.targetColor);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < buttonTexture.width; i++)
                    {
                        for (int j = 0; j < buttonTexture.height; j++)
                        {
                            buttonTexture.SetPixel(i, j, this.targetColor);
                        }
                    }
                }
                buttonTexture.Apply();
                textureChanged = true;
            }
            Style.normal.background = buttonTexture;
            Style.hover.background = buttonTexture;
        }

        if (Event.current.type == EventType.mouseDown)
        {
            alreadyPressed = false;
        }
        bool pressed = false;
        if (useGUILayout)
        {
            pressed = GUILayout.RepeatButton(GetContent(), Style);
        }
        //if not using GUILAyout, also check mouseover, mouseout
        else
        {
            if (Position.Contains(Event.current.mousePosition))
            {
                mouseOver = true;
                foreach (Action<T> action in onMouseOver)
                {
                    action.Invoke(containedObject);
                }
            }
            else if (mouseOver)
            {
                mouseOver = false;
                foreach (Action<T> action in onMouseOut)
                {
                    action.Invoke(containedObject);
                }
            }
            pressed = GUI.RepeatButton(Position, GetContent(), Style);
        }
        //Check if we need to invoke a press event (left or right click)
        if (pressed && !alreadyPressed)
        {
            alreadyPressed = true;
            if (Event.current.button == 0)
            {
                foreach (Action<T> action in onLeftClick)
                {
                    action.Invoke(containedObject);
                }
            }
            else if (Event.current.button == 1)
            {
                foreach (Action<T> action in onRightClick)
                {
                    action.Invoke(containedObject);
                }
            }
        }

        Style.normal.background = oldNormal;
        Style.hover.background = oldHover;
    }

    /// <summary>
    /// Gets the entire content.
    /// </summary>
    public override GUIContent GetContent()
    {
        if (!isContentStatic)
        {
            content = contentFunction(containedObject);
        }
        return content;
    }

    /// <summary>
    /// Gets the displayed text.
    /// </summary>
    public override string GetText()
    {
        return GetContent().text;
    }

    public void RegisterLeftClickAction(Action<T> onLeftClick)
    {
        this.onLeftClick.Add(onLeftClick);
    }

    public void DeregisterLeftClickAction(Action<T> onLeftClick)
    {
        this.onLeftClick.Remove(onLeftClick);
    }

    public void RegisterRightClickAction(Action<T> onRightClick)
    {
        this.onRightClick.Add(onRightClick);
    }

    public void DeregisterRightClickAction(Action<T> onRightClick)
    {
        this.onRightClick.Remove(onRightClick);
    }

}