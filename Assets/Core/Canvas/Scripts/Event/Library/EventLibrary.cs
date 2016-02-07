using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventLibrary : MonoBehaviour, IEventLibrary
{
    protected virtual EventSignature CreateSignature(ConstructorInfo ctorInfo)
    {
        return new EventSignature(ctorInfo);
    }

    public int LibraryIndex = 0;

    private HashSet<Type> eventTypes;
    private List<EventSignature> signatures;
    private Dictionary<Type, List<EventSignature>> signatureLookup;
    private Dictionary<string, EventSignature> signatureNames;

    protected static EventLibrary instance = null;
    public static EventLibrary Instance
    {
        get { return instance; }
    }

    public IList<EventSignature> GetSignatures()
    {
        return this.signatures.AsReadOnly();
    }

    public IEnumerable<EventSignature> GetSignaturesOfType(Type type)
    {
        List<EventSignature> list;
        if (this.signatureLookup.TryGetValue(type, out list) == false)
            yield break;

        foreach (EventSignature sig in list)
            yield return sig;
        yield break;
    }

    public EventSignature GetSignature(string name)
    {
        EventSignature signature;
        if (this.signatureNames.TryGetValue(name, out signature) == false)
            return null;
        return signature;
    }

    EventDescriptor IEventLibrary.GetDescriptor(string name)
    {
        return this.GetSignature(name);
    }

    void Awake()
    {
        this.Initialize();
        EventLibrary.instance = this;
    }

    protected void Initialize()
    {
        this.eventTypes = new HashSet<Type>();
        this.signatures = new List<EventSignature>();
        this.signatureLookup = new Dictionary<Type, List<EventSignature>>();
        this.signatureNames = new Dictionary<string, EventSignature>();
        this.FindEvents(this.LibraryIndex);
        this.PopulateSignatures();
    }

    private void FindEvents(int library)
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type type in types)
            if (type.IsSubclassOf(typeof(SmartEvent)))
                if (CheckIndex(type) == true)
                    this.eventTypes.Add(type);
    }

    private bool CheckIndex(Type t)
    {
        object[] attrs =
            t.GetCustomAttributes(typeof(LibraryIndexAttribute), false);

        if (attrs.Length > 0)
        {
            int[] indexes = ((LibraryIndexAttribute)attrs[0]).Indexes;
            return indexes.Contains(this.LibraryIndex);
        }
        return false;
    }

    private void PopulateSignatures()
    {
        foreach (Type type in this.eventTypes)
        {
            foreach (ConstructorInfo ctorInfo in type.GetConstructors())
            {
                try
                {
                    this.AddSignature(type, ctorInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        "Skipping constructor: "
                        + e.Message + "\n" + e.StackTrace);
                }
            }
        }
    }

    private void AddSignature(Type type, ConstructorInfo ctorInfo)
    {
       //s Debug.Log("Adding: " + ctorInfo.DeclaringType);
        EventSignature sig = this.CreateSignature(ctorInfo);
        this.signatures.Add(sig);
        if (this.signatureLookup.ContainsKey(type) == false)
            this.signatureLookup[type] = new List<EventSignature>();
        this.signatureLookup[type].Add(sig);
        if (sig.Name != null)
            this.signatureNames.Add(sig.Name, sig);
    }
}