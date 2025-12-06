using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public static GameUI I { get; private set; }

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }
    [Header("Panels")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject clearPanel;
    bool isPaused = false;
    bool isGameOver = false;
    bool isCleared = false;   
    bool _clickLock = false;

    // 다른 스크립트에서 참고할 일시정지 상태
    public static bool Paused { get; private set; }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (clearPanel    != null) clearPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Paused = false;
        isCleared = false;
    }

    void Update()
    {
        if (isGameOver || isCleared) return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    // ▶ 모든 “다시 시작”은 이 메서드만 호출하게
    void RetryCore()
{
    if (_clickLock) return;
    _clickLock = true;

    Time.timeScale = 1f;
    Paused = false;
    var scene = SceneManager.GetActiveScene();
    SceneManager.LoadScene(scene.name);
}

    // ========================
    //   Pause 관련
    // ========================
    public void TogglePause()
    {
        // 게임오버 상태에서는 Pause 토글 안 함
        if (isGameOver) return;

        isPaused = !isPaused;
        Paused = isPaused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void OnClickResume()
    {
        TogglePause();
        Debug.Log("Resume 버튼 눌림");
    }

    public void OnClickGoToMenu()
    {
        Debug.Log("Pause → 메인메뉴 버튼 눌림");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Paused = false;

        SceneManager.LoadScene("MainMenu");
    }

    // ========================
    //   Game Over 관련
    // ========================
    public void ShowGameOver()
    {
        isGameOver = true;
        isPaused = false;
        Paused = true;   // 게임오버 때도 입력 막기

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClickRetry()
    {
        RetryCore();
    }

    public void OnClickGameOverGoToMenu()
    {
        Debug.Log("GameOver → 메인메뉴");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Paused = false;

        SceneManager.LoadScene("MainMenu");
    }

    // ========================
    //   Game Clear 관련
    // ========================

    public void ShowClear(string nextSceneName = "")
    {
        if (isCleared || isGameOver) return;
        isCleared = true;
        isPaused  = false;
        Paused    = true;          // 입력/움직임 정지

        if (clearPanel != null) clearPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClickClearRetry()
    {
        RetryCore();
    }

    public void OnClickClearMenu()
    {
        OnClickGameOverGoToMenu();
    }

}
