using UnityEngine;
using UnityEngine.InputSystem;

public class FpsLooks : MonoBehaviour
{
    [SerializeField] Transform playerRig;
    [SerializeField] float sensX = 0.6f, sensY = 0.5f;
    [SerializeField] float pitchMin = -85f, pitchMax = 85f;

    float yaw, pitch;

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 일시정지 상태면 카메라 회전 아예 막기
        if (GameUI.Paused) return;

        if (Mouse.current == null) return;

        Vector2 d = Mouse.current.delta.ReadValue();
        yaw += d.x * sensX;
        pitch -= d.y * sensY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // 좌우 회전은 플레이어 리그, 상하 회전은 카메라 자체
        playerRig.rotation = Quaternion.Euler(0f, yaw, 0f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
