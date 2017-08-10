using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour 
{
	public float speed = 1f;

	Camera cam;
	Rigidbody rb;

	void Start()
	{
		cam = Camera.main;
		rb = GetComponent<Rigidbody>();
	}

	void Update()
	{
		Vector3 facingPoint = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.y));	//according to the camera in scene, the z component of mouse point is specified
		transform.LookAt(facingPoint + Vector3.up * transform.position.y);
	}

	void FixedUpdate()
	{
		Vector3 velocity = new Vector3(-Input.GetAxis("Vertical"), 0f, Input.GetAxis("Horizontal")).normalized * speed * Time.deltaTime;
		rb.MovePosition(transform.position + velocity);
	}
}
