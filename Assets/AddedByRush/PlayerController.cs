using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed;	
	//public Text countText;
	//public Text winText;

	private Rigidbody rb;
	//public int count;
	

	public bool playerJump;
	public float jumpHeight;

	//[SerializeField] public Button ResetButton; // assign in the editor
	//[SerializeField] public Button StartButton;

	//void setCountText()
	//{
	//	countText.text = "Score: " + count.ToString ();
	//	if (count >= 15)
	//		winText.text = "You win!";
	//}

	void Start()
	{
		rb = GetComponent<Rigidbody> ();
		rb.gameObject.SetActive (false);
	//	count = 0;
		playerJump = false;
	//	setCountText ();

	//	StartButton.onClick.AddListener (() => {
			StartGame ();
	//}); 
	//	ResetButton.onClick.AddListener(() => { ResetGame(); });

	//	winText.text = "";
	}
	void FixedUpdate ()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical")	;

		Vector3 movement;

		movement = new Vector3 (moveHorizontal, 0, moveVertical);
		//Debug.Log ("Movement is:" + moveHorizontal + moveVertical);
		rb.AddForce (movement * speed);
	}
	//void OnCollisionEnter(Collision col){
		
	//	if (col.gameObject.name == "Ground" || col.gameObject.name == "High Ground") {
	//		if(this.gameObject.name=="Player 1")
	//			playerJump=false;
	//	}
	//	if (col.gameObject.name == "West Wall" || col.gameObject.name == "East Wall" || col.gameObject.name == "North Wall" || col.gameObject.name == "South Wall") {
	//		if(this.gameObject.name=="Player 1"){
	//			count--;
	//			setCountText();
	//		}
	//	}
	//	if (col.gameObject.name == "Player 2" && this.gameObject.name=="Player 1" &&
	//	    col.gameObject.transform.position.y>this.gameObject.transform.position.y) {
	//		count--;
	//		setCountText();
	//	}
	//}

	//void onCollisionEnter(Collision col){
	//	count--;
	//	setCountText();
	//	if (col.transform.name == "Ground") {
	//		playerJump = false;
	//	}
		//if(collision.collider.gameObject.CompareTag("Wall")){
		//	count--;
		//	setCountText();
		//}
	//}
	void Update(){

		if(Input.GetButtonDown("Jump")&&playerJump==false){
			rb.AddForce(Vector3.up*jumpHeight);
			playerJump=true;
		}

	}
	//void OnTriggerEnter (Collider other) 
	//{
	//	if (other.gameObject.CompareTag ("PickUp")) 
	//	{
	//		count = count + 1;
	//		setCountText();
	//		other.gameObject.SetActive (false);
	//	}
	//}
	
	//public void ResetGame(){
	//	Application.LoadLevel(0);
	//}
	public void StartGame(){
		rb.gameObject.SetActive (true);
	}
}
