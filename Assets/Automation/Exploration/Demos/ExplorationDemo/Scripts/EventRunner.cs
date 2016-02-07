using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

public class EventRunner : MonoBehaviour 
{
    private static System.Random rnd;

    void Start()
    {
        rnd = new System.Random();
    }

	void Update () 
    {
        EventSignature sig = null;

        if (Input.GetKeyDown(KeyCode.G))
        {
            sig = EventLibrary.Instance.GetSignature("GrabAndGive");
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            sig = EventLibrary.Instance.GetSignature("SitTogether");
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            sig = EventLibrary.Instance.GetSignature("DistractAndSteal");
        }

        if (sig != null)
        {
            IEnumerable<IHasState> objs =
                ObjectManager.Instance.GetObjects().Cast<IHasState>();

            List<EventPopulation> applicable =
                new List<EventPopulation>(
                    EventPopulator.GetValidPopulations(
                        sig,
                        objs,
                        objs));

            if (applicable != null && applicable.Count > 0)
            {
                EventPopulation toRun =
                    applicable[rnd.Next(applicable.Count)];
                SmartEvent evt = sig.Create(toRun);
                evt.StartEvent(1.0f);
            }
        }
	}
}
