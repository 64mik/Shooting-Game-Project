using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // 게임 시작 버튼
    public void OnClickStart()
    {
        SceneManager.LoadScene("Main"); // 실제 게임 씬 이름
    }

    // 게임 종료 버튼
    public void OnClickQuit()
    {
        Application.Quit();

        // 에디터에서 테스트할 때 종료되도록
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

