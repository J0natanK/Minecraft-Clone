using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
	public float speed = 10f;
	public float liftSpeed = 5f;
	public float mouseSensitivity = 100f;

	private Rigidbody rb;
	private float xRotation = 0f;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.useGravity = false; // Disable gravity for flying
		Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
	}

	void Update()
	{
		HandleMovement();
		HandleRotation();
	}

	void HandleMovement()
	{
		// Get input for forward/backward movement
		float moveVertical = Input.GetAxis("Vertical");
		// Get input for sideways movement
		float moveHorizontal = Input.GetAxis("Horizontal");
		Vector3 move = transform.forward * moveVertical * speed * Time.deltaTime +
					   transform.right * moveHorizontal * speed * Time.deltaTime;
		rb.MovePosition(rb.position + move);

		// Get input for lift
		if (Input.GetKey(KeyCode.Space))
		{
			rb.velocity = new Vector3(rb.velocity.x, liftSpeed, rb.velocity.z);
		}
		else if (Input.GetKey(KeyCode.LeftControl))
		{
			rb.velocity = new Vector3(rb.velocity.x, -liftSpeed, rb.velocity.z);
		}
		else
		{
			rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
		}
	}

	void HandleRotation()
	{
		// Get mouse input for rotation
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		// Apply yaw rotation (left/right)
		rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, mouseX, 0f));

		// Apply pitch rotation (up/down)
		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		Vector3 targetRotation = new Vector3(xRotation, rb.rotation.eulerAngles.y, 0f);
		rb.MoveRotation(Quaternion.Euler(targetRotation));
	}
}
