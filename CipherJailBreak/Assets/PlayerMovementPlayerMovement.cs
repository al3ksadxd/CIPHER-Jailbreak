using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -19.62f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 1. Provera da li je igrač na zemlji
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Mali minus da drži igrača pripijenog za pod
        }

        // Ako trenutno radi Dash, pauziramo obično kretanje
        if (isDashing) return;

        // 2. Unos za kretanje (WASD / Strelice)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = move.normalized; // Sprečava brže kretanje po dijagonali

        // 3. Provera da li igrač drži Shift za trčanje
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // 4. Jump (Space)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 5. Dash na 'E'
        if (Input.GetKeyDown(KeyCode.E) && canDash)
        {
            // Ako drži WASD ide u tom smeru, u suprotnom ide unapred
            Vector3 dashDirection = move.magnitude > 0.1f ? move : transform.forward;
            StartCoroutine(PerformDash(dashDirection));
        }

        // 6. Primena gravitacije
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private IEnumerator PerformDash(Vector3 dashDirection)
    {
        canDash = false;
        isDashing = true;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            // Izvršava brz pomak u izabranom smeru bez uticaja gravitacije
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;

        // Cooldown tajmer
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // Vizuelni prikaz GroundCheck sfere u Editoru
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}