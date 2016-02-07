using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatusIcon : MonoBehaviour
{
	public bool FaceCamera = true;

	/// <summary>
	/// Path in the Resources/ directory where icon textures can be found
	/// </summary>
	private const string ICON_PATH = "Icons";

	/// <summary>
	/// Manages the loaded icons and fetches them by name
	/// </summary>
	private static IconResourceManager manager = null;

	/// <summary>
	/// The current icon name
	/// </summary>
	public string Icon {
		set {
			if (value == null)
				this.ClearIcon ();
			else
				this.SetIcon (value);
		}
	}

	/// <summary>
	/// Crude singleton class for loading and managing icon resources
	/// </summary>
	private class IconResourceManager
	{
		private Dictionary<string, Texture2D> textures = null;

		/// <summary>
		/// Loads all of the icon resources from the given path
		/// </summary>
		/// <param name="path"></param>
		public IconResourceManager (string path)
		{
			Texture2D[] icons = Resources.LoadAll<Texture2D> (path);
			// Debug.LogWarning("ICONRESOURCEMANAGER YO");
			if (icons.Length == 0)
				Debug.LogWarning ("No status icon textures found in Resources/" + path);

			this.textures = new Dictionary<string, Texture2D> ();
			foreach (Texture2D icon in icons)
				this.textures.Add (icon.name, icon);
		}

		/// <summary>
		/// Gets the icon with the specified name from the loaded icons
		/// </summary>
		/// <returns>The icon texture if found, otherwise null</returns>
		public Texture2D GetIcon (string name)
		{
			
			Texture2D result = null;
			if (this.textures.TryGetValue (name, out result) == true)
				return result;

			Debug.LogError ("No status icon found with name: " + name);
			return null;
		}
	}

	/// <summary>
	/// Starts by disabling the icon plane and initializing the resource
	/// manager (if the manager singleton has not yet been initialized)
	/// </summary>
	void Start ()
	{
		this.GetComponent<Renderer>().enabled = false;
		if (StatusIcon.manager == null)
			StatusIcon.manager = new IconResourceManager (ICON_PATH);
	}

	/// <summary>
	/// Orients the icon plane towards the camera
	/// </summary>
	void Update ()
	{
		if (this.FaceCamera == true) {
			// Getting a NullReferenceException here? Make sure your camera is
			// tagged as "MainCamera" in your Unity scene.
			Quaternion direction =
                Quaternion.LookRotation (
                    Camera.main.transform.position - transform.position);

			direction.x = 0.0f;
			direction.z = 0.0f;
			transform.rotation = direction;
			transform.Rotate (new Vector3 (90.0f, 0.0f, 0.0f));
		}
	}

	/// <summary>
	/// Sets the icon texture based on a given name
	/// </summary>
	void SetIcon (string iconName)
	{

		this.GetComponent<Renderer>().enabled = true;
		this.GetComponent<Renderer>().material.mainTexture = 
            StatusIcon.manager.GetIcon (iconName);
	}

	/// <summary>
	/// Disables the current icon
	/// </summary>
	void ClearIcon ()
	{
		this.GetComponent<Renderer>().enabled = false;
	}
}
