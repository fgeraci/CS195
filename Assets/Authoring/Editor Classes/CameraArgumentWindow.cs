using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// A CameraArgumentWindow is displayed in the main panel of the GUI to configure the camera for some
/// given EventStub.
/// </summary>
public class CameraArgumentWindow : IRenderable 
{
    //Nr of arguments as string
    private string nrOfArgumentsString = "0";

    //Parsed nr of arguments
    private int nrOfArguments;

    //The currently used CameraArguments (may be more than nrOfArguments)
    private List<CameraArgument> arguments = new List<CameraArgument>();

    //The LookAt target selections for all arguments
    private List<ActionBar> indexSelection = new List<ActionBar>();

    //The WallFade target selections for all arguments
    private List<ActionBar> fadeTargetsSelection = new List<ActionBar>();

    //The position type selection for all arguments
    private List<ActionBar> positionTypeSelection = new List<ActionBar>();

    //The rotation type selection for all arguments
    private List<ActionBar> rotationTypeSelection = new List<ActionBar>();

    //The position/offset selection strings for all arguments
    private List<string[]> offsetSelection = new List<string[]>();

    //The rotation selection strings for all arguments
    private List<string[]> rotationSelection = new List<string[]>();

    //The labels for the position/offset selection
    private string[] offsetArgNames = new string[] { "X", "Y", "Z" };

    //The labels for the rotation selection
    private string[] rotationArgNames = new string[] { "X", "Y", "Z", "W" };

    //The time offset selection for all arguments
    private List<string> timeOffsets = new List<string>();

    //The smoothness selection for all arguments
    private List<string> smoothnesses = new List<string>();

    //The current scroll position
    private Vector2 scrollPos;

    //The event for which to configure the camera
    private EventStub evnt;

    //All possible position types
    private IEnumerable<CameraPositionType> positionTypes;

    //All possible rotation types
    private IEnumerable<CameraRotationType> rotationTypes;

    /// <summary>
    /// Create a new CameraArgumentWindow for the given EventStub. Ensures that if the EventStub already has
    /// a camera configured, that configuration is displayed.
    /// </summary>
    public CameraArgumentWindow(EventStub evnt)
    {
        this.evnt = evnt;
        this.positionTypes = Enum.GetValues(typeof(CameraPositionType)).Cast<CameraPositionType>();
        this.rotationTypes = Enum.GetValues(typeof(CameraRotationType)).Cast<CameraRotationType>();
        foreach (CameraArgument arg in CameraArgumentManager.Instance.GetArgumentsForID(evnt.ID))
        {
            arguments.Add(arg);
            FillArgument(arg, arguments.Count - 1);
        }
        this.nrOfArguments = CameraArgumentManager.Instance.GetArgumentsForID(evnt.ID).Count();
        this.nrOfArgumentsString = this.nrOfArguments.ToString();
    }

    /// <summary>
    /// Render the window with the given height and width.
    /// </summary>
    public void Render(float height, float width)
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginHorizontal();
        //Enter the number of arguments to use
        GUILayout.Label("Number of arguments", GUILayout.Width(150));
        nrOfArgumentsString = GUILayout.TextField(nrOfArgumentsString, GUILayout.Width(150));
        GUILayout.EndHorizontal();
        nrOfArguments = Parse<int>(int.TryParse, nrOfArgumentsString, nrOfArguments);
        //If the nr of arguments increases, new ones need to be created.
        if (Event.current.type == EventType.Layout)
        {
            FillArguments(nrOfArguments);
        }
        for (int i = 0; i < Mathf.Min(nrOfArguments, arguments.Count); i++)
        {
            //Select the position type 
            GUILayout.Label("Select position type for argument " + i.ToString());
            positionTypeSelection[i].Render(true);
            //Select the rotation type
            GUILayout.Label("Select rotation type for argument " + i.ToString());
            rotationTypeSelection[i].Render(true);
            //Select the lookat target (applicable only if rotation type is LookAt, else ignored)
            GUILayout.Label("Select SmartObject argument, which will be the center of the camera's attention");
            indexSelection[i].Render(true);
            //Select the fade targets for which the walls fade
            GUILayout.Label("Select the SmartObject fade targets");
            fadeTargetsSelection[i].Render(true);
            //Select the camera position/offset
            GUILayout.Label("Select the offset from the SmartObject or the fixed camera position");
            GUILayout.BeginHorizontal();
            for (int j = 0; j < offsetArgNames.Length; j++)
            {
                GUILayout.Label(offsetArgNames[j], GUILayout.Width(50));
                offsetSelection[i][j] = GUILayout.TextField(offsetSelection[i][j], GUILayout.Width(80));
                arguments[i].Offset[j] = Parse<float>(float.TryParse, offsetSelection[i][j], arguments[i].Offset[j]);
            }
            //Select the camera position to be the current Camera.main position
            if (GUILayout.Button("Use current Camera position", GUILayout.Width(300)))
            {
                arguments[i].Offset = Camera.main.transform.position;
                offsetSelection[i][0] = arguments[i].Offset.x.ToString("0.00");
                offsetSelection[i][1] = arguments[i].Offset.y.ToString("0.00");
                offsetSelection[i][2] = arguments[i].Offset.z.ToString("0.00");
            }
            GUILayout.EndHorizontal();
            //Select the camera rotation for Fixed rotation type
            GUILayout.Label("Select the rotation used by the camera if using Fixed rotation type");
            GUILayout.BeginHorizontal();
            for (int j = 0; j < rotationArgNames.Length; j++)
            {
                GUILayout.Label(rotationArgNames[j], GUILayout.Width(50));
                rotationSelection[i][j] = GUILayout.TextField(rotationSelection[i][j], GUILayout.Width(80));
                arguments[i].FixedRotation[j] = Parse<float>(float.TryParse, rotationSelection[i][j], arguments[i].FixedRotation[j]);
            }
            //Select the camera rotation to be the current Camera.main rotation
            if (GUILayout.Button("Use current Camera rotation", GUILayout.Width(300)))
            {
                arguments[i].FixedRotation = Camera.main.transform.rotation;
                for (int j = 0; j < rotationArgNames.Length; j++)
                {
                    rotationSelection[i][j] = arguments[i].FixedRotation[j].ToString("0.00");
                }
            }
            GUILayout.EndHorizontal();
            //Select the event local time to start the argument
            GUILayout.Label("Select time after event start at which to start using the camera argument");
            timeOffsets[i] = GUILayout.TextField(timeOffsets[i], GUILayout.Width(50));
            arguments[i].TimeInEvent = Parse<float>(float.TryParse, timeOffsets[i], arguments[i].TimeInEvent);
            GUILayout.BeginHorizontal();
            //Select the smoothness of the camera position/rotation changes
            GUILayout.Label("Select the smoothness of the transitions. Low numbers mean smoother. Number must be positive.", GUILayout.Width(400));
            smoothnesses[i] = GUILayout.TextField(smoothnesses[i], GUILayout.Width(50));
            arguments[i].Smoothness = Parse<float>(float.TryParse, smoothnesses[i], arguments[i].Smoothness);
            GUILayout.EndHorizontal();
            Highlight(arguments, evnt);
        }
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// Saves the edited argument with the CameraArgumentManager.Instance.
    /// </summary>
    public void Save()
    {
        CameraArgumentManager.Instance.RemoveArguments(evnt.ID);
        for (int i = 0; i < nrOfArguments; i++)
        {
            CameraArgumentManager.Instance.AddArgument(evnt.ID, arguments[i]);
        }
    }

    /// <summary>
    /// Highlights the world where the cameras would be currently placed (note that these can change
    /// for the cameras which are placed at an offset).
    /// </summary>
    private void Highlight(IEnumerable<CameraArgument> arguments, EventStub evnt)
    {
        Highlighter.Instance.UnhighlightAll();
        foreach (CameraArgument arg in arguments)
        {
            Vector3 highlightPos;
            if (arg.PositionType == CameraPositionType.Fixed)
            {
                highlightPos = arg.Offset;
            }
            else
            {
                Transform target = evnt.GetSelectorForIndex(arg.TargetIndex).selectedObject.transform;
                highlightPos = target.position + target.up * arg.Offset.y + target.forward * arg.Offset.x;
            }
            Highlighter.Instance.Highlight(highlightPos);
        }
    }

    /// <summary>
    /// When increasing the number of arguments, makes sure the new arguments are properly instantiated
    /// either with an old value if they were previously already used or with an empty value.
    /// </summary>
    private void FillArguments(int newSize)
    {
        while (arguments.Count < newSize)
        {
            arguments.Add(new CameraArgument());
            FillArgument(arguments[arguments.Count - 1], arguments.Count - 1);
        }
    }

    /// <summary>
    /// Fills the argument at the given index, i.e. either uses an old argument if it was already used or
    /// instantiates a new one.
    /// </summary>
    private void FillArgument(CameraArgument arg, int index)
    {
        indexSelection.Add(new ActionBar(false, true, false));
        positionTypeSelection.Add(new ActionBar(false, true, false));
        rotationTypeSelection.Add(new ActionBar(false, true, false));
        fadeTargetsSelection.Add(new ActionBar(false, false, false));
        //initialize smoothness
        smoothnesses.Add(arg.Smoothness.ToString());
        //initialize position/offset
        offsetSelection.Add(new string[] { arg.Offset.x.ToString(), arg.Offset.y.ToString(), arg.Offset.z.ToString() });
        //initialize rotation
        rotationSelection.Add(new string[] { arg.FixedRotation.x.ToString(), arg.FixedRotation.y.ToString(),
            arg.FixedRotation.z.ToString(), arg.FixedRotation.w.ToString() });
        //initialize time offset
        timeOffsets.Add(arg.TimeInEvent.ToString());
        int count = arguments.Count - 1;
        for (int i = 0; i < evnt.NrOfNeededRoles; i++)
        {
            int local = i;
            SmartObjectSelector selector = evnt.GetSelectorForIndex(i);
            //For each object in the evnt, add a toggle for being the LookAt target
            indexSelection[count].AddToggle(
                selector.selectedObject != null ? selector.selectedObject.name : "Param " + (i+1).ToString(),
                (bool b) =>
                {
                    if (b)
                        arguments[count].TargetIndex = local;
                },
                arg.TargetIndex == i);
            //For each object in the event, add a toggle for being a WallFade target.
            fadeTargetsSelection[count].AddToggle(
                selector.selectedObject != null ? selector.selectedObject.name : "Param " + (i + 1).ToString(),
                (bool b) =>
                {
                    if (b)
                        arguments[count].FadeIndices.Add(local);
                    else
                        arguments[count].FadeIndices.Remove(local);
                },
                arg.FadeIndices.Contains(i));
        }
        //Initialize the radio buttons for selecting the position type.
        foreach (CameraPositionType type in positionTypes)
        {
            CameraPositionType local = type;
            positionTypeSelection[count].AddToggle(
                type.ToString(),
                (bool b) =>
                {
                    if (b)
                        arguments[count].PositionType = local;
                },
                local == arg.PositionType);
        }
        //Initialize the radio buttons for selecting the rotation type
        foreach (CameraRotationType type in rotationTypes)
        {
            CameraRotationType local = type;
            rotationTypeSelection[count].AddToggle(
                type.ToString(),
                (bool b) =>
                {
                    if (b)
                        arguments[count].RotationType = local;
                },
                local == arg.RotationType);
        }
    }

    private delegate bool TryParse<T>(string str, out T value);

    /// <summary>
    /// A function which tries parsing the given string value to a value of type T with the given
    /// parsing function, and either returns the parsed value on success or the given oldValue if the
    /// parsing does not succeed, to ensure the function always has a return value and does not fail.
    /// </summary>
    private T Parse<T>(TryParse<T> parseFunc, string value, T oldValue)
    {
        T newValue = default(T);
        if (parseFunc.Invoke(value, out newValue))
        {
            return newValue;
        }
        return oldValue;
    }
}
