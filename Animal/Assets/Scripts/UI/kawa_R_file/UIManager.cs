using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public enum UI_ID { GAUGE_HP, GAUGE_REASON, LION_RAGE }
	public List<GameObject> ui_list = new List<GameObject>();
	public Transform canvasParent;

    // キャラごとのRenderTexture(0～3)
    [SerializeField] private RenderTexture[] loseCharaRT;

    // 順位表示用RawImage(0:2位, 1:3位, 2:4位)
    [SerializeField] private RawImage[] rankImage;

    public Slider CreateUI(UI_ID ui_id, Transform pos, int pID) // pIDを追加
	{
		GameObject prefab = null;
		if (ui_id == UI_ID.GAUGE_HP) prefab = Resources.Load("Prefab/UI/HP_ber") as GameObject;
        else if (ui_id == UI_ID.GAUGE_REASON) prefab = Resources.Load("Prefab/UI/Reason_ber") as GameObject;
        else if (ui_id == UI_ID.LION_RAGE) prefab = Resources.Load("Prefab/UI/Lion_Rage_Icon") as GameObject;

		if (prefab != null)
		{
			GameObject uiObj = GameObject.Instantiate(prefab);
			uiObj.transform.SetParent(canvasParent, false);
			uiObj.transform.SetAsLastSibling();

			RectTransform rect = uiObj.GetComponent<RectTransform>();
			if (rect != null)
			{
				// 全体のサイズを少し小さくして画面分割に対応
				rect.localScale = new Vector3(0.8f, 0.8f, 1.0f);
				rect.position = pos.position;

				// 理性ゲージ（Reason）ならHPバーの上に少し重ねる
				if (ui_id == UI_ID.GAUGE_REASON)
				{
					rect.anchoredPosition += new Vector2(0f, 19f);
					rect.localScale = new Vector3(0.8f, 0.6f, 1.0f); // 少し細長く
				}
				else if (ui_id == UI_ID.LION_RAGE)
				{
					// バーの左側にずらす。
					rect.anchoredPosition += new Vector2(-200f, -100f);
					rect.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				}
			}
			ui_list.Add(uiObj);
			return uiObj.GetComponent<Slider>(); // Sliderコンポーネントを返す
		}
		return null;
	}

    public void ShowResult(int[] ranking_index, Character_Status.CharacterType[] player_chara_id)
    {
        // ranking[0] は1位なのでスキップ
        for (int i = 1; i < ranking_index.Length; i++)
        {
            int player_index = ranking_index[i];					// 何番プレイヤーか
            int chara_id = (int)player_chara_id[player_index] - 1;	// その人のキャラID

			// テクスチャを適用する
            rankImage[i - 1].texture = loseCharaRT[chara_id];
            rankImage[i - 1].gameObject.SetActive(true);
        }
    }

}