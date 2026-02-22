using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [Header("共通エフェクト")]
    public GameObject[] Common_EffectPrefabs;

    [Header("ライオンエフェクト")]
    public GameObject[] Lion_EffectPrefabs;

    [Header("ダチョウエフェクト")]
    public GameObject[] Ostrich_EffectPrefabs;

    [Header("サイエフェクト")]
    public GameObject[] Rhinoceros_EffectPrefabs;

    [Header("ラーテルエフェクト")]
    public GameObject[] Ratel_EffectPrefabs;

  
    /// <summary>
    /// 動物の名前とIDを指定してエフェクトを生成
    /// </summary>
    /// <param name="animalName">動物の名前（"Common", "Lion", "Ostrich", "Rhino", "Ratel"）</param>
    /// <param name="id">その動物内でのエフェクト番号</param>
    /// <param name="position">出す場所</param>
    public void PlayEffect(string animalName, int id, Vector3 position, Quaternion rotation)
    {
        // 1. 使うべき配列を一時的に格納する変数
        GameObject[] targetArray = null;

        // 2. 名前によってどの配列を使うか振り分ける
        switch (animalName)
        {
            case "Common":
                targetArray = Common_EffectPrefabs;
                break;
            case "Lion(Clone)":
                targetArray = Lion_EffectPrefabs;
                break;
            case "Ostrich(Clone)":
                targetArray = Ostrich_EffectPrefabs;
                break;
            case "Rhinoceros(Clone)":
                targetArray = Rhinoceros_EffectPrefabs;
                break;
            case "Ratel(Clone)":
                targetArray = Ratel_EffectPrefabs;
                break;
            default:
                Debug.LogError($"EffectManager: {animalName} という名前のリストは見つかりません。");
                return;
        }

        // 3. 選ばれた配列が空でないか、IDが範囲内かをチェックして生成
        if (targetArray != null && id >= 0 && id < targetArray.Length)
        {
            if (targetArray[id] != null)
            {
                GameObject instance = Instantiate(targetArray[id], position, rotation);
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