using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nuts : MonoBehaviour
{
    private float deleteTimer;

    EffectManager Effect_Manager = null;
    GameObject EffectManagerObj = null;

    private enum NUTS_EFFICACY
    {
        SPEED_BUFF,     // スピードアップ
        SPEED_DEBUFF,   // スピードダウン
        ATTACK_BUFF,    // 攻撃力アップ
        ATTACK_DEBUFF,  // 攻撃力ダウン
        HP_HEAL,        // HPゲージ回復
        REASON_HEAL,    // 理性解放ゲージ回復

        MAX,
    }

    // Start is called before the first frame update
    void Start()
    {
        // タイマーの初期化
        deleteTimer = 0f;

        EffectManagerObj = GameObject.Find("EffectManager");
        Effect_Manager = EffectManagerObj.GetComponent<EffectManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // 削除タイマーの更新
        this.UpdateDeleteTimer();
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに当たったら、効果を発動
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //// ここに効果 ////

            // ランダムに効果を付与
            NUTS_EFFICACY random_efficacy = (NUTS_EFFICACY)UnityEngine.Random.Range(0, (int)NUTS_EFFICACY.MAX);
            // エフェクトのナンバー
            int effect_num = 0;

            Character_Status other_chara_status = other.gameObject.GetComponent<Character_Status>();
            if (other_chara_status != null)
                switch (random_efficacy)
                { 
                    case NUTS_EFFICACY.SPEED_BUFF:      other_chara_status.SetSpeedBuff();      effect_num = 2; break;
                    case NUTS_EFFICACY.SPEED_DEBUFF:    other_chara_status.SetSpeedDebuff();    effect_num = 3; break;
                    case NUTS_EFFICACY.ATTACK_BUFF:     other_chara_status.SetAttackBuff();     effect_num = 2; break;
                    case NUTS_EFFICACY.ATTACK_DEBUFF:   other_chara_status.SetAttackDebuff();   effect_num = 3; break;
                    case NUTS_EFFICACY.HP_HEAL:         other_chara_status.SetNutsHpHeal();     effect_num = 2; break;
                    case NUTS_EFFICACY.REASON_HEAL:     other_chara_status.SetNutsReasonHeal(); effect_num = 2; break;
                }
            
            // エフェクト生成
            Vector3 offset = new Vector3(other.gameObject.transform.position.x - 0.7f, other.gameObject.transform.position.y, other.gameObject.transform.position.z - 1.0f);
            Effect_Manager.PlayEffect("Common", effect_num, offset, this.gameObject.transform.rotation, new Vector3(0.5f, 0.5f, 0.5f), other.gameObject.transform);

            // ItemManagerの生成した木の実の数を減らす
            GameObject.Find("ItemManager").GetComponent<ItemManager>().DecreaseNutsCount(1);
            // 自身を削除する
            Destroy(this.gameObject);
        }
    }

    // 削除タイマーの更新
    private void UpdateDeleteTimer()
    {
        // タイマーの加算
        deleteTimer += Time.deltaTime;

        // 10秒経過したら、自動で消える
        if (deleteTimer > 10f)
        {
            // ItemManagerの生成した木の実の数を減らす
            GameObject.Find("ItemManager").GetComponent<ItemManager>().DecreaseNutsCount(1);
            // 自身を削除する
            Destroy(this.gameObject);
        }
    }
}
