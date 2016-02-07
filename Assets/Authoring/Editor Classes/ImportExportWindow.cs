using UnityEngine;
using System.Linq;

/// <summary>
/// The window to be used for loading and saving narratives, to be rendererd in the GUI's main panel.
/// </summary>
public class ImportExportWindow : IRenderable
{
    //The serializer for actually loading and saving the narratives.
    private StoryArcSerializer serializer = StoryArcSerializer.Instance;

    //The name of the file to load
    private string importFileName = null;

    //The name of the file to save to
    private string exportFileName = "";

    //The selection of files available for loading
    private ActionBar importSelection;

    //The current scroll position
    private Vector2 scrollPos;

    //The current path
    private string path;

    /// <summary>
    /// Creates a new window to be used for loading and saving narratives.
    /// </summary>
    public ImportExportWindow()
    {
        Init();
    }

    private void Init()
    {
        importSelection = new ActionBar(false, true, false);
        //Add all available narratives to the selection of available files.
        foreach (string name in serializer.OfferNames())
        {
            string localName = name;
            importSelection.AddToggle(name, (bool val) =>
            {
                if (val)
                {
                    importFileName = exportFileName = localName;
                }
                else
                {
                    importFileName = null;
                }
            });
        }
        path = serializer.DirectoryPath;
    }

    /// <summary>
    /// Tries exporting the current StoryArc with the currently set name.
    /// </summary>
    public void TryExport()
    {
        ImporterExporter.TrySave(exportFileName);
    }

    /// <summary>
    /// Imports the currently selected StoryArc if any is selected.
    /// </summary>
    public void TryImport()
    {
        ImporterExporter.TryLoad(importFileName);
    }

    /// <summary>
    /// Renders the window with the given height and width.
    /// </summary>
    public void Render(float height, float width)
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        //Export part: Can enter a name for the exported narrative to have.
        GUILayout.Label("Enter a name and press Save to save your current narrative");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Width(100));
        exportFileName = GUILayout.TextField(exportFileName, GUILayout.Width(150));
        GUILayout.EndHorizontal();

        //Import part: Can select a narrative to import, can also delete selected narratives.
        //Selecting a narrative to import also changes the export name so it can be overwritten.
        GUILayout.Label("Select any of the narratives below and press Load to load them");
        importSelection.Render(true);
        GUILayout.Label("Press Delete to permanently remove the selected narrative");
        GUI.enabled = serializer.IsValidFileName(importFileName);
        if (GUILayout.Button("Delete", GUILayout.Width(100)))
        {
            serializer.Delete(importFileName);
            importSelection.RemoveToggle(importFileName);
            importFileName = null;
        }
        GUI.enabled = true;

        //Change the directory to search
        GUILayout.Label("Select the directory to search in");

        path = GUILayout.TextField(path);

        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            serializer.DirectoryPath = path;
            Init();
        }

        GUILayout.EndScrollView();
    }
}

/// <summary>
/// Utility class for loading and saving narratives.
/// </summary>
public static class ImporterExporter
{
    /// <summary>
    /// Tries loading the narrative under the given file name.
    /// </summary>
    /// <param name="filename">The narrative's file name.</param>
    public static void TryLoad(string filename)
    {
        FullStoryArc story = StoryArcSerializer.Instance.Deserialize(filename);
        //NOTE: crowds and waypoints were already added to the ObjectManager on creation.
        //imports the Story Arc to the event manager
        AuthoredEventManager.Instance.ImportStoryArc(story.Arc);
        //imports the dependencies to the event manager
        foreach (EventID key in story.Dependencies.Keys)
        {
            foreach (EventID value in story.Dependencies[key])
            {
                AuthoredEventManager.Instance.AddTerminationDependency(value, key);
            }
        }
        //imports the camera arguments to the camera manager
        CameraArgumentManager.Instance.AddArguments(story.CameraArguments);
    }

    /// <summary>
    /// Creates a FullStoryArc from the available managers.
    /// </summary>
    private static FullStoryArc CreateArc()
    {
        return new FullStoryArc(
            AuthoredEventManager.Instance.ToStoryArc(),
            AuthoredEventManager.Instance.Dependencies,
            ObjectManager.Instance.GetObjectsByType<SmartCrowd>(),
            ObjectManager.Instance.GetObjectsByType<SmartWaypoint>().Where((SmartWaypoint wp) => wp.IsSystemGenerated),
            CameraArgumentManager.Instance.IDToArguments);
    }

    /// <summary>
    /// Tries storing the current narrative under the given file name.
    /// </summary>
    /// <param name="filename">The narrative's file name.</param>
    public static void TrySave(string filename)
    {
        StoryArcSerializer.Instance.Serialize(CreateArc(), filename);
    }

    /// <summary>
    /// Tries storing the current narrative to the temporary file.
    /// </summary>
    public static void TrySaveToTemp()
    {
        StoryArcSerializer.Instance.Serialize(CreateArc(), StoryArcSerializer.TEMP_FILE_NAME, false);
    }
}
