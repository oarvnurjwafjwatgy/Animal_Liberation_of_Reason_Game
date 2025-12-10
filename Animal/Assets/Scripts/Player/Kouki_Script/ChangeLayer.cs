using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLayer : MonoBehaviour
{
    [SerializeField] private string layerName = default;

    [ContextMenu("Ghost")] public void SetLayer()
    {
        // レイヤー名からレイヤー番号を取得する
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"Layer name {layerName} is not found.");
            return;
        }

        // 自身のレイヤーをlayerにする
        gameObject.layer = layer;

        // 全ての子オブジェクトのレイヤーを切り替える
        RecursiveSetLayer(gameObject, layer);
    }

    private void RecursiveSetLayer(GameObject targetObject, int layer)
    {
        // 対象オブジェクトの子オブジェクトをチェックする
        foreach (Transform child in targetObject.transform)
        {
            // 子オブジェクトのレイヤーを切り替える
            GameObject childObject = child.gameObject;
            childObject.layer = layer;

            // 再帰的に全ての子オブジェクトを処理する
            RecursiveSetLayer(childObject, layer);
        }
    }
}
