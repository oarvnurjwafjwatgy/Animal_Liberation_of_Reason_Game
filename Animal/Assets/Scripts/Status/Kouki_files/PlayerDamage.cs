using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    Character_Status status;    // キャラのステータス
    private int Defences;       // 受け取ったキャラ防御力
    private int Damages;        // 与えるダメージ

    EffectManager Effect_Manager = null;
    GameObject EffectManagerObj = null;


    // Start is called before the first frame update
    void Start()
    {
        status = this.GetComponent<Character_Status>();
        Defences = status.GetDefensePower();
        Damages = 0;


        EffectManagerObj = GameObject.Find("EffectManager");
        Effect_Manager = EffectManagerObj.GetComponent<EffectManager>();
    }

    /*※ここの関数のみ川上流輝が担当しました。(一部は除く)*/
    // 攻撃判定（Collider等）に当たった瞬間の処理
    void OnTriggerEnter(Collider other)
    {
        // 当たった相手のステータスを取得
        Character_Status target = other.GetComponent<Character_Status>();

        // 相手がステータスを持っていて、かつ自分自身でない場合
        if (target != null && target != status)
        {
            // 1. 「自分の」理性解放が乗った今の攻撃力を取得
            int myCurrentAtk = status.CurrentAttackPower;

            // 2. 相手の TakeDamage に「自分の攻撃力」を直接叩き込む
            target.TakeDamage(myCurrentAtk);

			// 3. エフェクト再生(※川上流輝は担当外)
			Vector3 offset = new Vector3(other.gameObject.transform.position.x - 0.2f, other.gameObject.transform.position.y + 0.6f, other.gameObject.transform.position.z);

            Effect_Manager.PlayEffect("Common", 1, offset, this.gameObject.transform.rotation, new Vector3(3f, 3f, 3f));
            Debug.Log($"<color=orange>【攻撃成功】{gameObject.name}が{other.name}に{myCurrentAtk}ダメ送信</color>");
        }
    }

	public void SetDamage(int power)
    {
        Damages = (int)((float)power * (1f - (float)Defences / 100f));
    }
}
