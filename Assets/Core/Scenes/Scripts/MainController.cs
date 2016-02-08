using UnityEngine;
using System.Collections;
using TreeSharpPlus;

public class MainController : MonoBehaviour {

    GameObject[] spots;

    // Use this for initialization
    void Start () {
        Debug.Log("Initializing Main Controller ... ");
	    spots = GameObject.FindGameObjectsWithTag("Spot");
        if (spots.Length > 0) Debug.Log("Found "  + spots.Length + " spots for wandering ...");
    }
	
	// Update is called once per frame
	void Update () {
        GameObject[] testAgents = GameObject.FindGameObjectsWithTag("Test Agent");
        for(int i = 0; i < testAgents.Length; i++) {
            SmartObject so = testAgents[i].GetComponent<SmartObject>();
            Vector3 v = Vector3.zero;
            if (!(so.Behavior.Status == BehaviorStatus.InEvent)) {
                bool found = false;
                while (!found) {
                    int index = UnityEngine.Random.Range(0, spots.Length);
                    Transform t = spots[index].transform;
                    if (Vector3.Distance(t.position, so.transform.position) > 7f) {
                        v = t.position;
                        found = true;
                    }
                }
                v.y = 0;
                Val<Vector3> v1 = Val.V(() => v);
                Val<float> v2 = Val.V(() => 0.5f);
                SmartCharacter[] chars = { so.GetComponent<SmartCharacter>() };
                BehaviorEvent e = new BehaviorEvent(doEvent => chars[0].Node_GoToUpToRadius(v1, v2), chars);
                e.StartEvent(1.0f);
            }
        }
    }
}
