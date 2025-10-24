using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���̃X�N���v�g�� Rigidbody �� Controller ���K�v�ł�
// (Controller �� Rigidbody ��v�����Ă���̂ŁARequireComponent �͂ǂ��炩�Е��ł��@�\���܂����A
//  PlayerMovement ������ Rigidbody ���g�����߁A������ɂ������̂�������₷���ł�)
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]

public class Player : MonoBehaviour
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

        // ���� controller �� null �̏ꍇ�̓G���[���O���o���Ă����ƈ��S�ł�
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
            //    (leftStickInput.y �͉��E��O�Ȃ̂� Vector3 �� z �ɑΉ�)
            Vector3 moveDirection = new Vector3(leftStickInput.x, 0, leftStickInput.y);

            // 3. Rigidbody �̑��x (velocity) ��ύX���Ĉړ�������
            //    (Y���̑��x�͌��݂̏d�͂Ȃǂɂ�闎�����x���ێ�)
            rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.velocity.y, // Y���͕ύX���Ȃ�
                moveDirection.z * moveSpeed
            );
        }
    }
}