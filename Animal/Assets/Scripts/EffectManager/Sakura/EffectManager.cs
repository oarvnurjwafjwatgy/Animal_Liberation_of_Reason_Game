using UnityEngine;
using System.Collections.Generic;

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

    private Dictionary<Transform, GameObject> activeLoopEffects = new Dictionary<Transform, GameObject>();

    /// <summary>
    /// 動物の名前とIDを指定してエフェクトを生成
    /// </summary>
    /// <param name="animalName">動物の名前（"Common", "Lion", "Ostrich", "Rhino", "Ratel"）</param>
    /// <param name="id">その動物内でのエフェクト番号</param>
    /// <param name="position">出す場所</param>
    
    public void PlayEffect(
    string animalName,
    int id,
    Vector3 position,
    Quaternion rotation,
    Vector3 scale,
    Transform parent = null,
    bool roop = false)
    {
        GameObject[] targetArray = null;
        animalName = animalName.Replace("(Clone)", "");
        animalName = animalName.ToLower();

        switch (animalName)
        {
            case "common": targetArray = Common_EffectPrefabs; break;
            case "lion": targetArray = Lion_EffectPrefabs; break;
            case "ostrich": targetArray = Ostrich_EffectPrefabs; break;
            case "rhinoceros": targetArray = Rhinoceros_EffectPrefabs; break;
            case "ratel": targetArray = Ratel_EffectPrefabs; break;
            default:
                Debug.LogError($"EffectManager: {animalName} という名前のリストは見つかりません。");
                return;
        }

        if (targetArray != null && id >= 0 && id < targetArray.Length)
        {
            if (targetArray[id] != null)
            {
                GameObject instance = Instantiate(targetArray[id], position, rotation, parent);
                instance.transform.localScale = scale;

                //ループしない:2秒後削除
                if (!roop) { Destroy(instance, 2.0f); }
                //ループする:同じ親にエフェクトが出てたら先に消す
                else if (parent != null)
                {
                    StopLoopEffect(parent);
                    activeLoopEffects[parent] = instance;
                }
            }
            else Debug.LogWarning($"EffectManager: {animalName} の ID {id} が空っぽです！");
        }
    }

    public void StopLoopEffect(Transform parent)
    {
        if (parent != null && activeLoopEffects.ContainsKey(parent))
        {
            if (activeLoopEffects[parent] != null) Destroy(activeLoopEffects[parent]);
            activeLoopEffects.Remove(parent);
        }
    }
}