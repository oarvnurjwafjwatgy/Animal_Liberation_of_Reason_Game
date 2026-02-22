using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [Header("共通エフェクト：解放０、被ダメージ１、バフ２、デバフ３、死亡４")]
    public GameObject[] Common_EffectPrefabs;

    [Header("ライオンエフェクト:攻撃０、スキル１・２、")]
    public GameObject[] Lion_EffectPrefabs;

    [Header("ダチョウエフェクト:攻撃０、スキル１、")]
    public GameObject[] Ostrich_EffectPrefabs;

    [Header("サイエフェクト:攻撃０、スキル１")]
    public GameObject[] Rhinoceros_EffectPrefabs;

    [Header("ラーテルエフェクト:攻撃０、スキル１・２、")]
    public GameObject[] Ratel_EffectPrefabs;

  
    /// <summary>
    /// 動物の名前とIDを指定してエフェクトを生成
    /// </summary>
    /// <param name="animalName">動物の名前（"Common", "Lion", "Ostrich", "Rhino", "Ratel"）</param>
    /// <param name="id">その動物内でのエフェクト番号</param>
    /// <param name="position">出す場所</param>
    public void PlayEffect(string animalName, int id, Vector3 position, Quaternion rotation,Vector3 scale,Transform parent = null)
    {
        GameObject[] targetArray = null;

        switch (animalName)
        {
            case "Common": targetArray = Common_EffectPrefabs; break;
            case "Lion(Clone)": targetArray = Lion_EffectPrefabs; break;
            case "Ostrich(Clone)": targetArray = Ostrich_EffectPrefabs; break;
            case "Rhinoceros(Clone)": targetArray = Rhinoceros_EffectPrefabs; break;
            case "Ratel(Clone)": targetArray = Ratel_EffectPrefabs; break;
            default:
                Debug.LogError($"EffectManager: {animalName} という名前のリストは見つかりません。");
                return;
        }

        if (targetArray != null && id >= 0 && id < targetArray.Length)
        {
            if (targetArray[id] != null)
            {
                // Instantiate の引数に parent を追加
                GameObject instance = Instantiate(targetArray[id], position, rotation, parent);

                // --- 追加：大きさを変更する処理 ---
                instance.transform.localScale = scale;

                Destroy(instance, 2.0f);
            }
            else
            {
                Debug.LogWarning($"EffectManager: {animalName} の ID {id} が空っぽです！");
            }
        }
        else
        {
            Debug.LogError($"EffectManager: {animalName} の ID {id} は範囲外です。");
        }
    }
}