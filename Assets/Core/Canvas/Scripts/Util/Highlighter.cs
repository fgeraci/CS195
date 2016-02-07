using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A class for highlighting Smart Objects.
/// </summary>
public class Highlighter : MonoBehaviour
{
    /// <summary>
    /// Singleton instance. Must be null-checked, as not self-generating.
    /// </summary>
    public static Highlighter Instance { get; private set; }

    /// <summary>
    /// The prefab for the halo highlights.
    /// </summary>
    public GameObject HaloPrefab;

    private List<HaloHighlight> halos = new List<HaloHighlight>();

    private int currentIndex;

    private List<Highlight> currentHighlights = new List<Highlight>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Highlight the object after unhighlighting all others.
    /// </summary>
    public void HighlightAfterUnhighlight(SmartObject obj)
    {
        UnhighlightAll();
        Highlight(obj);
    }

    /// <summary>
    /// Highlight the collection of objects after unhighlighting all others.
    /// </summary>
    public void HighlightAfterUnhighlight(IEnumerable<SmartObject> objects)
    {
        UnhighlightAll();
        foreach (SmartObject obj in objects)
        {
            Highlight(obj);
        }
    }

    /// <summary>
    /// Highlight the position after unhighlighting all others.
    /// </summary>
    public void HighlightAfterUnhighlight(Vector3 position)
    {
        UnhighlightAll();
        HaloHighlight halo = GetHalo(currentIndex++);
        halo.TargetPosition = position;
        halo.haloEnabled = true;
    }

    /// <summary>
    /// Unhighlight all objects.
    /// </summary>
    public void UnhighlightAll()
    {
        foreach (HaloHighlight halo in halos)
        {
            halo.TargetTransform = null;
            halo.haloEnabled = false;
        }
        currentIndex = 0;
        foreach (Highlight hl in currentHighlights)
        {
            hl.SelectOff();
        }
    }

    /// <summary>
    /// Highlight the given object.
    /// </summary>
    public void Highlight(SmartObject obj)
    {
        if (obj is SmartCrowd)
        {
            IEnumerable<SmartObject> objects = ((SmartCrowd)obj).GetObjects();
            foreach (SmartObject o in objects)
            {
                Highlight(o);
            }
            return;
        }

        //Preferrably use improved highlighting system, but use old one as fallback
        Highlight highlight = obj.GetComponent<Highlight>();
        if (highlight != null)
        {
            highlight.SelectOn();
            currentHighlights.Add(highlight);
        }
        else
        {
            HaloHighlight halo = GetHalo(currentIndex++);
            halo.TargetTransform = obj.gameObject.transform;
            halo.haloEnabled = true;
        }
    }

    /// <summary>
    /// Highlight the given position.
    /// </summary>
    public void Highlight(Vector3 position)
    {
        HaloHighlight halo = GetHalo(currentIndex++);
        halo.TargetPosition = position;
        halo.haloEnabled = true;
    }

    private HaloHighlight GetHalo(int index)
    {
        while (index >= halos.Count)
        {
            Object halo = Instantiate(HaloPrefab);
            halos.Add(((GameObject)halo).GetComponent<HaloHighlight>());
        }
        return halos[index];
    }
}
