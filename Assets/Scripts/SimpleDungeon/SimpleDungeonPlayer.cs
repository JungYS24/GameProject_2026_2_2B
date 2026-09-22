using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleDungeonPlayer : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float moveSpeed = 5f;
    [Min(0f)] public float rotationSpeed = 12f;
    public float gravity = -20f;

    [Header("Reference")]
    [Tooltip("비어 있으면 Main Camera를 자동으로 사용합니다.")]
    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        Vector2 input = ReadMovementInput();
        Vector3 direction = GetCameraRelativeDirection(input);

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = direction * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private static Vector2 ReadMovementInput()
    {

        if (Keyboard.current == null) return Vector2.zero;

        float x = 0f;
        float y = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }


    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (cameraTransform == null)
            return new Vector3(input.x, 0f, input.y);

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }
}
