using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The line renderer used for the InEngine version of the GUI.
/// </summary>
public class InEngineLineRenderer : AbstractLineRenderer 
{
    //ColorToTexture mapping, so we only have one texture for each color.
    private static Dictionary<Color, Texture2D> colorToTexture = new Dictionary<Color, Texture2D>();

    /// <summary>
    /// This method draws a line from the given start point to the given end point. The line has the given color, and is
    /// of the given width. These vectors are in screen coordinates, not in world coordinates.
    /// Diagonal lines are not drawn diagonally, but instead as two vertical lines with a horizontal line in between.
    /// </summary>
    public override void DrawLine(Vector3 from, Vector3 to, Color lineColor, float lineWidth)
    {
        //make sure the texture for each color is only stored once for efficiency purposes
        if (!colorToTexture.ContainsKey(lineColor))
        {
            Texture2D newTex = new Texture2D(1, 1);
            newTex.SetPixel(0, 0, lineColor);
            newTex.Apply();
            colorToTexture[lineColor] = newTex;
        }

        //labels appear broader than the other lines, adapt their width a bit
        lineWidth = lineWidth / 1.5f;

        //save the old background texture
        Texture2D oldTex = GUI.skin.label.normal.background;
        GUI.skin.label.normal.background = colorToTexture[lineColor];

        //if both coordinates disagree, the line must be diagonal. Labels can only be diagonal painfully,
        //so convert it to horizontal and vertical lines
        if (from.x != to.x && from.y != to.y)
        {
            Vector3 higher = (from.y > to.y) ? to : from;
            Vector3 lower = (from.y > to.y) ? from : to;

            Vector3 left = (from.x > to.x) ? to : from;
            Vector3 right = (from.x > to.x) ? from : to;

            /* Instead of drawing a diagonal line, which is painful with labels, the line is drawn in three parts,
             * two vertical and one horizontal, as in the example below:
             * |
             * |
             * |
             * --------
             *        |
             *        |
             *        |
             */
            float halfHeight = (lower.y - higher.y) / 2;

            GUI.Label(new Rect(higher.x, higher.y, lineWidth, halfHeight), "");

            GUI.Label(new Rect(left.x, higher.y + halfHeight, right.x - left.x + lineWidth, lineWidth), "");

            GUI.Label(new Rect(lower.x, higher.y + halfHeight, lineWidth, halfHeight), "");

        }
        //in the case of x coordinates not agreeing, the line must be horizontal
        else if (from.x != to.x)
        {
            float minLeft = Mathf.Min(from.x, to.x);
            float width = Mathf.Abs(from.x - to.x);
            GUI.Label(new Rect(minLeft, from.y, width, lineWidth), "");
        }
        //and else it is vertical
        else
        {
            float minTop = Mathf.Min(from.y, to.y);
            float height = Mathf.Abs(from.y - to.y);
            GUI.Label(new Rect(from.x, minTop, lineWidth, height), "");
        }

        //reset the background texture for the label
        GUI.skin.label.normal.background = oldTex;
    }
}
