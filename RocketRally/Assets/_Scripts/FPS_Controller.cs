using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FPS_Controller : MonoBehaviour
{
    private bool m_initialized = false;

    public float g;

    // Planet transition
    public LayerMask planetOnlyMask;
    public Planet currentPlanet;
    private Transform m_planetTransform;
    private bool m_transitionTriggered = false;
    private bool m_isInPlanetTransition = false;
    private bool m_isInTransitionInitiationPhase = false;
    private Vector3 m_lastPlanetsPosition;

    // Player body
    [SerializeField] private Transform m_body;
    public Rigidbody RigidBody { get => m_rigidBody; }
    private Rigidbody m_rigidBody;
    private float m_initDrag, m_initAngDrag;
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

    // Player health, death, respawn
    private Health m_health;
    public Health Health { get => m_health; }
    private bool m_isDead = false;
    public bool IsDead { get => m_isDead; }
    private bool m_driftingAfterDeath = false;
    private bool m_playerInCooldown = false;
    private float m_respawnCooldown = 10.0f;
    private float m_currentRespawnTime;
    public float RemainungRespawnTime { get => m_respawnCooldown - m_currentRespawnTime; }

    private void Start()
    {
        StartCoroutine(Initialize());
        m_health = GetComponent<Health>();
    }

    private void Update()
    {
        if (!m_initialized)
        {
            return;
        }

        if (m_playerInCooldown)
        {
            return;
        }

        UpdateInput();
        UpdateHead();
        UpdateMovement();
    }

    private void FixedUpdate()
    {
        if (!m_initialized)
        {
            return;
        }

        UpdatePlanetSelection();
        UpdateGravity();
        UpdateJump();
    }

    private void OnDestroy()
    {
        m_feet.HitGround -= OnHitGround;
    }

    public void KillPlayer()
    {
        m_isDead = true;

        m_rigidBody.constraints = RigidbodyConstraints.None;
        m_rigidBody.drag = 0;
        m_rigidBody.angularDrag = 0;

        m_rigidBody.AddTorque(UnityEngine.Random.insideUnitSphere * 10);
        m_rigidBody.AddForce(
            (transform.position - currentPlanet.transform.position).normalized * 20.0f + 
            UnityEngine.Random.insideUnitSphere, ForceMode.Impulse);

        currentPlanet = null;

        m_driftingAfterDeath = true;
        StartCoroutine(HandleDeadPlayerInactivity());
    }

    public void RevivePlayer()
    {
        m_isDead = false;

        m_rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        m_rigidBody.drag = m_initDrag;
        m_rigidBody.angularDrag = m_initAngDrag;

        NW_PlayerScript.Instance.RevivePlayerServerRpc();
    }

    private IEnumerator Initialize()
    {
        yield return new WaitUntil(() => NW_PlayerScript.Instance != null);
        yield return new WaitUntil(() => NW_PlayerScript.Instance.Initialized);

        NW_PlayerScript nwPlayer = GetComponent<NW_PlayerScript>();

        m_head = GetComponentInChildren<Camera>();

        if (nwPlayer.IsLocal)
        {
            Cursor.lockState = CursorLockMode.Locked;

            m_feet = GetComponentInChildren<Feet>();
            m_feet.HitGround += OnHitGround;
            m_rigidBody = GetComponent<Rigidbody>();
            m_initDrag = m_rigidBody.drag;
            m_initAngDrag = m_rigidBody.angularDrag;

            m_gun = GetComponentInChildren<Gun>();

            m_headPosition = m_head.transform.localPosition;

            if (currentPlanet == null)
            {
                currentPlanet = GameObject.Find("Earth").GetComponent<Planet>();
            }
            m_planetTransform = currentPlanet.transform;
            m_lastPlanetsPosition = m_planetTransform.position;

            nwPlayer.SetFpsController(this);

            m_initialized = true;
        }
        else
        {
            Camera cam = m_head.GetComponent<Camera>();
            AudioListener al = m_head.GetComponent<AudioListener>();
            Destroy(cam);
            Destroy(al);
        }
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
        if (IsDead)
        {
            // increase drag when player drifted 150m away
            if (m_rigidBody.drag == 0.0f && Vector3.Distance(m_lastPlanetsPosition, transform.position) > 200.0f)
            {
                m_rigidBody.drag = m_initDrag * 4.0f;
                m_rigidBody.angularDrag = m_initAngDrag * 4.0f;
            }
        }
        else
        {
            if (m_driftingAfterDeath && m_feet.OnGround)
            {
                m_driftingAfterDeath = false;
            }
        }

        m_upVector = (transform.position - m_planetTransform.position).normalized;

        float gravity = 0;
        if (currentPlanet != null)
        {
            gravity = currentPlanet.gravity;
        }


        if (m_isInPlanetTransition)
        {
            gravity *= 3.0f;

            if (m_driftingAfterDeath)
            {
                gravity = -100.0f;
            }
        }

        g = gravity;

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

            if (hitInfo.transform != null && hitInfo.transform.root != currentPlanet?.transform.root)
            {
                m_head.transform.SetParent(null);

                //print("Switching to " + hitInfo.transform.root.name);
                currentPlanet = hitInfo.transform.root.GetComponentInChildren<Planet>();
                m_planetTransform = currentPlanet.transform.root;
                m_lastPlanetsPosition = m_planetTransform.position;

                if (m_isDead)
                {
                    RevivePlayer();
                }

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

    private void OnHitGround()
    {
        if (m_isInPlanetTransition && m_feet.OnGround)
        {
            m_isInPlanetTransition = false;
        }
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

    private IEnumerator HandleDeadPlayerInactivity()
    {
        m_playerInCooldown = true;
        m_currentRespawnTime = 0;
        while (m_currentRespawnTime < m_respawnCooldown)
        {
            m_currentRespawnTime += Time.deltaTime;
            yield return 0;
        }
        m_playerInCooldown = false;
    }
}
