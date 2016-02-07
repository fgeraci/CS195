using UnityEngine;
using System.Collections.Generic;
using System;
using TreeSharpPlus;
using System.Reflection;

/// <summary>
/// A sidebar to display elements of some type with a given content function.
/// </summary>
/// <typeparam name="T">The type of elements to display.</typeparam>
public class Sidebar<T>
{
    private class ContentComparer : IComparer<ContentRectangle<T>>
    {
        /// <summary>
        /// Compares the text content alphabetically.
        /// </summary>
        public int Compare(ContentRectangle<T> x, ContentRectangle<T> y)
        {
            return x.GetText().CompareTo(y.GetText());
        }

        public ContentComparer()
        {
        }
    }

    //Current scrollposition
    private Vector2 scrollPosition;

    //Whether content is static
    private bool isContentStatic;

    //Function to get the content for the objects
    private Func<T, GUIContent> contentFunction;

    //All objects, not just the displayed ones
    public HashSet<T> allObjects { get; private set; }

    //The renderable content for the objects
    private Dictionary<T, ContentRectangle<T>> objectToRectangle;

    //The currently displayed content
    private List<ContentRectangle<T>> currentContent;

    //What happens when left clicking on an object
    private HashSet<Action<T>> onLeftClick;

    //What happens when right clicking on an object
    private HashSet<Action<T>> onRightClick;

    //The search term to filter by
    private string searchTerm;

    //The types to filter by
    private Dictionary<Type, Boxed<bool>> currentFilterTypes;

    //Whether the toggles for the type filtering are shown
    private bool showingTypeSelection;

    //Comparer for sorting the content alphabetically
    private ContentComparer comparer;

    /// <summary>
    /// Create a new Sidebar with default ToString as ContentFunction and dynamic content.
    /// </summary>
    public Sidebar() : this(false) { }

    /// <summary>
    /// Create a new Sidebar with default ToString as ContentFunction.
    /// </summary>
    /// <param name="isContentStatic">Whether the content should stay static.</param>
    public Sidebar(bool isContentStatic) : this(isContentStatic, (T content) => new GUIContent(content.ToString())) { }

    /// <summary>
    /// Create a new Sidebar with the given ContentFunction.
    /// </summary>
    /// <param name="isContentStatic">Whether the content should stay static.</param>
    /// <param name="contentFunction">The function to get the content for the elements.</param>
    public Sidebar(bool isContentStatic, Func<T, GUIContent> contentFunction)
    {
        this.allObjects = new HashSet<T>();
        this.isContentStatic = isContentStatic;
        this.contentFunction = contentFunction;
        this.searchTerm = "";
        this.objectToRectangle = new Dictionary<T, ContentRectangle<T>>();
        this.currentContent = new List<ContentRectangle<T>>();
        this.onLeftClick = new HashSet<Action<T>>();
        this.onRightClick = new HashSet<Action<T>>();
        this.currentFilterTypes = new Dictionary<Type, Boxed<bool>>();
        this.comparer = new ContentComparer();
    }

    /// <summary>
    /// Renders the objectSidebar with its current content.
    /// </summary>
    public void Render()
    {
        GUILayout.Label("Enter filter term:");
        //Show the current search term, offer input
        searchTerm = GUILayout.TextField(searchTerm);
        //Toggle on which types should be filtered by.
        if (currentFilterTypes.Count > 1)
        {
            if (showingTypeSelection = GUILayout.Toggle(showingTypeSelection, "Show filter types"))
            {
                if (GUILayout.Toggle(false, "Show All"))
                {
                    currentFilterTypes.Keys.ForEach((Type t) => currentFilterTypes[t].value = true);
                }
                if (GUILayout.Toggle(false, "Hide All"))
                {
                    currentFilterTypes.Keys.ForEach((Type t) => currentFilterTypes[t].value = false);
                }                
                foreach (Type type in currentFilterTypes.Keys)
                {
                    currentFilterTypes[type].value = GUILayout.Toggle(currentFilterTypes[type], type.ToString());//, Styles.ToggleStyle);
                }
            }
        }
        //Filter the set of displayed elements.
        Filter();
        //Sort the content alphabetically.
        currentContent.Sort(comparer);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        //Do the actual rendering.
        foreach (ContentRectangle<T> rectangle in currentContent)
        {
            rectangle.Render();
        }
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// Returns whether the givne text contains the given searchTerm. Not case sensitive.
    /// </summary>
    private bool ContainsText(string text, string searchTerm)
    {
        return text.ToUpper().Contains(searchTerm.ToUpper());
    }

    /// <summary>
    /// Uses the current searchTerm and filter types to determine which objects should be shown.
    /// </summary>
    private void Filter()
    {
        List<ContentRectangle<T>> toRemove = new List<ContentRectangle<T>>();
        //Check which elements that are displayed must be removed.
        foreach (ContentRectangle<T> rectangle in currentContent)
        {
            if (!ContainsText(rectangle.GetText(), searchTerm) || 
                (currentFilterTypes.ContainsKey(rectangle.containedObject.GetType()) && 
                !currentFilterTypes[rectangle.containedObject.GetType()]))
            {
                toRemove.Add(rectangle);
                continue;
            }
            if (!allObjects.Contains(rectangle.containedObject))
            {
                toRemove.Add(rectangle);
            }
        }
        //Check which elements are not displayed but should be. Also check if there are new types
        //to filter by.
        foreach (T key in allObjects)
        {
            if (Event.current.type == EventType.Repaint && !currentFilterTypes.ContainsKey(key.GetType()))
            {
                currentFilterTypes[key.GetType()] = new Boxed<bool>(true);
            }
            ContentRectangle<T> rect = GetRectangleFor(key);
            if (ContainsText(rect.GetText(), searchTerm) && !currentContent.Contains(rect) &&
                (!currentFilterTypes.ContainsKey(rect.containedObject.GetType()) || 
                currentFilterTypes[rect.containedObject.GetType()]))
            {
                currentContent.Add(rect);
            }
        }
        currentContent.RemoveAll((ContentRectangle<T> rect) => toRemove.Contains(rect));
    }

    /// <summary>
    /// Gets the content rectangle for the given object, creates one if necessary.
    /// </summary>
    private ContentRectangle<T> GetRectangleFor(T obj)
    {
        if (!objectToRectangle.ContainsKey(obj))
        {
            objectToRectangle.Add(obj, new ContentRectangle<T>(obj, true, default(Rect), isContentStatic, contentFunction));
            objectToRectangle[obj].onLeftClick = onLeftClick;
            objectToRectangle[obj].onRightClick = onRightClick;
        }
        return objectToRectangle[obj];
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

    /// <summary>
    /// A simple class to create boxed values of some type S so even primitive values
    /// can be changed in a ForEach loop. Contains implicit conversion operators.
    /// </summary>
    /// <typeparam name="S">The type to be boxed.</typeparam>
    private class Boxed<S>
    {
        public S value;

        public Boxed(S value)
        {
            this.value = value;
        }

        public static implicit operator Boxed<S>(S value)
        {
            return new Boxed<S>(value);
        }

        public static implicit operator S(Boxed<S> value)
        {
            return value.value;
        }
    }
}
