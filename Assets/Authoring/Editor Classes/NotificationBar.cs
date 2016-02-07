using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A notification bar to be used for displaying notifications on screen. The NotificationBar can also
/// display tooltips stored at GUI.tooltip. Note that in that case, the bar should be rendered last to ensure
/// the tooltip is set when it is rendered.
/// </summary>
public class NotificationBar
{
    //The current content.
    private string content;

    //Whether this bar should display tooltips.
    private bool showTooltips;

    /// <summary>
    /// Create a new NotificationBar with the given content.
    /// </summary>
    /// <param name="content">Content to be displayed.</param>
    /// <param name="showTooltips">Do we show tooltips?</param>
    public NotificationBar(string content, bool showTooltips = true)
    {
        this.content = content;
        this.showTooltips = showTooltips;
    }

    /// <summary>
    /// Renders the bar. If a tooltip exists, we show that preferrably to the normal content.
    /// Tooltips are always shown as info type.
    /// </summary>
    public void Render()
    {
        string content = this.content;
        if (GUI.tooltip != "" && showTooltips)
        {
            content = GUI.tooltip;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label(content);
        GUILayout.EndHorizontal();
    }
}