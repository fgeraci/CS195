using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Ball_Script : MonoBehaviour
{
    private GameObject ball;
    private Rigidbody rb;
    public float speed =5;
    public bool isPickedUp;
    
    // Use this for initialization
    void Start()
    {
        ball = this.GetComponent<GameObject>();
        isPickedUp = false;        
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPickedUp == true)
        {
            return;
        }
        isPickedUp = false;
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movement * speed);

    }
}
