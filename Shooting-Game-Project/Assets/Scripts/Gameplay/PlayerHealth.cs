using UnityEngine;
using UnityEngine.Events;     

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] int maxHP = 100;
    [SerializeField] int currentHP;

    [Header("Death")]
    public UnityEvent onDie;  

    bool isDead;

    void Awake()
    {
        currentHP = maxHP;
        // (선택) HP 텍스트가 있다면 여기서 갱신
        // UIHUD.I?.SetScore(...) 같은 식으로 별도 HP UI가 있다면 반영
    }

    public void TakeDamage(int dmg)
    {
        Debug.Log($"[HP] TakeDamage {dmg}");
        if (isDead) return;
        currentHP = Mathf.Max(0, currentHP - dmg);

        if (currentHP <= 0)
        {
            Die();
        }
        Debug.Log($"[HP] currentHP={currentHP}");
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        // (선택) HP UI 갱신
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1) 입력/시야/사격 비활성화
        var look = GetComponentInChildren<FpsLooks>();
        if (look != null) look.enabled = false;

        var shoot = GetComponentInChildren<Gun>();
        if (shoot != null) shoot.enabled = false;

        // 2) 커서 풀기
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // 3) 게임오버 처리 (타임스케일 정지, 패널 켜기 등)
        // 이미 Pause/GameOver 매니저가 있다면 그쪽 API 호출
        Time.timeScale = 0f;
        // 예: GameOverPanel 켜기
        // gameOverPanel.SetActive(true);

        onDie?.Invoke(); // (선택) 인스펙터에서 추가 액션 연결

        Debug.Log("Player Died");
    }
}
