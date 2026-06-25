using UnityEngine;
using CharaType = CharacterType;

public class select_saver : MonoBehaviour
{
    public static select_saver Instance { get; private set; }

    public CharaType[] PlayerChoices = new CharaType[5];

    // Start is called before the first frame update
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

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
