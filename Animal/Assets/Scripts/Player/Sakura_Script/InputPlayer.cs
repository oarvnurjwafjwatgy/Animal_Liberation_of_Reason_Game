using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]
public class InputPlayer : MonoBehaviour
{
    public float moveSpeed = 5.0f; // �L�����N�^�[�̈ړ����x

    // �Q�Ƃ���R���|�[�l���g
    private Rigidbody rb;
    private Controller controller; // �쐬���� Controller �N���X

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

            // 3. Rigidbody �̑��x (velocity) ��ύX���Ĉړ�������
            rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.velocity.y, // Y���͕ύX���Ȃ�
                moveDirection.z * moveSpeed
            );
        }
    }

}