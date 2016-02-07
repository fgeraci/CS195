/// <summary>
/// Any window that can be rendered with a given width and height can implement
/// this interface.
/// </summary>
public interface IRenderable
{
    /// <summary>
    /// Renders the window with the given height and width.
    /// </summary>
    /// <param name="height">The available height on screen.</param>
    /// <param name="width">The available width on screen.</param>
    void Render(float height, float width);
}