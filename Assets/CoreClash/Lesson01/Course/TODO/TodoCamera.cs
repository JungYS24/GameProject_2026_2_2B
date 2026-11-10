using UnityEngine;

namespace CoreClash.Lesson01.Course
{
    // 카메라는 제공 코드입니다. 수업에서는 수정하지 않습니다.
    public class TodoCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -8f);
        [SerializeField] private float followSpeed = 7f;
        private Transform target;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 wanted = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position,
                wanted,
                1f - Mathf.Exp(-followSpeed * Time.deltaTime));
            transform.LookAt(target.position + Vector3.up * 0.5f);
        }
    }
}
