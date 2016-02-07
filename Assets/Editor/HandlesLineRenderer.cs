using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor version of the line renderer, using the Handles interface.
/// </summary>
public class HandlesLineRenderer : AbstractLineRenderer 
{
    /// <summary>
    /// Renders a line.
    /// </summary>
    /// <param name="from">Point to render line from.</param>
    /// <param name="to">Point to render line to.</param>
    /// <param name="lineColor">Color of the line</param>
    /// <param name="width">Width of the line</param>
    public override void DrawLine(Vector3 from, Vector3 to, Color lineColor, float width)
    {
        Color old = Handles.color;
        Handles.color = lineColor;
        Handles.DrawAAPolyLine(width, from, to);
        Handles.color = old;
    }
}
