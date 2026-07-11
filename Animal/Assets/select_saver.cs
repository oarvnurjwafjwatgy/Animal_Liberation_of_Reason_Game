using UnityEngine;
using CharaType = CharacterType;

public enum SlotType { PLAYER, CPU, NONE }//スロットの種類:列挙型


public class select_saver : MonoBehaviour
{
    public static select_saver Instance { get; private set; }

    public CharaType[] PlayerChoices = new CharaType[5];
    public SlotType[] SlotTypes = new SlotType[5];  //各スロットが人間かCPUかを保存する配列（1P〜4P用なので要素数は5）

	// 初期化
	void Start()
    {
        // 既にインスタンスが存在するかチェック
        if (Instance != null && Instance != this)
        {
            // 既に存在するなら、新しく生成された自分自身を破棄して重複を防ぐ
            Destroy(this.gameObject);
            return;
        }

        // インスタンスが存在しない場合、自分自身をInstanceとして設定
        Instance = this;

        // シーンを跨いでも破棄されないように設定
        DontDestroyOnLoad(this.gameObject);

        SetSlot(SlotType.CPU);
	}

	//初期設定:ゲーム開始時は「1Pだけが人間、2P〜4Pは自動的にCPU」
	private void SetSlot(SlotType type)
    {
        SlotTypes[1] = SlotType.PLAYER;
        SlotTypes[2] = type;
        SlotTypes[3] = type;
        SlotTypes[4] = type;
	}
}
