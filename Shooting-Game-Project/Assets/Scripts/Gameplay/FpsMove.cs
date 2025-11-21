using UnityEngine;
using UnityEngine.InputSystem; // 새 입력 시스템: Keyboard.current 등 사용

// 이 스크립트가 붙은 오브젝트에 CharacterController가 꼭 있어야 함
[RequireComponent(typeof(CharacterController))]
public class FpsMove : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform cam; // 카메라(보통 Main Camera)

    [Header("Move")]
    [SerializeField] float walkSpeed = 4.0f;   // 걷기 속도
    [SerializeField] float sprintSpeed = 6.5f; // 달리기 속도(Shift)
    [SerializeField] float acceleration = 12f; // 가속/감속 속도
    [SerializeField] float airControl = 0.4f;  // 공중에서의 조작 비율 (1보다 작게)

    [Header("Gravity")]
    [SerializeField] float gravity = -20f;     // 중력 가속도 (음수)

    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;        // 최대 스태미너
    [SerializeField] float staminaDrainPerSec = 25f; // 초당 소모량
    [SerializeField] float staminaRegenPerSec = 15f; // 초당 회복량
    [SerializeField] float sprintMinStamina = 5f;    // 이 값 이하로 떨어지면 스프린트 불가

    float stamina;  // 현재 스태미너
    CharacterController cc; // 충돌·계단·경사 처리용 컨트롤러
    Vector3 velocity;       // 수직 속도만 사용(y) — 중력/낙하용
    Vector3 planarVel;      // 수평(XZ) 속도 — 카메라 기준 이동용

    void Awake()
    {
        // 같은 오브젝트에 붙은 CharacterController 한 번만 찾아서 캐싱
        cc = GetComponent<CharacterController>();

        // 인스펙터에서 cam을 안 넣어줬다면 MainCamera를 자동으로 참조
        if (cam == null) cam = Camera.main.transform;

         stamina = maxStamina; // 시작 시 풀게이지
    }

    void Update()
    {
        bool grounded = cc.isGrounded; // 현재 바닥에 붙어 있는지 여부

        // --- 1) 입력 읽기 (WASD) ---
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;

            // A/D → x축 (-1 ~ 1)
            input.x = (kb.dKey.isPressed ? 1 : 0) +
                      (kb.aKey.isPressed ? -1 : 0);

            // W/S → y축 (-1 ~ 1)
            input.y = (kb.wKey.isPressed ? 1 : 0) +
                      (kb.sKey.isPressed ? -1 : 0);
        }
        // 대각선 과속 방지: 입력 벡터의 길이를 1로 맞춤
        input = input.normalized;

       bool shiftDown = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        // 움직이고 있고, Shift 누르고 있고, 스태미너가 어느 정도 남았을 때만 스프린트 허용
        bool wantsSprint = shiftDown && input != Vector2.zero;
        bool canSprint   = stamina > sprintMinStamina;
        bool sprint      = wantsSprint && canSprint;


        // --- 2) 카메라 기준 목표 수평 속도 계산 ---
        // 카메라의 수평 전방/우측 방향만 뽑아오기 (y = 0으로 평면화)
        Vector3 forward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 right   = new Vector3(cam.right.x,   0f, cam.right.z).normalized;

        // 걷기/달리기 중에 어떤 속도를 사용할지 결정
        float targetSpeed = sprint ? sprintSpeed : walkSpeed;

        // 입력(x,y)을 카메라 기준 XZ 방향으로 변환해서 목표 속도 벡터 생성
        Vector3 targetPlanar = (forward * input.y + right * input.x) * targetSpeed;

        // ----3) 지면에 있을 때와 공중에 있을 때의 가속/감속량 설정
        float accel = grounded ? acceleration : acceleration * airControl;

        // 현재 수평 속도를 목표 속도 쪽으로 ‘초당 accel만큼’ 부드럽게 붙이기
        planarVel = Vector3.MoveTowards(planarVel, targetPlanar, accel * Time.deltaTime);

        // 중력 적용
        if (grounded && velocity.y < 0f)
            // 바닥에 있을 땐 너무 크게 떨어지지 않게 살짝 음수로 고정해서 바닥에 붙여줌
            velocity.y = -2f;
        else
            // 공중에 있을 땐 중력 가속도만큼 계속 아래로 속도 누적
            velocity.y += gravity * Time.deltaTime;

        // --- 스태미너 소모 / 회복 ---
            if (grounded && sprint)
            {
            // 달리는 중 → 스태미너 소모
            stamina -= staminaDrainPerSec * Time.deltaTime;
            }
            else
            {   
            // 달리지 않을 때, 또는 공중일 때 → 서서히 회복
            stamina += staminaRegenPerSec * Time.deltaTime;
            }

            // 0 ~ max 사이로 고정
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);

            UIHUD.I?.SetStamina(stamina, maxStamina);

        

        // --- 4) CharacterController.Move로 실제 이동 적용 ---
        Vector3 motion = planarVel * Time.deltaTime; // 수평 이동
        motion.y = velocity.y * Time.deltaTime;      // 수직 이동

        


        cc.Move(motion); // 충돌, 경사, 계단 처리까지 포함해서 이동
    }
}



