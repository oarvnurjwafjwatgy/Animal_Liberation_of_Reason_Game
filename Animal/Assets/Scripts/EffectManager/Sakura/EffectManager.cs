using UnityEngine;

public class EffectManager : MonoBehaviour
{
    // インスペクターで複数のプレハブを登録できるようにします
    [Header("エフェクトのリスト（FBXのプレハブを登録）")]
    public GameObject[] effectPrefabs;

    /// <summary>
    /// 指定したIDのエフェクトを指定した位置に生成します
    /// </summary>
    /// <param name="id">配列のインデックス番号</param>
    /// <param name="position">発生させる場所</param>
    public void PlayEffect(int id, Vector3 position)
    {
        // 1. 配列が空でないかチェック
        if (effectPrefabs == null || effectPrefabs.Length == 0)
        {
            Debug.LogWarning("EffectManager: プレハブが一つも登録されていません！");
            return;
        }

        // 2. 指定されたIDが配列の範囲内かチェック（エラー防止）
        if (id >= 0 && id < effectPrefabs.Length)
        {
            if (effectPrefabs[id] != null)
            {
                // IDに対応するエフェクトを生成
                GameObject instance = Instantiate(effectPrefabs[id], position, Quaternion.identity);

                // 2秒後に消去
                Destroy(instance, 2.0f);
            }
            else
            {
                Debug.LogWarning($"EffectManager: ID {id} の要素が空です！");
            }
        }
        else
        {
            Debug.LogError($"EffectManager: ID {id} は範囲外です。0 から {effectPrefabs.Length - 1} の間で指定してください。");
        }
    }
}