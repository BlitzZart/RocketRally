using System.Collections;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    public LayerMask planetOnlyMask;

    public Planet currentPlanet;

    private Rigidbody m_rigidBody;
    private Transform m_planetTransform;
    private Camera m_head;
    private Gun m_gun;

    [SerializeField]
    private Vector3 m_upVector;

    [SerializeField]
    private Transform m_body;
    private Feet m_feet;

    private bool m_isInPlanetTransition = false;
    private Vector3 m_planetTransitionHit;

    // Movement
    private float m_walkSpeed = 7;
    private float m_runSpeed = 13;

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

        m_planetTransform = currentPlanet.transform;
    }
    private void Update()
    {
        UpdateShooting();
        UpdatePlanedSelection();
    }
    private void FixedUpdate()
    {
        UpdateGravity();
        UpdateHead();
        UpdateMovement();
        UpdateJump();
    }

    private void UpdateShooting()
    {
        if (Input.GetAxis("Fire1") > 0)
        {
            m_gun.Fire();
        }
    }
    private void UpdateGravity()
    {
        m_upVector = (transform.position - m_planetTransform.position).normalized;
        m_rigidBody.AddForce(m_upVector * currentPlanet.gravity);
    }
    private void UpdatePlanedSelection()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            RaycastHit hitInfo;
            Ray ray = new Ray(m_head.transform.position, m_head.transform.forward);
            Physics.Raycast(ray, out hitInfo, 1000, planetOnlyMask);

            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.green, 0.5f);

            if (hitInfo.transform != null && hitInfo.transform.root != currentPlanet.transform.root)
            {
                print("Switching to " + hitInfo.transform.root.name);
                currentPlanet = hitInfo.transform.root.GetComponentInChildren<Planet>();
                m_planetTransform = currentPlanet.transform.root;

                m_planetTransitionHit = hitInfo.point;
                StartCoroutine(PlanetTransition());
            }

        }
    }

    #region fixed updated functions
    private void UpdateMovement()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float yaw = Input.GetAxis("Mouse X");

        float speed = m_walkSpeed;
        if (Input.GetAxis("Run") > 0) { speed = m_runSpeed; }

        // MOVE
        transform.Translate(moveDirection * speed * Time.fixedDeltaTime);

        // ROTATE
        transform.Rotate(0, yaw, 0);
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, m_upVector) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 100.0f * Time.deltaTime);
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
    private void UpdateHead()
    {
        if (m_isInPlanetTransition)
        {
            return;
        }

        float headPitch = -Input.GetAxis("Mouse Y");
        m_head.transform.Rotate(headPitch, 0, 0);

        m_head.transform.localRotation = Quaternion.Slerp(m_head.transform.localRotation, Quaternion.Euler(m_head.transform.localRotation.eulerAngles.x, 0, 0), 0.01f * Time.fixedTime);
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

        float begin = Time.time;
        while (begin - Time.time > -0.05)
        {
            m_head.transform.LookAt(m_planetTransitionHit); 
            yield return new WaitForSeconds(0.01f);
        }
        m_isInPlanetTransition = false;

        yield return 0;
    }
}
