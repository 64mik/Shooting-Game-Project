using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject gameOverPanel;

    bool isPaused = false;
    bool isGameOver = false;

    // 다른 스크립트에서 참고할 일시정지 상태
    public static bool Paused { get; private set; }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Paused = false;
    }

    void Update()
    {
        if (isGameOver) return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
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
        Debug.Log("GameOver → 다시 시작");
        Time.timeScale = 1f;
        Paused = false;

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
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
}
