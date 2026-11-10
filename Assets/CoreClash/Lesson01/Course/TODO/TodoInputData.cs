using Fusion;
using UnityEngine;

namespace CoreClash.Lesson01.Course
{
    // 클라이언트 입력을 네트워크 틱에 실어 보내는 데이터입니다.
    public struct TodoInputData : INetworkInput
    {
        public Vector2 Move;
    }
}
