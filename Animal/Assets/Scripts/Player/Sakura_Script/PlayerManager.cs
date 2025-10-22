using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> PlayerPrefab = new List<GameObject>();
    [SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

    // ������ �y�ϐ���ǉ��z ������
    // ���������v���C���[�i��InputPlayer�j���o���Ă������X�g
    private List<InputPlayer> spawnedPlayers = new List<InputPlayer>();

    // �J�����ݒ肪�����������ǂ������o���Ă����t���O
    private bool isCameraSetupDone = false;
    // ������

    void Start()
    {
        // 1. ���C���J����������
        if (Camera.main != null)
        {
            Camera.main.gameObject.SetActive(false);
            Debug.Log("Main Camera �𖳌������܂����B");
        }

        // 2. �v���C���[�l���̌���
        var gamepads = Gamepad.all;
        int gamepadCount = gamepads.Count;
        int maxPlayers = Mathf.Min(PlayerPrefab.Count, PlayerTransforms.Count);
        int playersToSpawn = Mathf.Min(gamepadCount, maxPlayers);

        if (playersToSpawn < 2)
        {
            Debug.LogWarning($"�ڑ����ꂽ�R���g���[���[�� {playersToSpawn} �ł��B2�ȏ�K�v�ł��B");
            return;
        }
        if (playersToSpawn > 4)
        {
            playersToSpawn = 4;
        }

        Debug.Log($"�R���g���[���[ {gamepadCount} �����m�B{playersToSpawn} �l�̃v���C���[�𐶐����܂��B");

        // 3. �v���C���[�𐶐�
        for (int i = 0; i < playersToSpawn; i++)
        {
            // PlayerInput.Instantiate ���g��
            PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: PlayerPrefab[i],
                playerIndex: i,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[i]
            );

            // �X�|�[���n�_�ɔz�u
            if (PlayerTransforms[i] != null)
            {
                newPlayer.transform.position = PlayerTransforms[i].position;
                newPlayer.transform.rotation = PlayerTransforms[i].rotation;
            }

            // 4. InputPlayer ���擾
            InputPlayer inputPlayer = newPlayer.GetComponent<InputPlayer>();
            if (inputPlayer != null)
            {
                // 5. ���X�g�ɒǉ�
                spawnedPlayers.Add(inputPlayer);
            }
            else
            {
                Debug.LogError($"P{i + 1} ({newPlayer.name}) �̃v���n�u�� InputPlayer �X�N���v�g������܂���I");
            }
        }
    }

    /// <summary>
    /// �v���C���[�̃J�����̕\���̈� (Viewport Rect) ��ݒ肵�܂�
    /// </summary>
    private void SetCameraViewport(Camera cam, int playerIndex, int totalPlayers)
    {
        // (���g�͕ύX�Ȃ�)
        if (totalPlayers == 2)
        {
            if (playerIndex == 0) { cam.rect = new Rect(0f, 0f, 0.5f, 1f); }
            else { cam.rect = new Rect(0.5f, 0f, 0.5f, 1f); }
        }
        else if (totalPlayers == 3)
        {
            if (playerIndex == 0) { cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f); }
            else if (playerIndex == 1) { cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); }
            else { cam.rect = new Rect(0f, 0f, 0.5f, 0.5f); }
        }
        else if (totalPlayers == 4)
        {
            if (playerIndex == 0) { cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f); }
            else if (playerIndex == 1) { cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); }
            else if (playerIndex == 2) { cam.rect = new Rect(0f, 0f, 0.5f, 0.5f); }
            else { cam.rect = new Rect(0.5f, 0f, 0.5f, 0.5f); }
        }
    }

    // (Update �̓R�[�h�̃��W�b�N�����̂܂܍̗p)
    void Update()
    {
        // 1. �����A�ݒ芮���ς݂Ȃ�A�������Ȃ�
        if (isCameraSetupDone)
        {
            return;
        }

        // 2. �����A�v���C���[���܂����X�g�ɒǉ�����Ă��Ȃ���΁i�����O�j�A
        //    �������Ȃ�
        if (spawnedPlayers.Count == 0)
        {
            return;
        }

        // 3. �y�����`�F�b�N�z
        //    ���X�g���̃v���C���[�S���̃J����������OK���inull����Ȃ����j�m�F
        int totalPlayers = spawnedPlayers.Count;
        for (int i = 0; i < totalPlayers; i++)
        {
            // InputPlayer ���擾
            InputPlayer inputPlayer = spawnedPlayers[i];

            // InputPlayer �� GetPlayerCamera() ���Ă�ł݂�
            Camera playerCamera = inputPlayer.GetPlayerCamera();

            // 4. �����A��l�ł��J������ null (�����ł��Ă��Ȃ�) ��������...
            if (playerCamera == null)
            {
                // �i�v���n�u�Őݒ肵�Y��̉\���������j
                // ���̃t���[���� Update �͂�����߂�
                return;
            }
        }

        // 5. �y�ݒ���s�z
        //    (���̍s�܂ł��ǂ蒅���� �� �S���̃J������ null ����Ȃ������I)
        Debug.Log("�S���̃J�������������I��ʕ��������s���܂��B");

        for (int i = 0; i < totalPlayers; i++)
        {
            // ������x�J�������擾�i������ null ����Ȃ����Ƃ͊m�F�ς݁j
            Camera cam = spawnedPlayers[i].GetPlayerCamera();

            // (�O�̂���) �J������L����
            if (!cam.gameObject.activeSelf)
            {
                cam.gameObject.SetActive(true);
            }

            // ��ʕ��������s
            SetCameraViewport(cam, i, totalPlayers);
        }

        // 6. �y�����t���O�z
        //    �ݒ肪�I������̂ŁA�t���O�� true �ɂ���
        isCameraSetupDone = true;
    }
}