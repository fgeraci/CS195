using UnityEngine;

/// <summary>
/// A line renderer that draws lines from one point to the other with a specified color and width.
/// </summary>
public abstract class AbstractLineRenderer
{
    public abstract void DrawLine(Vector3 from, Vector3 to, Color lineColor, float width);
}

