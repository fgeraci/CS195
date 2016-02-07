using UnityEngine;
using System.Collections;

public enum PropType
{
    Ball,
    Wallet,
    Drink,
    Gun,
    Briefcase,
    Form,
}

public class Prop : FadingObject 
{
    public PropType PropType;

    public GameObject[] EffectPrefabs;

    void Update()
    {
        this.UpdateFade();
    }

    public void CreateEffect(int index)
    {
        GameObject go = 
            (GameObject)GameObject.Instantiate(this.EffectPrefabs[index]);
        go.transform.parent = this.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}