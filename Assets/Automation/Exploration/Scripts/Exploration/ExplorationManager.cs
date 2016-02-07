using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ExplorationManager : MonoBehaviour
{
    public bool ReadXMLGraphFromFile = true;
    public bool ReadBinaryGraphFromFile = true;

    public bool SaveXMLWorldToFile = true;
    public bool SaveXMLGraphToFile = true;
    public bool SaveBinaryGraphToFile = true;

    public bool RelativePath = true;
    public string XMLWorldFilePath = "/Data/encodedWorld.dat";
    public string XMLGraphFilePath = "/Data/encodedGraph.dat";
    public string BinaryGraphFilePath = "/Data/encodedGraph.bindat";

    private ExplorationSpace space;
    private IEnumerator explorationIter;

    public bool IsDone
    {
        get 
        {
            return this.space != null;
        }
    }

    public ExplorationSpace StateSpace
    {
        get
        {
            return this.space;
        }
    }

    void Start()
    {
        this.space = null;
        this.explorationIter = null;
    }

    private string XMLWorldDataPath()
    {
        if (this.RelativePath == true)
            return Application.dataPath + this.XMLWorldFilePath;
        return this.XMLWorldFilePath;
    }

    private string XMLGraphDataPath()
    {
        if (this.RelativePath == true)
            return Application.dataPath + this.XMLGraphFilePath;
        return this.XMLGraphFilePath;
    }

    private string BinaryGraphDataPath()
    {
        if (this.RelativePath == true)
            return Application.dataPath + this.BinaryGraphFilePath;
        return this.BinaryGraphFilePath;
    }

    /// <summary>
    /// Compares the contents of two arrays for equality
    /// </summary>
    private static bool CompareArray<T>(T[] array1, T[] array2)
    {
        if (array1.Length != array2.Length)
            return false;

        for (int i = 0; i < array1.Length; i++)
            if (array1[i].Equals(array2[i]) == false)
                return false;
        return true;
    }

    private ExplorationSpace LoadGraphFromFile()
    {
        if (this.ReadXMLGraphFromFile == true)
        {
            Debug.Log(
                "Loading XML graph... " 
                + this.XMLGraphDataPath());

            ExplorationSpace loadedSpace =
                DataIO.LoadGraphXML(
                    this.XMLGraphDataPath(),
                    EventLibrary.Instance);

            if (loadedSpace == null)
            {
                Debug.Log(
                    "File missing or out of date: "
                    + this.XMLGraphDataPath());
                return null;
            }
            else
            {
                return loadedSpace;
            }
        }
        else if (this.ReadBinaryGraphFromFile == true)
        {
            Debug.Log(
                "Loading Binary graph... " 
                + this.BinaryGraphDataPath());

            ExplorationSpace loadedSpace =
                DataIO.LoadGraphBinary(
                    this.BinaryGraphDataPath(),
                    EventLibrary.Instance);

            if (loadedSpace == null)
            {
                Debug.Log(
                    "File missing or out of date: "
                    + this.BinaryGraphDataPath());
                return null;
            }
            else
            {
                return loadedSpace;
            }
        }

        return null;
    }

    void Update()
    {
        // Are we done?
        if (this.IsDone == false)
        {
            // Do we want to store the world?
            if (this.SaveXMLWorldToFile == true)
            {
                DataIO.StoreWorldXML(
                    this.XMLWorldDataPath(), 
                    ObjectManager.Instance, 
                    EventLibrary.Instance);
                Debug.Log("Saved world to " + this.XMLWorldDataPath());
                this.SaveXMLWorldToFile = false;
            }

            // Load a world, if we're interested
            ExplorationSpace loaded = this.LoadGraphFromFile();
            if (loaded != null)
            {
                this.space = loaded;
                Debug.Log(this.StateSpace.Nodes.Count());
                Debug.Log(this.StateSpace);
            }

            if (this.SaveXMLGraphToFile == true)
            {
                DataIO.StoreGraphXML(
                    this.XMLGraphDataPath(),
                    this.space);
                Debug.Log(
                    "Saved to XML file: " + this.XMLGraphDataPath());
            }

            if (this.SaveBinaryGraphToFile == true)
            {
                DataIO.StoreGraphBinary(
                    this.BinaryGraphDataPath(),
                    this.space);
                Debug.Log(
                    "Saved to Binary file: " + this.BinaryGraphDataPath());
            }
        }
    }
}
