using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BallScript : MonoBehaviour {

    private Animator animator;
    private Rigidbody rb;
    public float speed;
    public bool isPickedUp;


	// Use this for initialization
	void Start () {
        
        isPickedUp = false;
        animator = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>();
	}

    // Update is called once per frame
    void Update()
    {
        if (isPickedUp == true)
        {
            return;
        }
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movement * speed);

    }
}
