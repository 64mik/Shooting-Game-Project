using UnityEngine;
using UnityEngine.InputSystem; // WASD/Sprint 읽음

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

    [Header("Gravity")]
    [SerializeField] float gravity = -20f;     // 중력

    CharacterController cc;
    Vector3 velocity;   // 수직 속도만 사용
    Vector3 planarVel;  // 수평 속도(카메라 기준)

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cam == null) cam = Camera.main.transform;
    }

    void Update()
    {
        bool grounded = cc.isGrounded;

        // --- 입력 읽기 (WASD/Shift) ---
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            input.x = (kb.dKey.isPressed ? 1 : 0) + (kb.aKey.isPressed ? -1 : 0);
            input.y = (kb.wKey.isPressed ? 1 : 0) + (kb.sKey.isPressed ? -1 : 0);
        }
        input = input.normalized;

        bool sprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        // --- 목표 수평 속도(카메라 기준) ---
        Vector3 forward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 right   = new Vector3(cam.right.x,   0f, cam.right.z).normalized;

        float targetSpeed = sprint ? sprintSpeed : walkSpeed;
        Vector3 targetPlanar = (forward * input.y + right * input.x) * targetSpeed;

        float accel = grounded ? acceleration : acceleration * airControl;
        planarVel = Vector3.MoveTowards(planarVel, targetPlanar, accel * Time.deltaTime);

        // --- 중력만 적용 (점프 없음) ---
        if (grounded && velocity.y < 0f)
            velocity.y = -2f; // 바닥에 살짝 눌러 붙게
        else
            velocity.y += gravity * Time.deltaTime;

        // --- 이동 적용 ---
        Vector3 motion = planarVel * Time.deltaTime;
        motion.y = velocity.y * Time.deltaTime;

        cc.Move(motion);
    }
}


