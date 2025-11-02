using UnityEngine;

// 총알/공격에 피격될 수 있는 대상이 구현하는 인터페이스.
// damage와 피격 위치를 함께 받는다.

public interface IHittable
{
    void TakeHit(float damage, Vector3 hitPoint, Vector3 hitNormal);
}