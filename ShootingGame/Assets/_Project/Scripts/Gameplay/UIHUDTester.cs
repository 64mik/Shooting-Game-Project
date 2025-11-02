using UnityEngine;

public class UIHUDTester : MonoBehaviour
{
    void Start()
    {
        // 시작할 때 한 번 테스트
        UIHUD.I.SetScore(123);
        UIHUD.I.SetTime(75);
        UIHUD.I.SetAccuracy(88.6f);
        UIHUD.I.CrosshairKick();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) UIHUD.I.SetScore(Random.Range(0, 999));
        if (Input.GetKeyDown(KeyCode.Alpha2)) UIHUD.I.SetTime(Random.Range(0, 300));
        if (Input.GetKeyDown(KeyCode.Alpha3)) UIHUD.I.SetAccuracy(Random.Range(0f, 100f));
        if (Input.GetKeyDown(KeyCode.Space))  UIHUD.I.CrosshairKick();  // 스페이스로 ‘뿅!’
    }
}
