using UnityEngine;
using System.Collections.Generic;
using TreeSharpPlus;
using System;
using System.Linq;

/// <summary>
/// The MainWindow, rendered in the GUI's main panel. This window is responsible for displaying the actual
/// narrative as well as logic for certain drag-and-drop and line drawing operations. This is the main anchor
/// point for the entire GUI, as the most important logic happens here.
/// </summary>
public class MainWindow : IRenderable
{
    #region Attributes
    //current level of zoom
    private float zoomLevel = 1.0f;

    private const float MAX_ZOOM = 3.0f;

    private const float MIN_ZOOM = 0.5f;

    //Single zoom step
    private const float STEP_ZOOM = 0.1f;

    //Object rect width for text rectangles
    private const float WIDTH_OBJECT_RECT_V_TEXT = 140;

    //Object rect width for image rectangles
    private const float WIDTH_OBJECT_RECT_V_IMG = 60 + 10;

    private const float HEIGHT_OBJECT_RECT = 80 + 10;

    private const float WIDTH_EVENT_RECT = 240 + 10;

    private const float HEIGHT_EVENT_RECT = 135 + 10;

    //Height of a single beat.
    private const float HEIGHT_LEVEL = HEIGHT_EVENT_RECT + HEIGHT_BETWEEN;

    //Height between 2 beats
    private const float HEIGHT_BETWEEN = 50;

    //Height of the lines
    private const float HEIGHT_CONNECTOR_LINES = HEIGHT_BETWEEN - 10;

    //Width of the connector lines in the rectangle
    private const float WIDTH_PARTICIPANT_CONNECTORS = WIDTH_EVENT_RECT - 60;

    private const float START_PARTICIPANT_CONNECTORS = 30;

    //Margin between two rectangles (right)
    private float MARGIN_BETWEEN;

    private const float MARGIN_TOP = 20;

    //How close one must go to a connector to "connect"
    private const float DELTA_CONNECTOR_DIST = 10.0f;

    //Total width of object rectangles.
    private float totalObjectWidth;

    //Height of window
    private float height;

    //Actual height of the entire content
    private float totalHeight;

    private CrowdAuthoringEditor parent;

    //Gradient used for object line colors.
    private Gradient colorGradient;

    //ContentRectangles for the objects
    private Dictionary<SmartObject, ContentRectangle<SmartObject>> rectangleForObject = new Dictionary<SmartObject, ContentRectangle<SmartObject>>();

    private Dictionary<SmartObject, Color> lineColorForObject = new Dictionary<SmartObject, Color>();

    //ContentRectangles for the events
    private Dictionary<EventStub, ContentRectangle<EventStub>> rectangleForEvent = new Dictionary<EventStub, ContentRectangle<EventStub>>();

    //Connectors for the events
    private Dictionary<EventStub, HashSet<ParticipantConnector>> connectors = new Dictionary<EventStub, HashSet<ParticipantConnector>>();

    private Dictionary<int, Level> levels = new Dictionary<int, Level>();

    public AuthoredEventManager manager { get; private set; }

    //How many events are in the most populated beat?
    private int maxNrOfEventsInLevel;

    //Which is the highest populated beat?
    private int maxLevel;

    private Vector2 scrollPosition;

    private HashSet<Action<SmartObject>> onObjectLeftClick = new HashSet<Action<SmartObject>>();

    private HashSet<Action<SmartObject>> onObjectRightClick = new HashSet<Action<SmartObject>>();

    private HashSet<Action<EventStub>> onEventLeftClick = new HashSet<Action<EventStub>>();

    private HashSet<Action<EventStub>> onEventRightClick = new HashSet<Action<EventStub>>();

    private HashSet<Action<EventStub>> onEventMouseOver = new HashSet<Action<EventStub>>();

    private HashSet<Action<EventStub>> onEventMouseOut = new HashSet<Action<EventStub>>();

    //Renderer for drawing lines
    private AbstractLineRenderer renderer;

    //Current mouse position for drawing lines
    private Vector3 currentMousePosition;

    //Where did drawing start?
    private ContentRectangle startDrawRectangle;

    //Are we drawing a participant line?
    private bool drawingParticipantConnection;

    //Who is the participant?
    private SmartObject participant;

    //Are we dragging an event?
    private bool draggingEvent;

    //Drag rectangle for participant
    private ContentRectangle<SmartObject> participantDragRect;

    //Drag rectangle for event
    private ContentRectangle<EventStub> eventDragRect;

    //Are we dragging a participant?
    private bool draggingParticipant;

    //The last added event stub
    public EventStub currentEventStub;

    //The current context menu for object/event
    private ActionBar contextMenu;

    //The area allocated for the context menu
    private Rect contextMenuArea;

    //Do we display lines?
    public bool ShowLines = false;

    //Creates the images with the param images on them.
    private List<Texturizer> texturizers = new List<Texturizer>();

	//Planning
	const PlanSpace OPTION = PlanSpace.All;
    private StateSpaceManager stateSpaceManager;
	private StoryArc undoArc;
    #endregion

    #region Creation
    /// <summary>
    /// Creates a new MainWindow.
    /// </summary>
    /// <param name="manager">The AuthoredEventManager responsible for managing EventStubs.</param>
    /// <param name="parent">The parent window.</param>
    /// <param name="renderer">The renderer for drawing lines.</param>
    public MainWindow(AuthoredEventManager manager, CrowdAuthoringEditor parent, AbstractLineRenderer renderer)
    {
        this.renderer = renderer;
        this.parent = parent;
        this.colorGradient = new Gradient();
        this.colorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.blue, 0.2f), 
            new GradientColorKey(Color.green, 0.4f), new GradientColorKey(Color.yellow, 0.6f), new GradientColorKey(Color.white, 0.8f), new GradientColorKey(Color.magenta, 1.0f)};
        this.colorGradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
        this.manager = manager;
        this.onObjectLeftClick.Add(StartDrawParticipantLine);
        this.onObjectRightClick.Add((SmartObject obj) => parent.onRepaint.Add(() => AddObjectContextMenu(obj)));
        this.onEventRightClick.Add((EventStub evnt) => parent.onRepaint.Add(() => AddEventContextMenu(evnt)));
        this.onEventMouseOver.Add(OnEventMouseOver);
        this.onEventMouseOut.Add(OnEventMouseOut);

        AddButtons();
		stateSpaceManager = new StateSpaceManager ();
	}

    public MainWindow Copy()
    {
        return new MainWindow(this.manager, this.parent, this.renderer);
    }
    #endregion

    #region GUIContent Functions
    /// <summary>
    /// Get the GUIContent for an EventStub.
    /// </summary>
    private GUIContent EventStubContent(EventStub evnt)
    {
        Texturizer texturizer = new Texturizer(evnt);
        this.texturizers.Add(texturizer);
        if (texturizer.HasTexture())
        {
            return new GUIContent(texturizer.GetTexture(), "Termination dependencies are highlighted");
        }
        else
        {
            return new GUIContent(evnt.Name, "Termination dependencies are highlighted");
        }
    }

    /// <summary>
    /// Get the GUIContent for a SmartObject.
    /// </summary>
    private GUIContent ObjectContent(SmartObject obj)
    {
        if (obj.Portrait != null)
        {
            return new GUIContent(obj.Portrait);
        }
        else
        {
            return CrowdAuthoringEditor.ObjectContentString(obj);
        }
    }
    #endregion

    /// <summary>
    /// Do any cleanup needed and play the scene.
    /// </summary>
    public void Play()
    {
        manager.Start();
        foreach (Texturizer texturizer in this.texturizers)
        {
            if (texturizer.HasTexture())
            {
                rectangleForEvent[texturizer.Event].GetContent().image =
                    texturizer.ToTexture2D();
                texturizer.Destroy();
            }
        }
        this.texturizers.Clear();
    }

    /// <summary>
    /// Clear the mainwindow, and do any cleanup needed
    /// </summary>
    public void Clear()
    {
        manager.ClearAllEvents();
        foreach (Texturizer texturizer in this.texturizers)
        {
            texturizer.Destroy();
        }
        this.texturizers.Clear();
    }

    #region Adding WaitFor dependency, Adding Event Planning
    /// <summary>
    /// Add Buttons to the main action bar for planning, fillin and termination dependencies.
    /// </summary>
    private void AddButtons()
    {
        parent.ActionBar.AddButton(new GUIContent("Add Termination Dependency",
            "First, select the events to terminate. Then select the events on which their termination depends."),
            () => InitDualGroupSelection(AddTerminationDependency));
        parent.ActionBar.AddButton("Plan Globally", () => PlanGlobal());
        parent.ActionBar.AddButton("Fill In All", () => FillInAll());
    }

    /// <summary>
    /// Creates a termination dependency between the eventsToTerminate and the dependentOn, so that the
    /// events in eventsToTerminate terminate as soon as any of those in dependentOn do so.
    /// </summary>
    private void AddTerminationDependency(HashSet<EventStub> eventsToTerminate, HashSet<EventStub> dependentOn)
    {
        foreach (EventStub evnt in eventsToTerminate)
        {
            foreach (EventStub dependency in dependentOn)
            {
                manager.AddTerminationDependency(dependency, evnt);
            }
        }
    }

    /// <summary>
    /// Logic that handles selecting two groups of event stubs, and then executing a given action when
    /// both groups are selected and the Finish button is pressed.
    /// </summary>
    private void InitDualGroupSelection(Action<HashSet<EventStub>, HashSet<EventStub>> onSelectionEnd)
    {
        HashSet<EventStub> firstSet = new HashSet<EventStub>();
        HashSet<EventStub> secondSet = new HashSet<EventStub>();
        //all onClick actions
        Action<EventStub> highlightGreen = (EventStub evnt) => SetEventColor(evnt, Color.green);
        Action<EventStub> highlightMagenta = (EventStub evnt) => SetEventColor(evnt, Color.magenta);
        Action<EventStub> addToFirst = (EventStub e) => firstSet.Add(e);
        Action<EventStub> addToSecond = (EventStub e) => secondSet.Add(e);
        onEventLeftClick.Add(addToFirst);
        onEventLeftClick.Add(highlightGreen);
        ActionBar bar = new ActionBar(true);
        bar.AddButton("Cancel", () =>
        {
            onEventLeftClick.Remove(addToFirst);
            onEventLeftClick.Remove(addToSecond);
            onEventLeftClick.Remove(highlightGreen);
            onEventLeftClick.Remove(highlightMagenta);
            ResetEventColors(firstSet.Union(secondSet));
            parent.PopActionBar();
        });
        bar.AddButton("Next", () => 
            {
                onEventLeftClick.Remove(addToFirst);
                onEventLeftClick.Remove(highlightGreen);
                onEventLeftClick.Add(highlightMagenta);
                onEventLeftClick.Add(addToSecond);
                bar.RemoveButton("Next");
                bar.AddButton("Finish", () =>
                {
                    onEventLeftClick.Remove(addToSecond);
                    parent.PopActionBar();
                    onEventLeftClick.Remove(highlightMagenta);
                    ResetEventColors(firstSet.Union(secondSet));
                    onSelectionEnd.Invoke(firstSet, secondSet);
                });
            });
        parent.AddActionBar(bar);
    }
    #endregion

    #region Rendering
    /// <summary>
    /// Renders all the rectangles, and the lines between them if wanted.
    /// </summary>
    /// <param name="height">The height of the main window rectangle on screen.</param>
    /// <param name="width">The width of the main window rectangle on screen.</param>
    public void Render(float height, float width)
    {
        //handle zoom
        float zoom = Input.GetAxis("GUI Zoom");
        if (Mathf.Abs(zoom) >= 0.1f)
        {
            float oldZoomLevel = zoomLevel;
            zoomLevel = Mathf.Clamp(zoomLevel + zoom * STEP_ZOOM, MIN_ZOOM, MAX_ZOOM);
            float scale = zoomLevel / oldZoomLevel;
            CalculateEventAndObjectPositions();
            Vector2 mouseLocal = new Vector2(
                Mathf.Clamp(Event.current.mousePosition.x, 0, width),
                Mathf.Clamp(Event.current.mousePosition.y, 0, height));
            //makes the zoom center around the mouse cursor
            scrollPosition = (scrollPosition + mouseLocal) * scale - mouseLocal;
        }

        this.MARGIN_BETWEEN = GUI.skin.button.margin.right;
        this.height = height;

        //handle end of drag/drop
        currentMousePosition = Event.current.mousePosition + scrollPosition;
        if (Event.current.type == EventType.mouseUp)
        {
            drawingParticipantConnection = false;
            if (draggingParticipant)
            {
                OnEndParticipantDrag();
            }
            if (draggingEvent)
            {
                OnEndEventDrag();
            }
            draggingParticipant = false;
            draggingEvent = false;
        }

        //remove events from the GUI that were deleted from the manager.
        List<EventStub> removedFromManager = new List<EventStub>(rectangleForEvent.Keys.Except(manager.AllEvents));
        if (removedFromManager.Count() > 0)
        {
            foreach (EventStub evnt in removedFromManager)
            {
                RemoveEvent(evnt, false);
            }
            CalculateEventAndObjectPositions();
        }


        //Add objects in all events that are not in the GUI (e.g. FillIn, WaypointGeneration)
        foreach(EventStub evnt in manager.AllEvents)
        {
            foreach (SmartObject obj in evnt.InvolvedObjects)
            {
                AddSmartObject(obj);
            }
        }

        //add events to the GUI which were added directly to the manager
        List<EventStub> missingInGUI = new List<EventStub>(manager.AllEvents.Except(rectangleForEvent.Keys));
        foreach (EventStub evnt in missingInGUI.OrderBy((EventStub evnt) => manager.GetLevelForEvent(evnt)))
        {
            AddFinishedEvent(evnt, false);
        }


        //calculate the maximum level of any event
        maxLevel = 0;
        foreach (EventStub evnt in manager.AllEvents)
        {
            maxLevel = Mathf.Max(maxLevel, manager.GetLevelForEvent(evnt));
        }

        GUI.enabled = contextMenu == null;

        this.totalHeight = zoomLevel * (maxLevel * HEIGHT_LEVEL + HEIGHT_OBJECT_RECT + MARGIN_TOP);

        //makes sure that if trying to place the event in the last level, extra space in GUI is allocated
        float totalHeight = Mathf.Max(this.totalHeight, draggingEvent ? eventDragRect.Position.yMax : 0);


        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, width, height), scrollPosition, 
            new Rect(0, 0, zoomLevel * (MARGIN_BETWEEN + maxNrOfEventsInLevel * (WIDTH_EVENT_RECT + MARGIN_BETWEEN)) + totalObjectWidth, totalHeight));
        foreach (ContentRectangle<SmartObject> rect in rectangleForObject.Values)
        {
            rect.Render();

            int objectLevel = manager.GetLevelForObject(rect.containedObject);

            if (ShowLines)
            {
                //draw the "life line" of the given object
                Vector3 startPoint = new Vector3(
                    rect.Position.xMin + rect.Position.width / 2,
                    rect.Position.yMin + rect.Position.height,
                    0);
                Vector3 endPoint = new Vector3(
                    rect.Position.xMin + rect.Position.width / 2,
                    rect.Position.yMax + zoomLevel * ((objectLevel > 0 ? objectLevel - 1 : objectLevel) * HEIGHT_LEVEL + HEIGHT_BETWEEN),
                    0);

                DrawLine(startPoint, endPoint, lineColorForObject[rect.containedObject]);
            }
        }

        foreach (ContentRectangle<EventStub> rect in rectangleForEvent.Values)
        {
            rect.Render();
        }

        GUI.enabled = true;

        if (ShowLines)
        {
            foreach (EventStub evnt in rectangleForEvent.Keys)
            {
                DrawLines(evnt);
            }
        }

        if (contextMenu != null)
        {
            GUILayout.BeginArea(contextMenuArea);
            contextMenu.Render(true, false);
            GUILayout.EndArea();
        }

        if (drawingParticipantConnection || draggingEvent)
        {
            OnContinueDraw();
        }

        if (draggingParticipant)
        {
            OnContinueDrag();
        }

        if (contextMenu != null && Event.current.type == EventType.mouseDown)
        {
            contextMenu = null;
        }

        GUI.EndScrollView();
    }

    /// <summary>
    /// Draws lines from the given rectangle to the contained object's predecessors' rectangles, as well as to the
    /// "life lines" of the event's involved objects.
    /// </summary>
    private void DrawLines(EventStub evnt)
    {
        foreach (Connector connector in connectors[evnt])
        {
            connector.Render();
        }
    }

    /// <summary>
    /// Draw a line from the end- to the start point with the given color.
    /// </summary>
    private void DrawLine(Vector3 startPoint, Vector3 endPoint, Color color)
    {
        renderer.DrawLine(startPoint, endPoint, color, Mathf.Max(2.0f, zoomLevel * 4.5f));
    }

    /// <summary>
    /// Draw a line from the end- to the start point with a default color.
    /// </summary>
    private void DrawLine(Vector3 startPoint, Vector3 endPoint)
    {
        DrawLine(startPoint, endPoint, Color.black);
    }

    /// <summary>
    /// Action to execute when doing mouse over over events.
    /// </summary>
    private void OnEventMouseOver(EventStub evnt)
    {
        foreach (EventStub current in rectangleForEvent.Keys)
        {
            if (manager.HasDependency(current, evnt))
            {
                SetEventColor(current, Color.yellow);
            }
        }
    }

    /// <summary>
    /// Action to execute when doing mouse out over events.
    /// </summary>
    private void OnEventMouseOut(EventStub evnt)
    {
        foreach (EventStub current in rectangleForEvent.Keys)
        {
            if (manager.HasDependency(current, evnt))
            {
                ResetEventColor(current);
            }
        }
    }

    #endregion

    #region RightClick Context Menus
    /// <summary>
    /// Adds a new menu when right clicking an event, which offers some options
    /// for the given event.
    /// </summary>
    private void AddEventContextMenu(EventStub evnt)
    {
        contextMenu = new ActionBar(false);

        contextMenuArea = new Rect(
            Event.current.mousePosition.x + scrollPosition.x,
            Mathf.Min(Event.current.mousePosition.y + scrollPosition.y - parent.HEIGHT_BARS, height - 140 + scrollPosition.y),
            250, 125);

        //Close the context menu on cancel
        contextMenu.AddButton("Cancel", () => contextMenu = null);
        //Clear the event's parameters on Clear Params
        contextMenu.AddButton("Clear Params", () =>
        {
            for (int i = 0; i < evnt.NrOfNeededRoles; i++)
            {
                evnt.GetSelectorForIndex(i).TrySet(null);
            }
            contextMenu = null;
        });
        //Remove the event from the manager and GUI
        contextMenu.AddButton("Remove", () =>
        {
            RemoveEvent(evnt);
            CalculateEventAndObjectPositions();
            contextMenu = null;
        });
        //Fill in missing parameters
        contextMenu.AddButton("Fill In", () =>
        {
            FillIn(evnt);
            contextMenu = null;
        });
        //Fill in missing parameters after selecting rules
        contextMenu.AddButton("Fill In With", () =>
        {
            CreateFillInWithContextMenU(contextMenu);
        });
        //Highlight the event
        contextMenu.AddButton("Highlight", () =>
        {
			SetEventColor(evnt, Color.cyan);
            contextMenu = null;
        });
        //Configure the camera for the event
        contextMenu.AddButton("Camera", () =>
        {
            SelectCameraArguments(evnt);
            contextMenu = null;
        });
    }

    /// <summary>
    /// Creates a context menu for the "Fill In With" action. On back, goes back
    /// to the previous context menu.
    /// </summary>
    private void CreateFillInWithContextMenU(ActionBar oldContextMenu)
    {
        this.contextMenu = new ActionBar(false);
        this.contextMenu.AddButton("Back", () => this.contextMenu = oldContextMenu);
        FillInOptionsSerializer.AddToggles(this.contextMenu);
        this.contextMenu.AddButton("Fill In", () =>
        {
            FillIn(null);
            this.contextMenu = null;
        });
    }

    /// <summary>
    /// Code for opening a new window to select camera arguments.
    /// </summary>
    private void SelectCameraArguments(EventStub evnt)
    {
        CameraArgumentWindow window = new CameraArgumentWindow(evnt);
        parent.mainArea = window;
        parent.AddActionBar(new ActionBar(true));
        parent.ActionBar.AddButton("Cancel", parent.Reset);
        parent.ActionBar.AddButton("Save", () =>
        {
            window.Save();
            parent.Reset();
        });
    }

    /// <summary>
    /// Adds a context menu for the given smart object.
    /// </summary>
    private void AddObjectContextMenu(SmartObject obj)
    {
        contextMenuArea = new Rect(
            Event.current.mousePosition.x + scrollPosition.x,
            Mathf.Min(Event.current.mousePosition.y + scrollPosition.y - parent.HEIGHT_BARS, height - 85 + scrollPosition.y),
            250, 70);

        contextMenu = new ActionBar(false);
        //Close the context menu on cancel
        contextMenu.AddButton("Cancel", () => contextMenu = null);
        //Offer to remove the object..
        contextMenu.AddButton("Remove", () => { RemoveSmartObject(obj); contextMenu = null; });
        //..but only if it does not participate in any events
        contextMenu.SetActive("Remove", manager.GetInvolvedInEvents(obj).Count == 0);
    }
    #endregion

    #region SmartObject logic
    /// <summary>
    /// Add the given smart object to the current content.
    /// </summary>
    public void AddSmartObject(SmartObject obj)
    {
        if (!rectangleForObject.ContainsKey(obj))
        {
            rectangleForObject.Add(obj, new ContentRectangle<SmartObject>(obj, false, new Rect(0, 0, 0, 0), true, ObjectContent));
            rectangleForObject[obj].Style = rectangleForObject[obj].GetContent().image == null ? GUI.skin.button : GUI.skin.GetStyle("ImageBackground");
            rectangleForObject[obj].onLeftClick = onObjectLeftClick;
            rectangleForObject[obj].onRightClick = onObjectRightClick;
            CalculateLineColors();
            CalculateEventAndObjectPositions();
        }
    }

    /// <summary>
    /// Removes the given SmartObject, iff no event uses it.
    /// </summary>
    public void RemoveSmartObject(SmartObject obj)
    {
        if (manager.GetInvolvedInEvents(obj).Count == 0)
        {
            rectangleForObject.Remove(obj);
            CalculateLineColors();
            CalculateEventAndObjectPositions();
        }
    }

    /// <summary>
    /// Calculate the line colors for each smart object's life line.
    /// </summary>
    private void CalculateLineColors()
    {
        float index = 0;
        float count = rectangleForObject.Count;
        foreach (SmartObject obj in rectangleForObject.Keys)
        {
            lineColorForObject[obj] = colorGradient.Evaluate(index / (count - 1));
            index++;
        }
    }
    #endregion

    #region Drawing line logic
    /// <summary>
    /// Start drawing a participant line or start dragging a participant, depending on whether
    /// ShowLines is true and the participant has an image associated to it.
    /// </summary>
    private void StartDrawParticipantLine(SmartObject obj)
    {
        if (rectangleForObject[obj].GetContent().image != null && !ShowLines)
        {
            ContentRectangle rect = rectangleForObject[obj];
            participantDragRect = new ContentRectangle<SmartObject>(
                obj,
                false,
                new Rect(currentMousePosition.x, currentMousePosition.y,
                    rect.Position.width / 2, rect.Position.height / 2),
                true,
                ObjectContent);
            participantDragRect.Style = GUI.skin.GetStyle("ImageBackground");
            draggingParticipant = true;
        }
        else
        {
            ShowLines = true;
            startDrawRectangle = rectangleForObject[obj];
            participant = obj;
            drawingParticipantConnection = true;
        }
    }

    /// <summary>
    /// Start dragging an event to position it within a new beat.
    /// </summary>
    private void StartDragEvent(EventStub current)
    {
        eventDragRect = rectangleForEvent[current];
        draggingEvent = true;
    }

    /// <summary>
    /// Gets the start position for drawing/dragging based on the rectangle's position
    /// and whether the start position should be at the top of the rect or not.
    /// </summary>
    private Vector3 GetStartDrawPosition(ContentRectangle rect, bool top)
    {
        return new Vector3(
            rect.Position.xMin + rect.Position.width / 2,
            rect.Position.yMin + (top ? 0 : rect.Position.height),
            0);
    }

    /// <summary>
    /// Continue dragging the participant rectangle.
    /// </summary>
    private void OnContinueDrag()
    {
        participantDragRect.SetLeftAndTop(currentMousePosition.x, currentMousePosition.y);
        participantDragRect.Render();
        //If we are at the border, automatically scroll.
        if (currentMousePosition.x <= scrollPosition.x + 5)
        {
            scrollPosition.x -= 5;
        }
        if (currentMousePosition.y >= scrollPosition.y + height - 5)
        {
            scrollPosition.y += 4;
        }
    }

    /// <summary>
    /// Finish dragging the participant rectangle when the mouse is released.
    /// </summary>
    private void OnEndParticipantDrag()
    {
        foreach (ContentRectangle<EventStub> rect in rectangleForEvent.Values)
        {
            for (int i = 0; i < rect.containedObject.NrOfNeededRoles; i++)
            {
                IEnumerable<Texturizer> available = texturizers.
                    Where((Texturizer t) => t.Event == rect.containedObject);
                if (rect.GetContent().image == null || available.Count() == 0)
                {
                    continue;
                }
                Rect connector = available.First().
                    GetRectForIndex(zoomLevel * WIDTH_EVENT_RECT, zoomLevel * HEIGHT_EVENT_RECT, i);
                connector.xMin = connector.xMin + rect.Position.xMin;
                connector.yMin = connector.yMin + rect.Position.yMin;
                connector.xMax = connector.xMax + rect.Position.xMin;
                connector.yMax = connector.yMax + rect.Position.yMin;
                if (participantDragRect.Position.Contains(connector.center))
                {
                    if (rect.containedObject.GetSelectorForIndex(i).
                        TrySet(participantDragRect.containedObject))
                    {
                        participantDragRect = null;
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finish dragging an event.
    /// </summary>
    private void OnEndEventDrag()
    {
        for (int i = 1; i <= maxLevel + 1; i++)
        {
            if (GetLevelCenter(i) > eventDragRect.Position.yMin && GetLevelCenter(i) < eventDragRect.Position.yMax)
            {
                manager.SetLevel(eventDragRect.containedObject, i);
            }
        }
        if (GetLevelCenter(1) > eventDragRect.Position.yMax)
        {
            Debug.Log("Doing nothing here...");
        }
        CalculateEventAndObjectPositions();
    }

    /// <summary>
    /// Continue drawing a line, or dragging an event.
    /// </summary>
    private void OnContinueDraw()
    {
        //Draw the line, noting that the startDrawPosition might not be visible anymore depending on the screen position.
        if (drawingParticipantConnection)
            DrawLine(GetStartDrawPosition(startDrawRectangle, startDrawRectangle is ContentRectangle<EventStub>), Event.current.mousePosition); 

        //check all connectors whether there is any sort of connection
        if (drawingParticipantConnection)
        {
            foreach (EventStub evnt in connectors.Keys)
            {
                foreach (ParticipantConnector conn in connectors[evnt])
                {
                    //check if there is a connector nearby, and try connecting the two 
                    if (Vector3.Distance(currentMousePosition, conn.GetPosition()) < DELTA_CONNECTOR_DIST)
                    {
                        if (conn.selector.selectedObject == participant)
                        {
                            conn.selector.TrySet(null);
                            drawingParticipantConnection = false;
                        }
                        else if (conn.selector.TrySet(participant))
                        {
                            drawingParticipantConnection = false;
                        }
                        return;
                    }
                }
            }
        }
        else if (draggingEvent)
        {
            eventDragRect.SetLeftAndTop(currentMousePosition.x, currentMousePosition.y);
        }
        //Auto scroll if we are at the edge
        if (currentMousePosition.x <= scrollPosition.x + 5)
        {
            scrollPosition.x -= 5;
        }
        if (currentMousePosition.y >= scrollPosition.y + height - 5)
        {
            scrollPosition.y += 4;
        }
    }
    #endregion

    #region Event logic
    /// <summary>
    /// Fill in the participants for the current event stub based on the set of rules defined.
    /// </summary>
    public void FillIn(EventStub evnt)
    {
        EventPopulation selected = new RoleFiller(
            manager.ToStoryArc(),
            manager.GetLevelForEvent(evnt) - 1,
            evnt.ToStoryEvent(),
            FillInOptionsSerializer.ActiveRules,
            stateSpaceManager).FillInRoles();
        //if we found a match, try setting the params. Success here is guaranteed.
        if (selected != null)
        {
            evnt.TrySetParams(selected.AsParams().Cast<SmartObject>());
        }
    }

    /// <summary>
    /// Fill in all events.
    /// </summary>
	public void FillInAll()
	{
        foreach (EventStub e in manager.EventsOrdereredByLevel)
            FillIn(e);
	}

    /// <summary>
    /// Removes the given event from the GUI and the manager.
    /// </summary>
    private void RemoveEvent(EventStub evnt, bool removeFromManager = true)
    {
        if (evnt == currentEventStub)
        {
            currentEventStub = null;
        }
        connectors.Remove(evnt);
        rectangleForEvent.Remove(evnt);
        foreach (int key in levels.Keys)
        {
            levels[key].eventsInLevel.Remove(evnt);
        }
        if (removeFromManager)
        {
            manager.RemoveEvent(evnt);
        }
        IEnumerable<Texturizer> eventTexturizers = texturizers.Where((Texturizer t) => t.Event == evnt);
        if (eventTexturizers.Count() > 0)
        {
            eventTexturizers.First().Destroy();
            texturizers.Remove(eventTexturizers.First());
        }
    }

    /// <summary>
    /// Logic for adding an event that is used both in AddEmptyEvent and AddFinishedEvent.
    /// If addToManager is true, adds the event to the AuthoredEventManager.W
    /// </summary>
    private void AddEvent(EventStub currentEvent, bool addToManager)
    {
        if (addToManager)
        {
            AuthoredEventManager.Instance.AddAuthoredEvent(currentEvent);
        }
        for (int i = 0; i < currentEvent.NrOfNeededRoles; i++)
        {
            currentEvent.GetSelectorForIndex(i).OnObjectChanged += 
                ((SmartObject o, SmartObject n) => UpdateEventParticipants(currentEvent, o, n));
        }
        connectors.Add(currentEvent, new HashSet<ParticipantConnector>());
        rectangleForEvent[currentEvent] = new ContentRectangle<EventStub>(currentEvent, false, new Rect(0, 0, 0, 0), true, EventStubContent);
        rectangleForEvent[currentEvent].Style = rectangleForEvent[currentEvent].GetContent().image == null ? GUI.skin.button : GUI.skin.GetStyle("ImageBackground");
        rectangleForEvent[currentEvent].onRightClick = this.onEventRightClick;
        rectangleForEvent[currentEvent].onLeftClick = this.onEventLeftClick;
        rectangleForEvent[currentEvent].onMouseOver = this.onEventMouseOver;
        rectangleForEvent[currentEvent].onMouseOut = this.onEventMouseOut;
        rectangleForEvent[currentEvent].RegisterLeftClickAction(StartDragEvent);
        for (int i = 0; i < currentEvent.NrOfNeededRoles; i++)
        {
            connectors[currentEvent].Add(new ParticipantConnector(i, currentEvent, this));
        }
        CalculateEventAndObjectPositions();
    }

    /// <summary>
    /// Adds a new empty event to the window.
    /// </summary>
    public void AddEmptyEvent(EventStub currentEvent)
    {
        currentEventStub = currentEvent;
        AddEvent(currentEvent, true);
    }

    /// <summary>
    /// Adds a new event to the window, where this event already has all the required parameters set.
    /// addToManager specifies whether the given event should then be added to the manager.
    /// keepCurrentLevel specifies whether the current level should be kept in the manager.
    /// </summary>
    public void AddFinishedEvent(EventStub evnt, bool addToManager)
    {
        foreach (SmartObject obj in evnt.InvolvedObjects)
        {
            AddSmartObject(obj);
        }
        AddEvent(evnt, addToManager);
    }

    /// <summary>
    /// Adds a predecessor to the given event and recalculates its level.
    /// </summary>
    public void AddPredecessorToEvent(EventStub currentEvent, EventStub predecessor)
    {
        manager.AddPredecessor(currentEvent, predecessor);
        CalculateEventAndObjectPositions();
    }

    /// <summary>
    /// Reflect an update of the given's event stubs in the GUI.
    /// </summary>
    private void UpdateEventParticipants(EventStub eventStub, SmartObject oldObj, SmartObject newObj)
    {
        CalculateEventAndObjectPositions();
    }
    #endregion

    #region Calculating positions
    /// <summary>
    /// Recalculate all event and object positions based on the changed levels.
    /// </summary>
    public void CalculateEventAndObjectPositions()
    {
        foreach (Level level in levels.Values)
        {
            level.eventsInLevel.Clear();
        }
        //clear all levels and recalculate them
        foreach (EventStub evnt in rectangleForEvent.Keys)
        {
            AddToLevel(evnt, manager.GetLevelForEvent(evnt));
        }

        //calculate the max nr of events in a level
        int max = 0;
        foreach (int key in levels.Keys)
        {
            max = Mathf.Max(max, levels[key].eventsInLevel.Count);
        }
        maxNrOfEventsInLevel = max;

        //calculate the positions of each event within a level. Also get the predecessor connectors for each event
        foreach (EventStub evnt in rectangleForEvent.Keys)
        {
            Level level = levels[manager.GetLevelForEvent(evnt)];
            int index = level.eventsInLevel.IndexOf(evnt);
            rectangleForEvent[evnt].SetPosition(new Rect(
                zoomLevel * (MARGIN_BETWEEN + index * (WIDTH_EVENT_RECT + MARGIN_BETWEEN)),
                zoomLevel * (MARGIN_TOP + HEIGHT_OBJECT_RECT + (level.level - 1) * HEIGHT_LEVEL + HEIGHT_BETWEEN),
                zoomLevel * WIDTH_EVENT_RECT, 
                zoomLevel * HEIGHT_EVENT_RECT));
        }

        //calculate the positions of each object, based on the maximum nr of events in a level
        //int objectIndex = 0;
        totalObjectWidth = 0;
        foreach (ContentRectangle<SmartObject> rect in rectangleForObject.Values)
        {
            rect.SetPosition(new Rect(
                zoomLevel * (MARGIN_BETWEEN + maxNrOfEventsInLevel * (WIDTH_EVENT_RECT + MARGIN_BETWEEN)) + totalObjectWidth,
                zoomLevel * MARGIN_TOP, 
                zoomLevel * (rect.GetContent().image == null ? WIDTH_OBJECT_RECT_V_TEXT : WIDTH_OBJECT_RECT_V_IMG),
                zoomLevel * HEIGHT_OBJECT_RECT));
            totalObjectWidth += zoomLevel * MARGIN_BETWEEN + rect.Position.width;
        }
    }

    /// <summary>
    /// Get the vertical center position of the given level.
    /// </summary>
    private float GetLevelCenter(int level)
    {
        return zoomLevel * (MARGIN_TOP + HEIGHT_OBJECT_RECT + (level - 1) * HEIGHT_LEVEL + HEIGHT_EVENT_RECT / 2 + HEIGHT_BETWEEN);
    }


    /// <summary>
    /// Adds the given event to the given level, if it is not contained yet.
    /// </summary>
    private void AddToLevel(EventStub evnt, int level)
    {
        if (!levels.ContainsKey(level))
        {
            levels[level] = new Level(level);
        }
        if (!levels[level].eventsInLevel.Contains(evnt))
        {
            levels[level].eventsInLevel.Add(evnt);
        }
    }
    #endregion

    #region Click logic

    public void RegisterLeftClickAction(Action<SmartObject> onLeftClick)
    {
        this.onObjectLeftClick.Add(onLeftClick);
    }

    public void DeregisterLeftClickAction(Action<SmartObject> onLeftClick)
    {
        this.onObjectLeftClick.Remove(onLeftClick);
    }

    public void RegisterRightClickAction(Action<SmartObject> onRightClick)
    {
        this.onObjectRightClick.Add(onRightClick);
    }

    public void DeregisterRightClickAction(Action<SmartObject> onRightClick)
    {
        this.onObjectRightClick.Remove(onRightClick);
    }
    #endregion

    #region Helper classes
    /// <summary>
    /// A class for a single level, keeping track of the events contained in it.
    /// </summary>
    private class Level
    {
        public int level;

        public List<EventStub> eventsInLevel;

        public Level(int level)
        {
            this.level = level;
            this.eventsInLevel = new List<EventStub>();
        }
    }

    /// <summary>
    /// A connector used to draw lines from.
    /// </summary>
    private abstract class Connector
    {
        protected EventStub eventStub;

        protected MainWindow window;

        protected float leftOffset;

        protected const float SELECTOR_HEIGHT = 10.0f;

        public abstract void Render();

        public Vector3 GetPosition()
        {
            return new Vector3(
                window.rectangleForEvent[eventStub].Position.xMin + this.leftOffset,
                window.rectangleForEvent[eventStub].Position.yMin - window.zoomLevel * SELECTOR_HEIGHT,
                0);
        }
    }

    private class ParticipantConnector : Connector
    {
        public SmartObjectSelector selector;

        private bool recentlyHovered;

        private int index;

        private List<ContentRectangle<SmartObject>> compatibleSmartObjects;

        public ParticipantConnector(int index, EventStub eventStub, MainWindow window)
        {
            this.index = index;
            this.eventStub = eventStub;
            this.window = window;
            this.selector = eventStub.GetSelectorForIndex(index);
            this.compatibleSmartObjects = new List<ContentRectangle<SmartObject>>();
            this.leftOffset = CalculateLeftOffset();
        }

        private float CalculateLeftOffset()
        {
            float widthBetweenParticipants = eventStub.NrOfNeededRoles == 0 ? 0 : 
                window.zoomLevel * WIDTH_PARTICIPANT_CONNECTORS / eventStub.NrOfNeededRoles;
            return window.zoomLevel * START_PARTICIPANT_CONNECTORS + (index + 0.5f) * widthBetweenParticipants;
        }

        public override void Render()
        {
            this.leftOffset = CalculateLeftOffset();
            Rect eventRect = window.rectangleForEvent[eventStub].Position;
            Vector3 connectorPosition = new Vector3(
                eventRect.xMin + leftOffset,
                eventRect.yMin,
                0);
            
            if (selector.selectedObject == null)
            {
                window.DrawLine(connectorPosition, GetPosition());
            }
            else
            {
                Rect objectRect = window.rectangleForObject[selector.selectedObject].Position;
                float heightInLevel = window.zoomLevel * (HEIGHT_CONNECTOR_LINES - index * HEIGHT_CONNECTOR_LINES / eventStub.NrOfNeededRoles);
                Vector3 middle = new Vector3(connectorPosition.x, connectorPosition.y - heightInLevel, 0);
                Color color = window.lineColorForObject[selector.selectedObject];
                window.DrawLine(connectorPosition, middle, color);
                window.DrawLine(middle, new Vector3(objectRect.xMin + objectRect.width / 2, middle.y, 0), color);
            }

            if (Vector3.Distance(window.currentMousePosition, GetPosition()) <= 10)
            {
                if (!recentlyHovered)
                {
                    compatibleSmartObjects.Clear();
                    //highlight all object rectangles that can be used for this role
                    IEnumerable<IHasState> objects =
                        ObjectManager.Instance.GetObjects().Cast<IHasState>();
                    foreach (SmartObject obj in Filter.ByState(objects, selector.MinimalRoles))
                    {
                        if (window.rectangleForObject.ContainsKey(obj) && selector.CanSet(obj))
                        {
                            ContentRectangle<SmartObject> rect = window.rectangleForObject[obj];
                            compatibleSmartObjects.Add(rect);
                            rect.Highlight();
                        }
                    }
                }
                recentlyHovered = true;
                //show a popup to display the role
                GUI.Label(new Rect(connectorPosition.x - 30, connectorPosition.y - 40, 150, 100), selector.ToString());//, Styles.LabelStyle);
            }
            else if (recentlyHovered)
            {
                foreach (ContentRectangle<SmartObject> rect in compatibleSmartObjects)
                {
                    rect.Unhighlight();
                }
                recentlyHovered = false;
            }
        }
    }
    #endregion

    #region Setting Event Color Interface
    /// <summary>
    /// Sets the rectangle for the given Event Stub to be of the given color.
    /// </summary>
    public void SetEventColor(EventStub evnt, Color color)
    {
        if (rectangleForEvent.ContainsKey(evnt))
        {
            rectangleForEvent[evnt].Highlight(color);
        }
    }

    /// <summary>
    /// Resets any color previously set for the given Event Stub.
    /// </summary>
    public void ResetEventColor(EventStub evnt)
    {
        if (rectangleForEvent.ContainsKey(evnt))
        {
            rectangleForEvent[evnt].Unhighlight();
        }
    }

    /// <summary>
    /// Retsets the event colors of the given collection of Event Stubs.
    /// </summary>
    /// <param name="events"></param>
    public void ResetEventColors(IEnumerable<EventStub> events)
    {
        foreach (EventStub evnt in events)
        {
            ResetEventColor(evnt);
        }
    }
    #endregion

    #region Event Planning
    /// <summary>
    /// Plan across the entire event space.
    /// </summary>
    private void PlanGlobal()
    {
		Rules.ClearCache ();
		StoryArc arc = manager.ToStoryArc();
		undoArc = arc;

		EventPlanner ePlanner = new EventPlanner(stateSpaceManager);
		bool success = false;
		arc = ePlanner.planGlobal(arc, rectangleForObject.Keys, OPTION, ref success);
		arc = ePlanner.depopulateArc (arc); //removes the implicit Paramteres again
		manager.ClearAllEvents();
		manager.ImportStoryArc(arc);
        HighlightPlannedEvents(ePlanner.IDs);
    }

    /// <summary>
    /// Plan locally for a given level.
    /// </summary>
    private void PlanLocal(int level)
    {
		StoryArc arc = manager.ToStoryArc();
		
		EventPlanner ePlanner = new EventPlanner(stateSpaceManager);
		bool success=false;
		arc = ePlanner.planGlobalUpTo(level+1, arc, rectangleForObject.Keys, OPTION, ref success);
		manager.ClearAllEvents();
		manager.ImportStoryArc(arc);
        HighlightPlannedEvents(ePlanner.IDs);
    }

    /// <summary>
    /// Highlight the events that were planned.
    /// </summary>
    private void HighlightPlannedEvents(IEnumerable<EventID> IDs)
    {
        foreach (EventStub evnt in manager.AllEvents)
        {
            if (IDs.Contains(evnt.ID))
            {
                AddFinishedEvent(evnt, false);
                SetEventColor(evnt, Color.cyan);
            }
        }
    }
    #endregion
}