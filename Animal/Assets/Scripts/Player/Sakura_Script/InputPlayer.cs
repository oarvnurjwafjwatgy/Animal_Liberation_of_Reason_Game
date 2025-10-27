using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]
public class InputPlayer : MonoBehaviour
{
    public float moveSpeed = 5.0f; // �L�����N�^�[�̈ړ����x
    public GameObject cameraObject;

    // �Q�Ƃ���R���|�[�l���g
    private Rigidbody rb;
    private Controller controller; // �쐬���� Controller �N���X

    // Start is called before the first frame update
    void Start()
    {
        cameraObject = transform.GetChild(0).gameObject;
        //cameraObject = GameObject.Find("Camera");
    }

    private void Awake()
    {
        // �����Q�[���I�u�W�F�N�g�ɃA�^�b�`����Ă���R���|�[�l���g���擾
        TryGetComponent(out rb);
        TryGetComponent(out controller);

        if (controller == null)
        {
            Debug.LogError("Controller �R���|�[�l���g��������܂���I");
        }
    }

    // �������Z�� FixedUpdate �ōs���܂�
    private void FixedUpdate()
    {
        // Controller �N���X���������擾�ł��Ă��邩�m�F
        if (controller != null && rb != null)
        {
            // 1. Controller �N���X����X�e�B�b�N�̓��͒l���擾
            Vector2 leftStickInput = controller.GetLeftStick();

            // 2. ���͒l (Vector2) �� 3D �̈ړ����� (Vector3) �ɕϊ�
            Vector3 moveDirection = new Vector3(leftStickInput.x, 0, leftStickInput.y);

            Vector3 cameraForward = Vector3.Scale(cameraObject.transform.forward, new Vector3(1,0,1)).normalized;
            Vector3 moveForward = cameraForward * leftStickInput.y + cameraObject.transform.right * leftStickInput.x;
            rb.velocity = moveForward * moveSpeed + new Vector3(0, rb.velocity.y, 0);

            // 3. Rigidbody �̑��x (velocity) ��ύX���Ĉړ�������
            /*rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.velocity.y, // Y���͕ύX���Ȃ�
                moveDirection.z * moveSpeed
            );*/

            this.UpdateCamera();
        }
    }

    private void UpdateCamera()
    {
        // Controller�N���X����R�X�e�B�b�N�̓��͒l���擾
        Vector2 rightStickInput = controller.GetRightStick();

        // �J�����̉��ړ�
        if (rightStickInput.x > 0.25f || rightStickInput.x < -0.25f)
        {
            cameraObject.transform.RotateAround(this.transform.position, Vector3.up, rightStickInput.x * Time.deltaTime * 200f);
        }
        // �J�����̏c�ړ�
        float camera_angle_x = cameraObject.transform.localEulerAngles.x;
        Debug.Log(camera_angle_x);
        if (rightStickInput.y > 0.25f && (camera_angle_x < 80f || camera_angle_x <= 360f && camera_angle_x > 180f))
        {
            // ��ړ�
            cameraObject.transform.RotateAround(this.transform.position, cameraObject.transform.right, rightStickInput.y * Time.deltaTime * 200f);
        }
        if (rightStickInput.y < -0.25f && (camera_angle_x > 280f || camera_angle_x >= 0f && camera_angle_x < 180f))
        {
            // ���ړ�
            cameraObject.transform.RotateAround(this.transform.position, cameraObject.transform.right, rightStickInput.y * Time.deltaTime * 200f);
        }
    }
}