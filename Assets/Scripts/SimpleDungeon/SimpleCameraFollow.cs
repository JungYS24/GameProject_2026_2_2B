using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -8f);
    [Min(0f)] public float followSpeed = 8f;
    public bool lookAtTarget = true;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime);

        if (lookAtTarget)
            transform.LookAt(target.position + Vector3.up);
    }
}
