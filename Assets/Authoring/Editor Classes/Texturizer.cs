using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A class which can be used to create render textures for event stubs that can then be displayed in the GUI.
/// </summary>
public class Texturizer 
{
    //The width of an image to be used for this must be 1920 pixels to work correctly.
    private const float IMG_WIDTH = 1920;

    //And the height must be 1080 pixels.
    private const float IMG_HEIGHT = 1080;

    //The width of a single parameter image (e.g. a guard) in pixels
    private const float PARAM_IMG_WIDTH = (PARAM_CUBE_WIDTH / IMG_CUBE_WIDTH) * IMG_WIDTH;

    //The height of a single parameter image (e.g. a guard) in pixels
    private const float PARAM_IMG_HEIGHT = (PARAM_CUBE_HEIGHT / IMG_CUBE_HEIGHT) * IMG_HEIGHT;

    //The width of the image cube in world space.
    private const float IMG_CUBE_WIDTH = IMG_CUBE_HEIGHT * 16.0f / 9.0f;
    
    //The height of the image cube in world space
    private const float IMG_CUBE_HEIGHT = 10.0f;

    //The width of a parameter cube in world space
    private const float PARAM_CUBE_WIDTH = PARAM_CUBE_HEIGHT * 3.0f / 4.0f;

    //The height of a parameter cube in world space;
    private const float PARAM_CUBE_HEIGHT = 3.15f;

    //The z offset for the camera/cubes to allow many images in a scene.
    private static float zOffset = 0.0f;

    //The event to make a texture for
    public EventStub Event { get; private set; }

    //The cube to project the image on
    private GameObject imageCube;

    //The camera rendering to texture
    private Camera camera;

    //Does this Texturizer instance have a texture?
    private bool hasTexture;

    //The current render texture, exists only if hasTexture is true.
    private RenderTexture currentTex;

    //The cubes to project the parameter images on
    private List<GameObject> paramCubes = new List<GameObject>();

    //A default material for empty param slots
    private Material emptySlotMat;

    //A default material for filled param slots
    private Material defaultSlotMat;

    //The offsets for the param images in pixels from the main image
    private ImageOffsets offsets;

    /// <summary>
    /// Creates a new Texturizer for the given event.
    /// </summary>
    /// <param name="evnt">Event to create a Texturizer for.</param>
    public Texturizer(EventStub evnt)
    {
        this.Event = evnt;
        Material mat = Resources.Load<Material>(
            EventFolderName(evnt) + "/Mat");
        TextAsset offsetsFile = Resources.Load<TextAsset>(
            EventFolderName(evnt) + "/Offsets");
        //only if we have a material and an offsets file can we create a texture.
        if (mat != null && offsetsFile != null)
        {
            //set up the camera at a fixed 16:9 aspect ratio
            camera = new GameObject().AddComponent<Camera>();
            camera.transform.position = new Vector3(0, -100, 0 + zOffset);
            camera.backgroundColor = Color.black;
            camera.aspect = 16.0f / 9.0f;
            camera.orthographic = true;
            //create a render texture to which the camera renders
            currentTex = new RenderTexture(800, 450, 24);
            currentTex.Create();
            camera.targetTexture = currentTex;
            //create a single cube to hold the event image's material
            imageCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            imageCube.transform.localScale = new Vector3(IMG_CUBE_WIDTH, IMG_CUBE_HEIGHT, 0.01f);
            imageCube.transform.position = new Vector3(0, -100, 0.7f + zOffset);
            imageCube.transform.Rotate(Vector3.forward, 180.0f);
            imageCube.GetComponent<MeshRenderer>().material = mat;
            emptySlotMat = Resources.Load<Material>("EventThumbnails/ObjMat_Default/Mat_Empty");
            defaultSlotMat = Resources.Load<Material>("EventThumbnails/ObjMat_Default/Mat_Default");
            //create param cubes at the correct offset
            offsets = LitJson.JsonMapper.ToObject<ImageOffsets>(offsetsFile.text);
            if (offsets.Offsets.Length < evnt.NrOfNeededRoles)
            {
                //do nothing, else we crash as we don't have enough offsets
            }
            else
            {
                //use the offsets to place the param cubes at the correct position
                for (int i = 0; i < evnt.NrOfNeededRoles; i++)
                {
                    int index = i;
                    evnt.GetSelectorForIndex(i).OnObjectChanged +=
                        (SmartObject o, SmartObject n) => SetCube(index);
                    paramCubes.Add(CreateParamCube(offsets.Offsets[i]));
                    SetCube(i);
                }
            }
        }
        this.hasTexture = mat != null && offsetsFile != null && offsets.Offsets.Length >= evnt.NrOfNeededRoles;
        zOffset += 1.0f;
    }

    /// <summary>
    /// Create a cube for a single parameter residing at the given offset.
    /// </summary>
    /// <param name="offset">The offset from the left/top edge of the image in pixels.</param>
    /// <returns>A new param cube.</returns>
    private GameObject CreateParamCube(Offset offset)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = new Vector3(PARAM_CUBE_WIDTH, PARAM_CUBE_HEIGHT, 0.01f);
        cube.transform.position = new Vector3(
            0.0f - IMG_CUBE_WIDTH / 2 + PARAM_CUBE_WIDTH / 2, 
            -100.0f + IMG_CUBE_HEIGHT / 2 - PARAM_CUBE_HEIGHT / 2, 
            0.699f + zOffset);
        cube.transform.position = new Vector3(
            cube.transform.position.x + ((IMG_CUBE_WIDTH) / IMG_WIDTH) * offset.x,
            cube.transform.position.y - ((IMG_CUBE_HEIGHT) / IMG_HEIGHT) * offset.y,
            cube.transform.position.z);
        cube.transform.Rotate(Vector3.forward, 180.0f);
        return cube;
    }

    /// <summary>
    /// Set the param cube at the given index's image to the one used by the SmartObject at the given index
    /// of the event.
    /// </summary>
    /// <param name="index">The cube/SmartObjcet index.</param>
    private void SetCube(int index)
    {
        SmartObjectSelector sel = Event.GetSelectorForIndex(index);
        if (sel.selectedObject == null)
        {
            //disable the renderer if no object found
            paramCubes[index].GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            //enable the renderer else and check if the object has a portrait, or use the default
            //material instead.
            paramCubes[index].GetComponent<MeshRenderer>().enabled = true;
            SmartObject newObj = sel.selectedObject;
            Texture2D tex = newObj.Portrait;
            Material mat = null;
            if (tex != null)
            {
                mat = new Material(Shader.Find("Unlit/Texture"));
                mat.mainTexture = tex;
            }
            mat = mat == null ? defaultSlotMat : mat;
            paramCubes[index].GetComponent<MeshRenderer>().material = mat;
        }
    }

    /// <summary>
    /// The folder name where the Material and the Offsets file for the given event stub is stored.
    /// </summary>
    /// <param name="evnt">Event Stub to find folder for.</param>
    /// <returns>The path to the folder within the Resources directory.</returns>
    private string EventFolderName(EventStub evnt)
    {
        IconNameAttribute[] names = evnt.MainSignature.GetAttributes<IconNameAttribute>();
        if (names.Length > 0)
        {
            return "EventThumbnails/" + names[0].Name;
        }
        return "EventThumbnails/Evnt_" + evnt.Name.Replace(" ", "");
    }

    /// <summary>
    /// Gets the rectangle for the given parameter when the image is displayed
    /// in the given displayWidth and displayHeight.
    /// </summary>
    public Rect GetRectForIndex(float displayWidth, float displayHeight, int index)
    {
        return new Rect(
            offsets[index].x * (displayWidth / IMG_WIDTH),
            offsets[index].y * (displayHeight / IMG_HEIGHT),
            (PARAM_CUBE_WIDTH / IMG_CUBE_WIDTH) * displayWidth,
            (PARAM_CUBE_HEIGHT / IMG_CUBE_HEIGHT) * displayHeight);
    }

    /// <summary>
    /// Returns whether a render texture was created for the last set event.
    /// </summary>
    public bool HasTexture()
    {
        return hasTexture;
    }

    /// <summary>
    /// Returns the current render texture.
    /// </summary>
    public RenderTexture GetTexture()
    {
        return currentTex;
    }

    /// <summary>
    /// Returns a 2D texture with the contents of the current render texture.
    /// </summary>
    public Texture2D ToTexture2D()
    {
        RenderTexture.active = currentTex;
        Texture2D res = new Texture2D(800, 450);
        res.ReadPixels(new Rect(0, 0, 800, 450), 0, 0);
        res.Apply();
        RenderTexture.active = null;
        return res;
    }

    /// <summary>
    /// Destroys all data associated with this Texturizer instance.
    /// </summary>
    public void Destroy()
    {
        if (this.hasTexture)
        {
            GameObject.Destroy(camera.gameObject);
            GameObject.Destroy(imageCube);
            GameObject.Destroy(currentTex);
            paramCubes.ForEach((GameObject obj) => GameObject.Destroy(obj));
        }
    }

    #region Helper classes
    public class ImageOffsets
    {
        public Offset[] Offsets { get; set; }
        public Offset this[int index] { get { return Offsets[index]; } }
        public ImageOffsets() { }
    }
    public class Offset
    {
        public int x { get; set; }
        public int y { get; set; }
        public Offset() { }
    }
    #endregion
}
