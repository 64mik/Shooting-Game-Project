using UnityEngine;
using UnityEngine.InputSystem; // 새 입력 시스템에서 Keyboard.current 사용

[RequireComponent(typeof(CharacterController))]
public class FpsMove : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform cam; // MainCam

    [Header("Move")]
    [SerializeField] float walkSpeed = 4.0f;
    [SerializeField] float sprintSpeed = 6.5f;
    [SerializeField] float acceleration = 12f; // 가속/감속
    [SerializeField] float airControl = 0.4f;  // 공중 조작 비율

    [Header("Jump/Gravity")]
    [SerializeField] float jumpHeight = 1.0f;  // 스페이스 점프 높이
    [SerializeField] float gravity = -20f;     // 중력
    [SerializeField] float coyoteTime = 0.1f;  // 가장자리 여유 시간

    CharacterController cc;
    Vector3 velocity;          // 수직 속도 포함
    Vector3 planarVel;         // 수평 속도(카메라 기준)
    float lastGroundedTime;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cam == null) cam = Camera.main.transform;
    }

    void Update()
    {
        bool grounded = cc.isGrounded;
        if (grounded) lastGroundedTime = Time.time;

        // --- 입력 읽기 (WASD/Shift/Space) ---
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            input.x = (kb.dKey.isPressed ? 1 : 0) + (kb.aKey.isPressed ? -1 : 0);
            input.y = (kb.wKey.isPressed ? 1 : 0) + (kb.sKey.isPressed ? -1 : 0);
        }
        input = input.normalized;

        bool sprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        // --- 목표 수평 속도(카메라 진행방향 기준) ---
        Vector3 forward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 right   = new Vector3(cam.right.x,   0f, cam.right.z).normalized;

        float targetSpeed = (sprint ? sprintSpeed : walkSpeed);
        Vector3 targetPlanar = (forward * input.y + right * input.x) * targetSpeed;

        // 지상/공중 가속도 다르게 적용
        float accel = grounded ? acceleration : acceleration * airControl;
        planarVel = Vector3.MoveTowards(planarVel, targetPlanar, accel * Time.deltaTime);

        // --- 점프/중력 ---
        if ((grounded || Time.time - lastGroundedTime <= coyoteTime) && jumpPressed)
        {
            // 점프 높이에 맞는 초기 위속도 계산
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        // 중력 누적
        velocity.y += gravity * Time.deltaTime;

        // --- 이동 적용 ---
        Vector3 motion = planarVel * Time.deltaTime;
        motion.y = 0f;
        // CharacterController는 수직이 velocity.y에 의해 따로 내려감
        motion += Vector3.up * velocity.y * Time.deltaTime;

        CollisionFlags flags = cc.Move(motion);

        // 바닥 닿으면 수직속도 리셋
        if ((flags & CollisionFlags.Below) != 0 && velocity.y < 0f)
            velocity.y = -2f; // 살짝 눌러 붙게
    }
}

