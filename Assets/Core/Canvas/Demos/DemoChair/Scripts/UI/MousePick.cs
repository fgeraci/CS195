using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MousePick : MonoBehaviour 
{
    public static MousePick instance;
    public SmartObject CurrentUser = null;

    public LayerMask layerMask;
    public LayerMask selectLayerMask;
    private List<ContextObject> receivers = null;

    public Vector3 mouseOffset = Vector3.zero;
    public AudioClip soundPick = null;
    public AudioClip soundActivate = null;
    public AudioClip soundDeny = null;
    public AudioClip soundFailed = null;

    void Awake()
    {
        this.receivers = new List<ContextObject>();
        instance = this;
    }

    public void AddReceiver(ContextObject co)
    {
        this.receivers.Add(co);
    }

	void LateUpdate() 
    {
        if (Input.GetMouseButtonDown(0) == true)
        {
            foreach (ContextObject co in this.receivers)
                if (co.HasMouse() == true)
                    return;

            foreach (ContextObject co in this.receivers)
                co.SendMessage("OnDeactivate");

            RaycastHit hit;
            Ray ray = this.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, this.layerMask) == true)
            {
                hit.collider.SendMessage(
                    "OnActivate", 
                    hit.point, 
                    SendMessageOptions.DontRequireReceiver);
            }
        }
        else if (Input.GetMouseButtonDown(1) == true)
        {
            foreach (ContextObject co in this.receivers)
                co.SendMessage("OnDeselect");

            this.Select(null);
            RaycastHit hit;
            Ray ray = this.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, this.selectLayerMask) == true)
            {
                hit.collider.SendMessage(
                    "OnSelect",
                    hit.point,
                    SendMessageOptions.DontRequireReceiver);
            }
        }
	}

    public void OnAffordance(ContextObject obj, string name)
    {
        if (obj.HasEntries == true)
        {
            AffordanceRunner runner = 
                this.CurrentUser.GetComponent<AffordanceRunner>();
            runner.RunAffordance(obj.ParentObject, name);
        }
    }

    public void Select(SmartObject newUser)
    {
        this.CurrentUser = newUser;
    }

    public void PlayPick()
    {
        this.GetComponent<AudioSource>().clip = this.soundPick;
        this.GetComponent<AudioSource>().Play();
    }

    public void PlayActivate()
    {
        this.GetComponent<AudioSource>().clip = this.soundActivate;
        this.GetComponent<AudioSource>().Play();
    }

    public void PlayDeny()
    {
        this.GetComponent<AudioSource>().clip = this.soundDeny;
        this.GetComponent<AudioSource>().Play();
    }

    public void PlayFailed()
    {
        this.GetComponent<AudioSource>().clip = this.soundFailed;
        this.GetComponent<AudioSource>().Play();
    }
}
