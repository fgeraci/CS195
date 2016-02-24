Please, do not push assets, only code, controllers, scenes, terrains etc.

README

First of all, we provide a build of the main bank scene for both Windows and Mac. This build is fully
functional with a fully configured scene and various events and has been tested. For non-experts, this
is the easiest entry point to get started with creating narratives. For the user interface documentation,
please refer to the USER INTERFACE section within this README file.

Apart from the builds for Mac/Windows, we also provide a Unity project which must be opened with the 
Unity Engine. See the below README to get started on using the project.

MAIN SCENE: We recommend using the VideoFullBank scene (just search the name in Unity). It offers
many fully configured characters and other objects. Also take it as a help on how to configure your
own characters and objects.

CHARACTERS: Smart Characters are the most complicated to set up. Generally, they need the following
components:
	- Animator (Use the CharacterController) for animations
	- CrossFadeFBBIKSettings for the IK
	- IKController
	- CrossfadeFBBIK (2 times), as the IK controller has a primary and secondary IK script
	- Crossfade Look At IK for the head look
	- Character Controller
	- Nav Mesh Agent
	- Unity Steering Controller
	- _NavigatorScript (or the characters won't animate properly)
	- Character-/Body-/Behavior Mecanim
	- Smart Character script itself
	- AffordanceRunner
	- Starting Tags to initialize the state
	- Assign ID to assign a unique ID to use
	- Highlight for proper highlighting
	- Set Orientation

For documentation on the IK system, please refer to www.root-motion.com/final-ik.html

The Smart Character script then needs additional setup:
	- Interaction Targets: These are used by the IK system, e.g. for reaching. Refer to the above IK
      system documentation on how to configure them
	- Portrait: A 2D texture if you want to display an image in the GUI
	- Status Icon: A plane to display some images on
	- Waypoint Back/Front: Waypoints to approach character from the front and back
	- Prop Holders: These are used to hold props (e.g. wallet, forms, guns, ...) The hand prop holders
      usually require the actual snap point to be a child of the respective hand bone. Check the 
      configured characters as inspiration.
    - Decoration Stars: To display stars when eing incapacitated.

OTHER SMART OBJECTS:

A general Smart Object usually needs at least:
	- Some SmartObject script
	- Starting Tagds to initialize state
	- Assign ID to assign a unique ID to use
    - Highlight if there is highlightable geometry

Other Smart Objects also often have Prop Holders/Interaction Targets/Status Icons and Portraits.
Again, see some other objects as inspiration (e.g. the DrinkDispenser, the BankCounters, ...)

USER INTERFACE:

To use the user interface, the scene should contain some scripts, particularly the GUIInEngineVersion,
a Highlighter, and an EventLibrary (so that there are actually events). A prefab with these scripts can
be found in Authoring/Prefabs -> GUIManager.prefab.

We also provide a user interface, that can be accessed both as an editor and an in-engine version. Note
that some features are untested in the editor version, so using the in-engine version is preferrable.
The GUI can also be used in the build environments. It offers the following features:
	- Crowd Selection: Press "Select Crowd" to select a spatial crowd and configure its event scheduler.
	- Options: Some options for parameter filling, and whether to show lines in the main panel are available.
	- Clear: Clears the main panel of all events/objects.
	- Play: Plays the currently authored narrative, can only be pressed when all events are filled
	- Load/Save: Load/Save a narrative
	- Reset: Resets the scene, keeps the narrative (NOTE: does not work in editor version)
	- Add Termination Dependency: Add termination edges to the main panel.
	- Plan Globally: Do validation of the entire story and fill in missing pieces.
	- Fill In All: Fill in all empty parameters.

Crowd selection: It is recommended to be looking at the ground at roughly a 90 degree angle when selecting
a spatial area for a crowd. After selecting the spatial area, the event scheduler can be configured.
Here you have the following options:
	- If you have already created and saved event schedulers before, you can select one from a set of
	 radio buttons
	- All available events (i.e. the ones with no state changes) are displayed in a list. To use them,
      activate them and enter a probability greater than 0. 
	- You can normalize the probabilities so their sum is 1. This is automatically done when you set the
	  scheduler as well.
	- If you have selected an event scheduler that you previously saved, delete it with "Delete Current".
	- Event schedulers are saved by name. If you want to save yours, enter a Name in the top textbox.
	- Enter the time between updates (in seconds) for the event scheduler, so it will try to update
      with the given frequency (but note that it will not update more often than the frame rate).
	- After configuration, set the scheduler, and save it if wanted (make sure to assign a name before
      if saving it). Saving will overwrite an existing scheduler of the same name.
	- Select the "Crowd Static" check box if the crowd's members should only be computed once.

To start authoring a narrative, drag and drop events and objects from the two sidebars to the right
(left = events, right  = objects) and begin connecting them to populate events. Note that the sidebars
can be filtered by text, and the object sidebar can also be filtered by the type of smart object that
you want to use.

Right clicking on an event in the main panel offers a context menu with the following options:
	- Cancel closes the menu
	- Clear params removes all parameters from the event
	- Remove removes the event from the panel
	- Fill In tries filling in empty parameters.
	- Highlight highlights the event in GUI
	- Camera offers camera configuration for the event.

Clicking the "Camera" button in the context menu takes you to the camera configuration panel. Here
you have the following options for configuring the camera for this event:
	- Number of arguments: How often should camera config change during the event?
	- Position type: Fixed means camersa stays at fixed position, offset means camera follows
      its target, FixedOffset means camera stays at fixed position, but fixed position calculated
	  weith offset from target
	- Rotation type: Fixed means rotation is fixed, LookAt will look at the target instead
	- Center of attention: The object used for offset/lookat target
	- Fade targets: Targets that cause the walls to fade (might not be applicable for all scenes, as
	  scripts created for bank scene)
	- Offset/position: Select the offset/position (or use current camera position)
	- Rotation: Select rotation if type is Fixed (or use current rotation)
	- Time after event: How long after the event start should this config come in effect?
	- Smoothness: Smoothness of transitions. 0 = no transition, higher than framerate = instant
	
PLANNING:

<<<<<<< .mine
Note that the system only uses objects for planning which were dragged into the authoring area before.
To change that, you can change the attribute OPTION in the MainWindow class to 'PlanSpace.All' instead of
'PlanSpace.Reduced'.

=======
EVENT AND OBJECT IMAGES:

By default, the GUI will display text boxes for both the smart objects and the events. However, it is
possible to configure the events and images so the GUI displays images for both, as can be seen in 
almost all events used in the bank scene. This can be done as follows:
	- Events: Setting up images for events is the more complicated of the two. Go to 
	  Resourves/EventThumbnails to see a setup for the currently available images. There are various
	  folders called Evnt_(Event name). When selecting one, we see it consist of three parts:
		- Img: The image itself
		- Mat: A material (usually with shader unlit/texture) containing the image
		- Offsets: A JSON file
	  Trying to use a new image for an Event called "NewEvent1" requires the following steps:
		- Make sure the image is of size 1920 x 1080 pixels. Otherwise, some things will not work properly.
		- Create a folder Evnt_NewEvent1 in EventThumbnails (or create a folder Foo and use a 
		  NameIcon attribute for the event, passing it the name Foo as argument).
		- In this folder, set up a material with the image and call it "Mat".
		- Then set up a JSON file called "Offsets.json". See other of the Offsets file for examples
		  of the JSON structure. Essentially, each explicit event parameter should have one box for its
		  image. In Offsets.json, specify the left/top border of that event box for the participant
		  image to use, for each of the boxes. If the number of explicit participants and number of
		  boxes specified do not match, the image will not be displayed.

Setting up the images for the smart objects is easier. These must be in format 3:4, but don't need
a specific exact resolution. Simply assign the image to use to the smart object's "Portrait" slot
in the inspector, and the configuration is done. If Smart Crowds should have an image, create a new
GameObject with a CrowdGenerator script attached, and assign the Portrait variable there (see the 
class documentation).
>>>>>>> .r34755
