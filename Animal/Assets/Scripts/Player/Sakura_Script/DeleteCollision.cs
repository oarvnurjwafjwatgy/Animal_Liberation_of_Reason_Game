using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteCollision : MonoBehaviour
{
    InputPlayer playerScript;
    bool isRhinocerosSkillRunning; // 「サイのスキルによって生成されたか」を覚えるフラグ

    void Start()
    {
        // 親（Player土台）を取得
        GameObject playerObj = transform.parent.gameObject;
        playerScript = playerObj.GetComponent<InputPlayer>();

        // 同じ階層にサイのモデルがいるかチェック
        Transform sibling = transform.parent.Find("Rhinoceros(Clone)");

        // サイのモデルが存在し、かつ現在「スキル中」であれば特殊フラグを立てる
        if (sibling != null && playerScript != null && playerScript.IsRhinocerosSkillActive())
        {
            isRhinocerosSkillRunning = true;
        }
        else
        {
            // サイではない、あるいはサイだけど通常攻撃（スキル中ではない）の場合は1秒後に削除
            isRhinocerosSkillRunning = false;
            Destroy(this.gameObject, 1f);
        }
    }

    private void Update()
    {
        // サイの「スキル」で生成された判定オブジェクトのみ、終了を監視して消す
        if (isRhinocerosSkillRunning && playerScript != null)
        {
            // プレイヤーがスキルをOFFにしたら即座に削除
            if (playerScript.IsRhinocerosSkillActive() == false)
            {
                Destroy(this.gameObject);
            }
        }
    }
}