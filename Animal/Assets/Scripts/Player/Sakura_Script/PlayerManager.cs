using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // �� PlayerInput ���g�����߂ɕK�{�ł�

public class PlayerManager : MonoBehaviour
{
    //�v���C���[�̃v���t�@�u��ݒ� (�C���X�y�N�^�[����ݒ�ł���悤�� private ���폜)
    [SerializeField] private List<GameObject> PlayerPrefab = new List<GameObject>();
    //�v���C���[�̏o���ʒu (�C���X�y�N�^�[����ݒ�ł���悤�� private ���폜)
    [SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

    void Start()
    {
        // �ڑ�����Ă���R���g���[���[�̐����Q��
        var gamepads = Gamepad.all;
        int gamepadCount = gamepads.Count;

        // �X�|�[���\�ȍő�l���́A�v���n�u�̐��A�܂��̓X�|�[���n�_�̐��́u���Ȃ����v
        int maxPlayers = Mathf.Min(PlayerPrefab.Count, PlayerTransforms.Count);

        // ���ۂɃX�|�[������l���́A�u�ڑ����ꂽ�R���g���[���[���v�Ɓu�ő�l���v�́u���Ȃ����v
        // (��: �R���g���[���[��5�ł��AmaxPlayers��4�Ȃ�A4�l�܂�)
        int playersToSpawn = Mathf.Min(gamepadCount, maxPlayers);

        // �v�]: 2�`4�l�̏ꍇ�̂ݐ�������
        if (playersToSpawn < 1)
        {
            Debug.LogWarning($"�ڑ����ꂽ�R���g���[���[�� {playersToSpawn} �ł��B2�ȏ�K�v�ł��B");
            return; // 2�l�����Ȃ珈���𒆒f
        }

        // (����4�l��葽���Ă�4�l�ɐ�������ꍇ)
        if (playersToSpawn > 4)
        {
            playersToSpawn = 4;
        }

        Debug.Log($"�R���g���[���[ {gamepadCount} �����m�B{playersToSpawn} �l�̃v���C���[�𐶐����܂��B");

        // ���肵���l�� (playersToSpawn) �������[�v�i����ŃG���[�͋N���܂���j
        for (int i = 0; i < playersToSpawn; i++)
        {
            // ���d�v�� Instantiate �̑���� PlayerInput.Instantiate ���g��
            // ����ɂ��A�v���n�u�����ƃR���g���[���[���蓖�Ă𓯎��ɍs��
            PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: PlayerPrefab[i],         // i�Ԗڂ̃v���n�u (P1=Lion, P2=Rhino...)
                playerIndex: i,                // �v���C���[�ԍ� (0, 1, 2, 3)
                controlScheme: "Gamepad",      // "Gamepad" �X�L�[�}���g��
                pairWithDevice: gamepads[i]    // i�Ԗڂ̃R���g���[���[�����蓖��
            );

            // ���������v���C���[���Ai�Ԗڂ̃X�|�[���n�_�Ɉړ��E��]������
            if (PlayerTransforms[i] != null)
            {
                newPlayer.transform.position = PlayerTransforms[i].position;
                newPlayer.transform.rotation = PlayerTransforms[i].rotation;
            }
            else
            {
                Debug.LogWarning($"P{i + 1} �̃X�|�[���n�_���ݒ肳��Ă��܂���B");
            }
        }
    }

    // Update �͋�̂܂܂�OK
    void Update()
    {

    }
}