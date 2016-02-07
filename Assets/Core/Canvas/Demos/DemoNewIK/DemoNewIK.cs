using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RootMotion.FinalIK;

public class DemoNewIK: MonoBehaviour
{
    public InteractionObject Object1 = null;
    public InteractionObject Object2 = null;
    public InteractionObject Object3 = null;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            this.GetComponent<IKController>().StartInteraction(FullBodyBipedEffector.RightHand, this.Object1);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            this.GetComponent<IKController>().StartInteraction(FullBodyBipedEffector.RightHand, this.Object2);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            this.GetComponent<IKController>().StartInteraction(FullBodyBipedEffector.RightHand, this.Object3);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            this.GetComponent<IKController>().StopInteraction(FullBodyBipedEffector.RightHand);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            this.GetComponent<IKController>().StartInteraction(FullBodyBipedEffector.LeftHand, this.Object1);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            this.GetComponent<IKController>().StartInteraction(FullBodyBipedEffector.LeftHand, this.Object2);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            this.GetComponent<IKController>().StartInteraction(FullBodyBipedEffector.LeftHand, this.Object3);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            this.GetComponent<IKController>().StopInteraction(FullBodyBipedEffector.LeftHand);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            this.GetComponent<IKController>().LookAt(this.Object1.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            this.GetComponent<IKController>().LookAt(this.Object2.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            this.GetComponent<IKController>().LookAt(this.Object3.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            this.GetComponent<IKController>().LookStop();
        }
    }
}
