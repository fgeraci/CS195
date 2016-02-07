using System.Collections.Generic;
using LitJson;
using System.Reflection;
using System;
using System.IO;
using UnityEngine;
using System.Linq;

/// <summary>
/// Options for role fill-in that can be changed in the GUI.
/// </summary>
public static class FillInOptionsSerializer
{
    /// <summary>
    /// All existing rules.
    /// </summary>
    public static List<IRoleFillerRule> AllRules = new List<IRoleFillerRule>();

    /// <summary>
    /// The actually active rules.
    /// </summary>
    public static List<IRoleFillerRule> ActiveRules = new List<IRoleFillerRule>();

    private static string PATH_TO_SERIALIZED_OPTIONS = 
        Path.Combine(Application.persistentDataPath, "FillInOptions.json");

    /// <summary>
    /// Mark the given rule either active or inactive, depending on the value
    /// of active.
    /// </summary>
    /// <param name="active">Should the rule be active or inactive?</param>
    public static void MarkActive(IRoleFillerRule rule, bool active)
    {
        if (active && !ActiveRules.Contains(rule))
        {
            ActiveRules.Add(rule);
        }
        else
        {
            ActiveRules.Remove(rule);
        }
    }

    /// <summary>
    /// Adds the option toggles to the given ActionBar.
    /// </summary>
    public static void AddToggles(ActionBar bar)
    {
        foreach (IRoleFillerRule rule in AllRules)
        {
            IRoleFillerRule local = rule;
            bar.AddToggle(local.Name, (bool b) => MarkActive(local, b), ActiveRules.Contains(local));
        }

    }

    /// <summary>
    /// Gets the action bar for selecting the different options.
    /// </summary>
    public static ActionBar ToActionBar(bool isHorizontal)
    {
        Load();
        ActionBar result = new ActionBar(isHorizontal);
        AddToggles(result);
        return result;
    }

    /// <summary>
    /// Write the options and whether they are active to a file so they can be retrieved later.
    /// </summary>
    public static void Save()
    {
        File.WriteAllText(PATH_TO_SERIALIZED_OPTIONS, JsonMapper.ToJson(new SerializedRules(ActiveRules)));
    }

    /// <summary>
    /// Load the options from file.
    /// </summary>
    private static void Load()
    {
        SerializedRules rules = new SerializedRules();
        if (File.Exists(PATH_TO_SERIALIZED_OPTIONS))
        {
            rules = JsonMapper.ToObject<SerializedRules>(File.ReadAllText(PATH_TO_SERIALIZED_OPTIONS));
        }
        rules.ToRules();
    }

    #region Wrapper Class
    /// <summary>
    /// Wrapper class for serialization of the rules.
    /// </summary>
    private class SerializedRules
    {
        public List<string> activeRules { get; set; }

        public SerializedRules() 
        {
            activeRules = new List<string>();
        }

        public SerializedRules(List<IRoleFillerRule> activeRules)
        {
            this.activeRules = new List<string>();
            foreach (IRoleFillerRule rule in activeRules)
            {
                this.activeRules.Add(rule.GetType().Name);
            }
        }

        /// <summary>
        /// Converts the stored data to a list of rules, indicates which are active. Stores results
        /// in AllRules and ActiveRules.
        /// </summary>
        public void ToRules()
        {
            AllRules = new List<IRoleFillerRule>();
            ActiveRules = new List<IRoleFillerRule>();
            List<Type> ruleTypes = ReflectionUtil.GetSubclassesOf(typeof(IRoleFillerRule));
            foreach (Type t in ruleTypes.Where((Type t) => !t.IsAbstract))
            {
                IRoleFillerRule rule = null;
                try 
                {
                    rule = (IRoleFillerRule)Activator.CreateInstance(t);
                }
                catch(Exception)
                {
                    Debug.LogError(String.Format("Type {0} does not have an empty constructor", t.Name));
                    continue;
                }
                AllRules.Add(rule);
                if (activeRules.Contains(t.Name))
                {
                    ActiveRules.Add(rule);
                }
            }
        }
    }
    #endregion
}