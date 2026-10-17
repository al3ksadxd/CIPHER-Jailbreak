using UnityEngine;
using UnityEngine.InputSystem; 

public class MouseLook : MonoBehaviour
{
    [Header("Podešavanja osetljivosti")]
    public float mouseSensitivity = 10f;

    [Header("Reference")]
    public Transform playerBody;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * (mouseSensitivity * 0.05f);

        float mouseX = mouseDelta.x;
        float mouseY = mouseDelta.y;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
