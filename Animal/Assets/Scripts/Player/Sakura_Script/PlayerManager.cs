using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private const int MaxSupportedPlayers = 4;

    // プレイヤーのプレファブを設定 (インスペクターから設定できるように private を削除)
    [SerializeField] private List<GameObject> PlayerPrefab = new List<GameObject>();
    // プレイヤーの出現位置 (インスペクターから設定できるように private を削除)
    [SerializeField] private List<Transform> PlayerTransforms = new List<Transform>();

    public int playerCount;

    void Start()
    {
        // 接続されているコントローラーの数を参照
        var gamepads = Gamepad.all;
        int gamepadCount = gamepads.Count;

        // スポーン可能な最大人数は、プレハブの数、またはスポーン地点の数の「少ない方」
        int maxPlayers = Mathf.Min(PlayerPrefab.Count, PlayerTransforms.Count);

        // 実際にスポーンする人数は、「接続されたコントローラー数」と「最大人数」の「少ない方」
        int playersToSpawn = Mathf.Min(gamepadCount, maxPlayers);
        playerCount = playersToSpawn;

        if (playersToSpawn < 1)
        {
            Debug.LogWarning($"接続されたコントローラーが {playersToSpawn} 個です。1個以上必要です。");
            return;
        }

        if (playersToSpawn > MaxSupportedPlayers)
        {
            playersToSpawn = MaxSupportedPlayers;
        }

        Debug.Log($"コントローラー {gamepadCount} 個を検知。{playersToSpawn} 人のプレイヤーを生成します。");

        for (int i = 1; i <= playersToSpawn; i++)
        {
            if (Animal_Select.playerChoices[i] == Character_Status.CharacterType.NONE)
            {
                continue;
            }

            int padIndex = i - 1;
            if (padIndex >= gamepads.Count)
            {
                Debug.LogWarning($"{i}Pのキャラは選ばれていますが、コントローラーが足りません。");
                break;
            }

            GameObject selectedPrefab = GetSelectedPlayerPrefab(i);
            if (selectedPrefab == null)
            {
                Debug.LogWarning($"{i}Pのプレファブを取得できませんでした。");
                continue;
            }

            PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: selectedPrefab,
                playerIndex: padIndex,
                controlScheme: "Gamepad",
                pairWithDevice: gamepads[padIndex]
            );

            if (PlayerTransforms[padIndex] != null)
            {
                newPlayer.transform.position = PlayerTransforms[padIndex].position;
                newPlayer.transform.rotation = PlayerTransforms[padIndex].rotation;
            }
            else
            {
                Debug.LogWarning($"P{i} のスポーン地点が設定されていません。");
            }
        }
    }

    private GameObject GetSelectedPlayerPrefab(int playerId)
    {
        Character_Status.CharacterType selectedType = Animal_Select.playerChoices[playerId];

        string resourcePath = selectedType switch
        {
            Character_Status.CharacterType.RHINOCELOS => "Prefab/Player/Sakura_Prefab/サイ/Sai",
            Character_Status.CharacterType.OSTRICH => "Prefab/Player/Sakura_Prefab/ダチョウ/bird",
            Character_Status.CharacterType.LION => "Prefab/Player/Sakura_Prefab/ライオン/Red_lion",
            Character_Status.CharacterType.RATEL => "Prefab/Player/Sakura_Prefab/ラーテル/ratel",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(resourcePath))
        {
            GameObject resourcePrefab = Resources.Load<GameObject>(resourcePath);
            if (resourcePrefab != null)
            {
                return resourcePrefab;
            }
        }

        if (playerId - 1 < PlayerPrefab.Count)
        {
            return PlayerPrefab[playerId - 1];
        }

        return null;
    }
}
