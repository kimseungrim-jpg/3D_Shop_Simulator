using UnityEngine;

/// <summary>
/// 플레이어의 키보드 이동 입력과 마우스 시점 회전을 처리
/// UI 상태에 따라 이동 및 시점 조작 기능 여부를 제어
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 200f;
    public bool canMove = true;

    float xRotation = 0f;

    public Transform cameraTransform;

    CharacterController controller;

    /// <summary>
    /// 게임 조작을 위해 커서를 잠금
    /// </summary>
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 상점이나 결과 UI가 열린 동안에는 이동 입력과 카메라 회전을 차단
        if (!canMove)
            return;

        Move();
        Look();
    }

    /// <summary>
    /// 수평, 수직 입력을 플레이어의 오른쪽,앞쪽 방향으로 변환하여 이동
    /// </summary>
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 월드 축이 아닌 플레이어가 바라보는 방향을 기준으로 전후좌우 이동 방향을 계산
        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);
    }

    /// <summary>
    /// 마우스 이동량을 이용해 카메라의 상하 시점과 플레이어의 좌우 방향을 회전
    /// </summary>
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity* Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //카메라가 뒤집히거나 과하게 회전하는 것을 방지
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        //상하 회전의 경우 카메라에만 적용, 좌우 회전은 플레이어 전체에 적용
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
