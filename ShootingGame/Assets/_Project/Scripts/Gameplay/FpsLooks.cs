using UnityEngine;
using UnityEngine.InputSystem;

public class FpsLooks : MonoBehaviour
{
    [SerializeField] Transform playerRig;
    [SerializeField] float sensX = 2.5f, sensY = 2.0f;
    [SerializeField] float pitchMin = -85f, pitchMax = 85f;
    float yaw, pitch;
    void OnEnable(){Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;}
    void Update() {
        if (Mouse.current == null) return;

        Vector2 d = Mouse.current.delta.ReadValue();
        yaw += d.x * sensX * Time.deltaTime * 60f;
        pitch -= d.y * sensY * Time.deltaTime * 60f;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        playerRig.rotation = Quaternion.Euler(0f, yaw, 0f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
