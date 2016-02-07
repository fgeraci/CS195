using UnityEngine;
using System.Collections;

/// <summary>
/// Class that is used to generate Smart Crowds, manages assigning portraits to them.
/// </summary>
public class CrowdGenerator : MonoBehaviour 
{
    /// <summary>
    /// Self-genreating singleton instance. If a CrowdGenerator exists in the scene, that is 
    /// selected as the instance.
    /// </summary>
    public static CrowdGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                return instance = new GameObject("CrowdGenerator").AddComponent<CrowdGenerator>();
            }
            return instance;
        }
    }

    private static CrowdGenerator instance;

    /// <summary>
    /// The parent for the created crowds.
    /// </summary>
    public GameObject Parent;

    /// <summary>
    /// The name prefix for the created crowds.
    /// </summary>
    public string NamePrefix = "Crowd";

    /// <summary>
    /// The portraits for the created crowds.
    /// </summary>
    public Texture2D[] CrowdGUIEventPortraits;

    private int currentCrowdIndex = 0;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Generates a crowd with the given arguments.
    /// </summary>
    /// <param name="criteria">The crowd's criteria</param>
    /// <param name="isStatic">Are the crowd's members static?</param>
    /// <param name="collection">The crowd's event scheduler.</param>
    public SmartCrowd GenerateCrowd(ISmartCrowdCriteria criteria, bool isStatic, EventCollection collection)
    {
        SmartCrowd instance = new GameObject(NamePrefix + currentCrowdIndex).AddComponent<SmartCrowd>();
        instance.tag = "Smart Object";
        if (Parent != null)
        {
            instance.transform.parent = Parent.transform;
        }
        instance.Initialize(criteria, isStatic, collection);
        instance.Portrait = GetPortrait(currentCrowdIndex);
        currentCrowdIndex++;
        return instance;
    }

    /// <summary>
    /// Gets the portrait for the crowd with the given index.
    /// </summary>
    private Texture2D GetPortrait(int index)
    {
        if (CrowdGUIEventPortraits == null || CrowdGUIEventPortraits.Length == 0)
        {
            return null;
        }
        return CrowdGUIEventPortraits[Mathf.Min(index, CrowdGUIEventPortraits.Length - 1)];
    }
}
