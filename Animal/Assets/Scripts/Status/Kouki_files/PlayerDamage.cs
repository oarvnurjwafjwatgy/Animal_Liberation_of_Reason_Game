using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    Character_Status status;    // キャラのステータス
    private int Defences;       // 受け取ったキャラ防御力
    private int Damages;        // 与えるダメージ

    // Start is called before the first frame update
    void Start()
    {
        status = this.GetComponent<Character_Status>();
        Defences = status.GetDefensePower();
        Damages = 0;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDamage(int power)
    {
        Damages = (int)((float)power * (1f - (float)Defences / 100f));
    }
}
