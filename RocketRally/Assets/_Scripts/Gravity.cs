using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    private Rigidbody m_rigidBody;
    private Planet m_planet;
    private Transform m_planetTransform;
    private Camera m_head;
    [SerializeField]
    private Transform m_body;
    private Feet m_feet;

    // Movement
    private float m_walkSpeed = 7;
    private float m_runSpeed = 13;

    // Jump
    private float m_jumpCooldown = 0.1f;
    private bool m_jumpCooldownOver = true;
    private float m_jumpForce = 10.0f;

    private void Start()
    {
        m_head = GetComponentInChildren<Camera>();
        m_feet = GetComponentInChildren<Feet>();
        m_rigidBody = GetComponent<Rigidbody>();
        m_planet = FindObjectOfType<Planet>();

        m_planetTransform = m_planet.transform;
    }

    private void FixedUpdate()
    {
        UpdateGravity();
        UpdateHeadAndBody();
        UpdateMovement();
        UpdateJump();
        UpdateRotations();
    }

    private void UpdateGravity()
    {
        m_rigidBody.AddForce((transform.position - m_planetTransform.position).normalized * m_planet.gravity);
    }
    private void UpdateMovement()
    {
        Vector3 direction = Vector3.zero;

        direction = m_body.right * Input.GetAxis("Horizontal");
        direction += m_body.forward * Input.GetAxis("Vertical");

        Debug.DrawRay(transform.position, direction * 3, Color.green);

        float speed = m_walkSpeed;
        if (Input.GetAxis("Run") > 0)
        {
            speed = m_runSpeed;
        }

        transform.Translate(direction * Time.fixedDeltaTime * speed, Space.World);
        //m_rigidBody.AddForce(direction * 10);
    }

    private void UpdateJump()
    {
        if (Input.GetAxis("Jump") <= 0)
        {
            return;
        }

        if (!m_feet.OnGround || !m_jumpCooldownOver)
        {
            return;
        }

        m_jumpCooldownOver = false;

        StopCoroutine(JumpCoolDown());
        StartCoroutine(JumpCoolDown());

        m_rigidBody.AddForce(transform.up * m_jumpForce, ForceMode.Impulse);
    }

    private void UpdateHeadAndBody()
    {
        float yaw = Input.GetAxis("Mouse X");
        float headPitch = -Input.GetAxis("Mouse Y");

        m_head.transform.Rotate(headPitch, 0, 0);

        m_body.Rotate(0, yaw, 0);
    }

    private void UpdateRotations()
    {
        Vector3 up = (transform.position - m_planetTransform.position).normalized;

        transform.up = up;
    }

    private IEnumerator JumpCoolDown()
    {
        yield return new WaitForSeconds(m_jumpCooldown);   
        m_jumpCooldownOver = true;
    }
}
