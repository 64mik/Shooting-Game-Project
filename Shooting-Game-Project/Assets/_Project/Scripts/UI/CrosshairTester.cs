using UnityEngine;
using UnityEngine.InputSystem; // 새 입력 시스템

public class CrosshairTester : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            UIHUD.I?.CrosshairKick();
    }
}

