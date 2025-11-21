using UnityEngine;

// 총알/공격에 피격될 수 있는 대상이 구현하는 인터페이스.

public interface IHittable
{
    void TakeHit(float damage, Vector3 hitPoint, Vector3 hitNormal);
}