using Fusion;
using UnityEngine;

namespace CoreClash.Lesson02
{
    public enum Lesson02Button
    {
        // TODO 1: 공격 버튼을 정의하세요.
        Attack
    }

    public struct Lesson02InputData : INetworkInput
    {
        // TODO 2: 이동 값과 NetworkButtons를 정의하세요.
        public Vector2 Move;
        public NetworkButtons Buttons; // 버튼을 누른 순간과 떼는 순간을 네트워크 틱 기준으로 판별 가능
    }
}