using System;
using System.Collections;
using UnityEngine;

public class FPS_Controller : MonoBehaviour
{
    // Planet transition
    public LayerMask planetOnlyMask;
    public Planet currentPlanet;
    private Transform m_planetTransform;
    private bool m_transitionTriggered = false;
    private bool m_isInPlanetTransition = false;
    private bool m_isInTransitionInitiationPhase = false;
    private Vector3 m_planetTransitionHit;

    // Player body
    [SerializeField] private Transform m_body;
    private Rigidbody m_rigidBody;
    public Camera Head { get => m_head; }
    private Camera m_head;
    private Feet m_feet;
    private Gun m_gun;
    private Vector3 m_headPosition;
    private Vector3 m_upVector;


    // Movement
    [SerializeField] float m_mouseSensitivity = 5;
    [SerializeField] private float m_walkSpeed = 7;
    [SerializeField] private float m_runSpeed = 13;

    // Jump
    private float m_jumpCooldown = 0.1f;
    private bool m_jumpCooldownOver = true;
    private float m_jumpForce = 10.0f;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        m_head = GetComponentInChildren<Camera>();
        m_feet = GetComponentInChildren<Feet>();
        m_rigidBody = GetComponent<Rigidbody>();
        m_gun = GetComponentInChildren<Gun>();

        m_headPosition = m_head.transform.localPosition;

        if (currentPlanet == null)
        {
            currentPlanet = GameObject.Find("Earth").GetComponent<Planet>();
        }

        m_planetTransform = currentPlanet.transform;
    }

    private void Update()
    {
        UpdateInput();
        UpdateHead();

        UpdateMovement();
    }

    private void FixedUpdate()
    {
        UpdatePlanetSelection();
        UpdateGravity();


        UpdateJump();
    }

    private void UpdateInput()
    {
        // shooting
        if (Input.GetAxis("Fire1") > 0)
        {
            m_gun.Fire();
        }

        // planet selection
        if (Input.GetButtonDown("Fire2"))
        {
            // decouppling input from fixedUpdate
            m_transitionTriggered = true;
        }
    }

    private void UpdateGravity()
    {
        m_upVector = (transform.position - m_planetTransform.position).normalized;

        float gravity = currentPlanet.gravity;

        if (m_isInPlanetTransition)
        {
            gravity *= 3.0f;
        }

        m_rigidBody.AddForce(m_upVector * gravity);
    }
    private void UpdatePlanetSelection()
    {
        if (m_transitionTriggered)
        {
            m_transitionTriggered = false;
            RaycastHit hitInfo;
            Ray ray = new Ray(m_head.transform.position, m_head.transform.forward);
            Physics.Raycast(ray, out hitInfo, 1000, planetOnlyMask);

            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.green, 0.5f);

            if (hitInfo.transform != null && hitInfo.transform.root != currentPlanet.transform.root)
            {
                m_head.transform.SetParent(null);

                //print("Switching to " + hitInfo.transform.root.name);
                currentPlanet = hitInfo.transform.root.GetComponentInChildren<Planet>();
                m_planetTransform = currentPlanet.transform.root;
                m_planetTransitionHit = hitInfo.point;

                StartCoroutine(PlanetTransition());
            }

        }
    }

    #region fixed updated functions

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

        if (m_isInPlanetTransition)
        {
            m_isInPlanetTransition = false;
        }

        m_jumpCooldownOver = false;

        StopCoroutine(JumpCoolDown());
        StartCoroutine(JumpCoolDown());

        m_rigidBody.AddForce(transform.up * m_jumpForce, ForceMode.Impulse);
    }
    private void UpdateHead()
    {
        if (m_isInTransitionInitiationPhase)
        {
            return;
        }

        float headPitch = -Input.GetAxis("Mouse Y") * m_mouseSensitivity/* * Time.deltaTime*/;
        m_head.transform.Rotate(headPitch, 0, 0);

        m_head.transform.localRotation = Quaternion.Slerp(m_head.transform.localRotation, Quaternion.Euler(m_head.transform.localRotation.eulerAngles.x, 0, 0), 0.01f * Time.fixedTime);
    }
    private void UpdateMovement()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float yaw = Input.GetAxis("Mouse X") * m_mouseSensitivity /** Time.deltaTime*/;

        float speed = m_walkSpeed;
        if (Input.GetAxis("Run") > 0) { speed = m_runSpeed; }

        // MOVE
        transform.Translate(moveDirection * speed * Time.deltaTime);

        // ROTATE
        transform.Rotate(0, yaw, 0);
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, m_upVector) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 100.0f * Time.deltaTime);
    }
    #endregion

    private IEnumerator JumpCoolDown()
    {
        yield return new WaitForSeconds(m_jumpCooldown);   
        m_jumpCooldownOver = true;
    }
    private IEnumerator PlanetTransition()
    {
        m_isInPlanetTransition = true;

        m_isInTransitionInitiationPhase = true;
        // wait for 3 ticks
        yield return 0;
        yield return 0;
        yield return 0;

        m_head.transform.SetParent(m_body);
        m_head.transform.localPosition = m_headPosition;

        m_isInTransitionInitiationPhase = false;
    }
}
