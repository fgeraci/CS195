using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class DataIO
{
    private static void FileWrite(Action write)
    {
        try
        {
            write();
        }
        catch (FileLoadException e)
        {
            Debug.LogError("Error writing to file: " + e);
        }
        catch (DirectoryNotFoundException e)
        {
            Debug.LogError("Error writing to file: " + e);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError("Error writing to file: " + e);
        }
    }

    private static T FileRead<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch (DirectoryNotFoundException e)
        {
            Debug.LogError("Error reading from file: " + e);
            return default(T);
        }
        catch (FileLoadException e)
        {
            Debug.LogError("Error reading from file: " + e);
            return default(T);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError("Error reading from file: " + e);
            return default(T);
        }
    }

    public static void StoreGraphXML(
        string path,
        ExplorationSpace space)
    {
        FileWrite(() =>
            DataSerializer.WriteToXmlFile<SpaceData>(
                path,
                new SpaceData(space)));
    }

    public static void StoreGraphBinary(
        string path,
        ExplorationSpace space)
    {
        FileWrite(() =>
            DataSerializer.WriteToBinaryFile<SpaceData>(
                path,
                new SpaceData(space)));
    }

    public static ExplorationSpace LoadGraphXML(
        string path,
        IEventLibrary library)
    {
        return GraphDecoder.Decode(
            FileRead<SpaceData>(() =>
                DataSerializer.ReadFromXmlFile<SpaceData>(path)),
            library);
    }

    public static ExplorationSpace LoadGraphBinary(
        string path,
        IEventLibrary library)
    {
        return GraphDecoder.Decode(
            FileRead<SpaceData>(() =>
                DataSerializer.ReadFromBinaryFile<SpaceData>(path)),
            library);
    }

#if !EXTERNAL
    public static void StoreWorldXML(
        string path,
        ObjectManager manager,
        EventLibrary library)
    {
        FileWrite(() =>
            DataSerializer.WriteToXmlFile<WorldData>(
                path,
                WorldEncoder.EncodeWorldData(manager, library)));
    }

    public static void StoreWorldBinary(
        string path,
        ObjectManager manager,
        EventLibrary library)
    {
        FileWrite(() =>
            DataSerializer.WriteToBinaryFile<WorldData>(
                path,
                WorldEncoder.EncodeWorldData(manager, library)));
    }
#endif

    public static bool LoadWorldXML(
        string path,
        out WorldState worldState,
        out EventDescriptor[] evtDescs)
    {
        WorldData data =
            FileRead(() =>
                DataSerializer.ReadFromXmlFile<WorldData>(path));

        if (data != null)
        {
            worldState = data.InitialState.Decode();
            evtDescs = data.Events.Convert(ed => ed.Decode()).ToArray();
            return true;
        }
        worldState = null;
        evtDescs = null;
        return false;
    }

    public static bool LoadWorldBinary(
        string path,
        out WorldState worldState,
        out EventDescriptor[] evtDescs)
    {
        WorldData data =
            FileRead(() =>
                DataSerializer.ReadFromBinaryFile<WorldData>(path));

        if (data != null)
        {
            worldState = data.InitialState.Decode();
            evtDescs = data.Events.Convert(ed => ed.Decode()).ToArray();
            return true;
        }
        worldState = null;
        evtDescs = null;
        return false;
    }
}
