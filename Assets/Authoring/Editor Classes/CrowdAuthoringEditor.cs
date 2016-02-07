using UnityEngine;
using System.Collections.Generic;
using TreeSharpPlus;
using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Linq;

/// <summary>
/// The main editor class, which contains logic for connecting all sub windows within the GUI.
/// While the MainWindow class has most application logic, this class glues all the different window parts
/// together.
/// </summary>
public class CrowdAuthoringEditor
{

    //Helper Function for SmartObject content string
    public static GUIContent ObjectContentString(SmartObject obj)
    {
        return new GUIContent(obj.gameObject.name + "\n" + obj.GetType().ToString());
    }

    #region Attributes
    //Height of the GUI
    private float height;

    //Width of the GUI
    private float width;

    //Width of a single side bar
    private const float WIDTH_SIDEBAR = 180;

    //Height of a single action bar
    private const float HEIGHT_ACTIONBAR = 28;

    //Height of a single notification bar
    private const float HEIGHT_NOTIFICATIONBAR = 27;

    //Height of all the bars
    public float HEIGHT_BARS;

    //Side bar for objects
    private Sidebar<SmartObject> objectSidebar;

    //Side bar for events
    private Sidebar<EventStub> eventSidebar;

    //All action bars (functioning as a stack)
    private List<ActionBar> actionBars;

    //The "newest" action bar (the only one rendered active)
    public ActionBar ActionBar { get { return actionBars[actionBars.Count - 1]; } }

    //The current notification bar
    private NotificationBar notificationBar;

    //Main Area and windows implementing IRenderable
    public IRenderable mainArea;

    //The main window. mainArea == mainWindow is not always true
    private MainWindow mainWindow;

    //Highlighter script for highlighting objects
    private Highlighter highlighter;

    /// <summary>
    /// Any action that should be done on rePaint, e.g. changes to the GUI.
    /// It is ensured, that these methods are called after everything else is rendered, and before
    /// the next rendering pass begins.
    /// </summary>
    public HashSet<Action> onRepaint = new HashSet<Action>();

    //if the constructor has an exception, sets this bool to false so
    //the GUI is not rendered (which would likely make it crash)
    private bool mustClose = false;
    
    //Boolean to indicate whether we were loading the level in the
    //last frame but are not anymore -> indicates a Reset
    //It is static, as static variables are not reset when loading a level.
    //Thus we can use it to check when Reset was used..
    //TODO Find a way to do this that is less ridicolously hacky?
    private static bool wasLoadingLevel = false;
    #endregion

    #region Standard action bars
    //Default bar shown
    private ActionBar defaultBar;

    //Bar to show when selecting a crowd
    private ActionBar selectCrowdBar;

    //Bar to show when showing options
    private ActionBar optionsBar;
    #endregion

    #region Attributes for current event
    //Selecting positional parameters (i.e. generating waypoints)
    private MouseHandler.ClickActions positionSelectAction;

    //the waypoints generated for the current event stub
    private HashSet<SmartWaypoint> newlyGeneratedWaypoints = new HashSet<SmartWaypoint>();

    //Drag and drop
    private ContentRectangle dragRectangle;

    //The rectangle for dragging an object
    private ContentRectangle<SmartObject> participantDragRectangle;

    //Are we dragging an object?
    private bool draggingParticipant;

    //The rectangle for dragging an event
    private ContentRectangle<EventStub> eventDragRectangle;

    //Are we dragging an event?
    private bool draggingEvent;

    //crowd selection
    private MouseHandler.DragActions crowdRectangleSelectAction;

    private MouseHandler.DragActions drawRectangleAction;

    private Vector3 worldPosition1;

    private bool crowdStatic;
    #endregion

    #region Create GUI
    /// <summary>
    /// Create a new CrowdAuthoringEditor with the given line renderer.
    /// </summary>
    /// <param name="renderer">The line renderer for rendering lines in the main window.</param>
    public CrowdAuthoringEditor(AbstractLineRenderer renderer)
    {
        try
        {
            highlighter = Highlighter.Instance;

            objectSidebar = new Sidebar<SmartObject>(true, ObjectContentString);
            objectSidebar.RegisterLeftClickAction(highlighter.HighlightAfterUnhighlight);
            objectSidebar.RegisterLeftClickAction(OnParticipantDragStart);
            eventSidebar = new Sidebar<EventStub>(true, (EventStub e) => new GUIContent(e.Name));
            eventSidebar.RegisterLeftClickAction(OnEventDragStart);
            eventSidebar.allObjects.UnionWith(AllEvents());
            PeriodicMethodCaller.GetInstance().StartCallPeriodically(FindAllSmartObjects, 1.0f);

            actionBars = new List<ActionBar>();
            notificationBar = new NotificationBar("This is the main view, where you can see all your upcoming events");

            defaultBar = DefaultBar();
            selectCrowdBar = SelectCrowdBar();
            optionsBar = OptionsBar();

            actionBars.Add(defaultBar);

            mainArea = mainWindow = new MainWindow(AuthoredEventManager.Instance, this, renderer);
            mainWindow.RegisterLeftClickAction(highlighter.HighlightAfterUnhighlight);

            positionSelectAction = new MouseHandler.ClickActions((RaycastHit hit) => { }, TryCreateWaypoint);
            MouseHandler.GetInstance().RegisterClickEvents(positionSelectAction);

            RectanglePainter painter = RectanglePainter.GetWithColor(Color.blue);
            drawRectangleAction = new MouseHandler.DragActions(painter.StartPainting, painter.ContinuePainting, (Vector3 pos) => painter.StopPainting());
            crowdRectangleSelectAction = new MouseHandler.DragActions(OnSelectionStart, (Vector3 pos) => { }, OnSelectionStopped);
            EventCollectionManager.Instance.SetAllCollections(EventCollectionSerializer.Instance.Load());

        }
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);
            Debug.Log(e.Message);
            Debug.Log(e.Source);
            mustClose = true;
        }
    }

    /// <summary>
    /// Execute when the GUI is being disabled, e.g. because it's closed or scene execution stops.
    /// </summary>
    public void OnDisable()
    {
        FillInOptionsSerializer.Save();
        EventCollectionSerializer.Instance.Save(EventCollectionManager.Instance.AllCollections);
    }
    #endregion

    #region Rendering

    /// <summary>
    /// Renders the GUI in a box of the given width and height.
    /// </summary>
    public void Render(float width, float height)
    {
        //If we used Reset before, reload the stored narrative (hacky..)
        if (wasLoadingLevel && !Application.isLoadingLevel)
        {
            ImporterExporter.TryLoad(StoryArcSerializer.TEMP_FILE_NAME);
        }
        //Are we loading the level? this is only true when we actually reset the level.
        wasLoadingLevel = Application.isLoadingLevel;

        this.width = width;
        this.height = height;

        //Did something go wrong?
        if (mustClose)
        {
            GUILayout.Label("Something went wrong.");
            GUILayout.Label("There seems to have been an exception. Why don't you check the debug log?");
            return;
        }

        //Can only press play if all params are set.
        this.GetMainActionBar().SetActive("Play", AllParamsSet());

        HEIGHT_BARS = HEIGHT_NOTIFICATIONBAR + HEIGHT_ACTIONBAR * actionBars.Count;

        //drag logic
        if (draggingParticipant || draggingEvent)
        {
            OnDragContinue();
        }
        if (draggingParticipant && Event.current.type == EventType.mouseUp)
        {
            onRepaint.Add(OnParticipantDragEnd);
        }
        if (draggingEvent && Event.current.type == EventType.mouseUp)
        {
            onRepaint.Add(OnEventDragEnd);
        }

        //Render all the different parts in their areas
        GUILayout.BeginArea(new Rect(0, HEIGHT_BARS, width - 2 * WIDTH_SIDEBAR, height - HEIGHT_BARS));
        mainArea.Render(height - HEIGHT_BARS, width - 2 * WIDTH_SIDEBAR);
        GUILayout.EndArea();

        for (int i = 0; i < actionBars.Count; i++)
        {
            GUILayout.BeginArea(new Rect(0, HEIGHT_NOTIFICATIONBAR + i * HEIGHT_ACTIONBAR, width, HEIGHT_ACTIONBAR));
            actionBars[i].Render(i == actionBars.Count - 1);
            GUILayout.EndArea();
        }

        GUILayout.BeginArea(new Rect(width - 2 * WIDTH_SIDEBAR, HEIGHT_BARS, WIDTH_SIDEBAR, height - HEIGHT_BARS));
        eventSidebar.Render();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(width - WIDTH_SIDEBAR, HEIGHT_BARS, WIDTH_SIDEBAR, height - HEIGHT_BARS));
        objectSidebar.Render();
        GUILayout.EndArea();

        if (participantDragRectangle != null)
        {
            participantDragRectangle.Render();
        }
        if (eventDragRectangle != null)
        {
            eventDragRectangle.Render();
        }
        
        //The notification bar shows tooltips, so render it last (uses GUI.tooltip).
        GUILayout.BeginArea(new Rect(0, 0, width, HEIGHT_NOTIFICATIONBAR));
        notificationBar.Render();
        GUILayout.EndArea();

        //If we execute all onRepaint events here, we can make sure they are available
        //in the next rendering pass, and don't change anything in the current pass.
        if (Event.current.type == EventType.Repaint)
        {
            foreach (Action action in onRepaint)
            {
                action.Invoke();
            }
            onRepaint.Clear();
        }
    }

    #endregion

    #region Reset
    /// <summary>
    /// Resets the current selection. As there is no information in which state the GUI was previously, must do a full set
    /// of reset action, even if maybe not all are necessary.
    /// </summary>
    public void Reset()
    {
        notificationBar = new NotificationBar("This is the main view, where you can see all your upcoming events");
        actionBars.Clear();
        actionBars.Add(defaultBar);
        MouseHandler.GetInstance().DeregisterDragEvents(drawRectangleAction);
        MouseHandler.GetInstance().DeregisterDragEvents(crowdRectangleSelectAction);
        selectCrowdBar.RemoveButton("Set Scheduler");
        selectCrowdBar.RemoveButton("Set And Save Scheduler");
        mainArea = mainWindow;
    }
    #endregion

    #region API
    /// <summary>
    /// Sets the current NotificationBar to the given bar.
    /// </summary>
    public void SetNotificationBar(NotificationBar bar)
    {
        this.notificationBar = bar;
    }

    /// <summary>
    /// Adds a new ActionBar.
    /// </summary>
    public void AddActionBar(ActionBar bar)
    {
        this.actionBars.Add(bar);
    }

    /// <summary>
    /// Pops the current ActionBar.
    /// </summary>
    public void PopActionBar()
    {
        this.actionBars.RemoveAt(this.actionBars.Count - 1);
    }

    /// <summary>
    /// Gets the main ActionBar, i.e. the one always displayed highest on the screen.
    /// </summary>
    public ActionBar GetMainActionBar()
    {
        return this.actionBars[0];
    }
    #endregion

    #region Parameter selection for event stubs
    /// <summary>
    /// If the Event uses SmartWaypoints, these can be automatically generated by right clicking in the world.
    /// The first open waypoint slot in the Event is then filled with the generated waypoint.
    /// </summary>
    private void TryCreateWaypoint(RaycastHit hit)
    {
        //can only add waypoints to the current event stub.
        if (mainWindow.currentEventStub != null)
        {
            SmartWaypoint newWP = SmartWaypoint.GenerateWaypoint(hit.point);
            //check if an empty slot needs a SmartWaypoint as generated
            int slot = -1;
            for (int index = 0; index < mainWindow.currentEventStub.NrOfNeededRoles; index++)
            {
                if (mainWindow.currentEventStub.GetSelectorForIndex(index).selectedObject == null &&
                    mainWindow.currentEventStub.GetSelectorForIndex(index).CanSet(newWP))
                {
                    slot = index;
                    break;
                }
            }
            //slot >= 0 -> an empty slot needs a SmartWaypoint
            if (slot >= 0)
            {
                newlyGeneratedWaypoints.Add(newWP);
                mainWindow.currentEventStub.GetSelectorForIndex(slot).TrySet(newWP);
                highlighter.HighlightAfterUnhighlight(newWP);
            }
            else
            {
                DestroyCreatedWaypoint(newWP);
            }
        }
    }

    /// <summary>
    /// Checks if the old object was a generated waypoint and destroys it if it is not used anywhere.
    /// </summary>
    private void CheckForWaypoint(SmartObject old)
    {
        if (old != null && old is SmartWaypoint &&
            ((SmartWaypoint)old).IsSystemGenerated &&
            AuthoredEventManager.Instance.GetInvolvedInEvents(old).Count == 0)
        {
            DestroyCreatedWaypoint((SmartWaypoint)old);
            mainWindow.RemoveSmartObject(old);
        }
    }

    /// <summary>
    /// Destroys a generated waypoint. Careful: Also destroys non-generated
    /// waypoints, make sure to only call method with generated ones.
    /// </summary>
    private void DestroyCreatedWaypoint(SmartWaypoint waypoint)
    {
        newlyGeneratedWaypoints.Remove(waypoint);
        ObjectManager.Instance.DeregisterSmartObject(waypoint);
        GameObject.Destroy(waypoint.gameObject);
    }
    #endregion

    #region Drag and drop
    /// <summary>
    /// Executed when dragging a SmartObject from the sidebar is started.
    /// </summary>
    private void OnParticipantDragStart(SmartObject obj)
    {
        dragRectangle = participantDragRectangle = new ContentRectangle<SmartObject>(obj, false, 
            new Rect(Event.current.mousePosition.x + width - WIDTH_SIDEBAR, Event.current.mousePosition.y + HEIGHT_BARS, 120, 60), true, ObjectContentString);
        draggingParticipant = true;
    }
    
    /// <summary>
    /// Executed when dragging an Event from the sidebar is started.
    /// </summary>
    private void OnEventDragStart(EventStub evnt)
    {
        dragRectangle = eventDragRectangle = new ContentRectangle<EventStub>(evnt, false, 
            new Rect(Event.current.mousePosition.x + width - 2 * WIDTH_SIDEBAR, Event.current.mousePosition.y + HEIGHT_BARS, 120, 60), true, (EventStub e) => new GUIContent(e.Name));
        draggingEvent = true;
    }

    /// <summary>
    /// Executed when the mouse is released after dragging of a SmartObject started.
    /// </summary>
    private void OnParticipantDragEnd()
    {
        //check if the rectangle is in the eventstub details window, if yes, add it
        if (participantDragRectangle.Position.Overlaps(new Rect(0, HEIGHT_BARS, width - 2 * WIDTH_SIDEBAR, height - HEIGHT_BARS)))
        {
            mainWindow.AddSmartObject(participantDragRectangle.containedObject);
        }
        participantDragRectangle = null;
        draggingParticipant = false;
    }

    /// <summary>
    /// Executed when the mouse is released after dragging of an Event started.
    /// </summary>
    private void OnEventDragEnd()
    {
        if (eventDragRectangle.Position.Overlaps(new Rect(0, HEIGHT_BARS, width - 2 * WIDTH_SIDEBAR, height - HEIGHT_BARS)))
        {
            EventStub toAdd = new EventStub(eventDragRectangle.containedObject);
            mainWindow.AddEmptyEvent(toAdd);
            for (int i = 0; i < toAdd.NrOfNeededRoles; i++)
            {
                toAdd.GetSelectorForIndex(i).OnObjectChanged += 
                    (SmartObject o, SmartObject n) => CheckForWaypoint(o);
            }
        }
        eventDragRectangle = null;
        draggingEvent = false;
    }

    /// <summary>
    /// Executed when drag of Event/SmartObject has started, and the mouse is not yet released.
    /// </summary>
    private void OnDragContinue()
    {
        dragRectangle.SetLeftAndTop(Event.current.mousePosition.x, Event.current.mousePosition.y);
    }
    #endregion

    #region Crowd selection
    /// <summary>
    /// Starts the crowd selection logic, i.e. selecting a new crowd in the scene.
    /// </summary>
    private void StartCrowdSelection()
    {
        notificationBar = new NotificationBar("Select a crowd by creating a rectangle in the scene or selecting the individual objects.");
        actionBars.Add(selectCrowdBar);
        MouseHandler.GetInstance().RegisterDragEvents(drawRectangleAction);
        MouseHandler.GetInstance().RegisterDragEvents(crowdRectangleSelectAction);
    }

    /// <summary>
    /// Logic for starting the drawingLine when selecting a crowd.
    /// </summary>
    private void OnSelectionStart(Vector3 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            worldPosition1 = hit.point;
        }
    }

    /// <summary>
    /// Logic for stopping the drawingLine when selecting a crowd.
    /// </summary>
    private void OnSelectionStopped(Vector3 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit) && worldPosition1 != new Vector3())
        {
            //Create a criteria based on selected positons, and then create the event scheduler
            GroundRectangle rect = new GroundRectangle(worldPosition1, hit.point);
            SpatialAreaCriteria criteria = new SpatialAreaCriteria(rect);
            highlighter.HighlightAfterUnhighlight(SmartCrowdCriteria.AllSatisfyingCriteria(criteria));
            CreateEventScheduler(criteria);
        }
    }

    /// <summary>
    /// Opens the window to create a new EventScheduler for the selected crowd.
    /// </summary>
    private void CreateEventScheduler(ISmartCrowdCriteria criteria)
    {
        EventSchedulerWindow schedulerSetup = new EventSchedulerWindow();
        mainArea = schedulerSetup;
        notificationBar = new NotificationBar("You have selected a crowd. See the game view for the contained objects. You can now create an EventScheduler for the crowd or cancel.");
        MouseHandler.GetInstance().DeregisterDragEvents(crowdRectangleSelectAction);
        MouseHandler.GetInstance().DeregisterDragEvents(drawRectangleAction);
        ActionBar.AddButton("Set And Save Scheduler", () =>
        {
            Debug.Log("Exectuting Set and save");
            CrowdGenerator.Instance.GenerateCrowd(criteria, crowdStatic, schedulerSetup.ToEventCollection(true));
            Reset();
        });
        ActionBar.AddButton("Set Scheduler", () =>
        {
            CrowdGenerator.Instance.GenerateCrowd(criteria, crowdStatic, schedulerSetup.ToEventCollection(false));
            Reset();
        });
    }

    #endregion

    #region Sidebars
    /// <summary>
    /// Finds all smart objects in the scene and adds them to the Smart Object objectSidebar.
    /// </summary>
    private void FindAllSmartObjects()
    {
        objectSidebar.allObjects.UnionWith(ObjectManager.Instance.GetObjects());
        objectSidebar.allObjects.IntersectWith(ObjectManager.Instance.GetObjects());
    }
    #endregion

    #region Default action bars
    /// <summary>
    /// The default bar, shown when the GUI is first opened,and when event selection is finished.
    /// </summary>
    private ActionBar DefaultBar()
    {
        ActionBar result = new ActionBar(true);
        result.AddButton(new GUIContent("Select Crowd", "Select a crowd by spatial parameters"), StartCrowdSelection);
        result.AddButton(new GUIContent("Options", "Change options for FillIn and ordering of new Events"), 
            () => actionBars.Add(optionsBar));
        result.AddButton(new GUIContent("Clear", "Clear all Events and SmartObjects from the window"), 
            () => { mainWindow.Clear(); mainArea = mainWindow = mainWindow.Copy(); });
        result.AddButton(new GUIContent("Play", "Play the scene. Changes can not be made anymore once playback starts"),
            () => { highlighter.UnhighlightAll(); mainWindow.Play();  });
        result.AddButton("Load/Save", StartLoadSave);
        result.AddButton("Reset", () =>
        {
            StoryArcSerializer.Instance.Delete(StoryArcSerializer.TEMP_FILE_NAME, false);
            ImporterExporter.TrySaveToTemp();
            //clear all events there, otherwise it tries using destroyed smart objects 
            AuthoredEventManager.Instance.ClearAllEvents();
            //must clear all receivers from the behavior manager, else if a tree is running everything crashes
            BehaviorManager.Instance.ClearReceivers();
            //must deregister all current objects here, otherwise they stay in manager as null objects..
            foreach (SmartObject obj in new List<SmartObject>(ObjectManager.Instance.GetObjects()))
            {
                 ObjectManager.Instance.DeregisterSmartObject(obj);
            }
            Application.LoadLevel(Application.loadedLevel);
        });

        
        return result;
    }

    /// <summary>
    /// Default option bar displaying options for filling in of roles.
    /// </summary>
    private ActionBar OptionsBar()
    {
        ActionBar result = FillInOptionsSerializer.ToActionBar(true);
        //result.AddToggle(new GUIContent("Keep Tier", "Manually specify event location"), 
        //    (bool val) => mainWindow.KeepCurrentLevel = AuthoredEventManager.Instance.keepCurrentLevel = val);
        result.AddButton("Save", () => actionBars.RemoveAt(actionBars.Count - 1));
        result.AddToggle("Show Lines", (bool val) => mainWindow.ShowLines = val, false);
        return result;
    }

    /// <summary>
    /// The default bar for selecting a new crowd.
    /// </summary>
    private ActionBar SelectCrowdBar()
    {
        ActionBar result = new ActionBar(true);
        result.AddToggle("Crowd Static", (bool b) => crowdStatic = b);
        result.AddButton("Cancel", Reset);
        return result;
    }
    #endregion

    #region All Events
    /// <summary>
    /// Returns all EventStubs for the current EventLibrary that can be 
    /// generated.
    /// </summary>
    private HashSet<EventStub> AllEvents()
    {
        HashSet<EventStub> result = new HashSet<EventStub>();
        if (EventLibrary.Instance != null)
        {
            IEnumerable<EventSignature> signatures = EventLibrary.Instance.GetSignatures();
            foreach (EventSignature signature in signatures)
            {
                //Do not include events with a HideInGUI attribute..
                if (signature.GetAttributes<HideInGUIAttribute>().Length > 0 &&
                    signature.GetAttributes<HideInGUIAttribute>()[0].Indices.Contains(EventLibrary.Instance.LibraryIndex))
                {
                    continue;
                }
                EventStub stub;
                if (EventStub.TryGetEventStub(signature, out stub))
                {
                    result.Add(stub);
                }
            }
        }
        return result;
    }
    #endregion

    #region Load/Save
    /// <summary>
    /// Creates the load/save window.
    /// </summary>
    private void StartLoadSave()
    {
        ImportExportWindow window = new ImportExportWindow();
        this.mainArea = window;
        this.AddActionBar(new ActionBar(true));
        this.ActionBar.AddButton("Cancel", Reset);
        this.ActionBar.AddButton("Load", () => { window.TryImport(); Reset(); });
        this.ActionBar.AddButton("Save", () => { window.TryExport(); Reset(); });
    }
    #endregion

    /// <summary>
    /// Returns whether all the explicit params of all events have been set.
    /// </summary>
    /// <returns></returns>
    private bool AllParamsSet()
    {
        bool result = true;
        foreach (EventStub evnt in AuthoredEventManager.Instance.AllEvents)
        {
            result = result && evnt.NrOfNeededRoles == evnt.InvolvedObjects.Count();
        }
        return result;
    }
}
