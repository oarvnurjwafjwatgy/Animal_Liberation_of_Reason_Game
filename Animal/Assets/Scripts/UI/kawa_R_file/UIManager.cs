using System.Collections.Generic;
using UnityEngine;



public class UIManager : MonoBehaviour
{
    public enum UI_ID
    {
        GAUGE_HP,
        GAUGE_REASON
    }

    public List<GameObject> ui_list = new List<GameObject>();
    public Transform canvasParent;

    void Start()
    {
        canvasParent.transform.localPosition = Vector3.zero;
    }

    public void CreateUI(UI_ID ui_id, Transform pos)
    {
        switch (ui_id)
        {
			//����UI_ID��GAUGE_HP�Ȃ��
			case UI_ID.GAUGE_HP:

				//HP�Q�[�W�������ăL�����o�X�̎q�ɐݒ�
				GameObject ListObjects_HP = GameObject.Instantiate
                (Resources.Load("Prefab/UI/HP_ber")) as GameObject;

                ListObjects_HP.transform.SetParent(canvasParent, false);
                ListObjects_HP.transform.localPosition = pos.position;
                ListObjects_HP.transform.localScale = Vector3.one;
                ListObjects_HP.transform.rotation = Quaternion.identity;
                ui_list.Add(ListObjects_HP);   //���X�g�ɒǉ�

                break;

			//����UI_ID��GAUGE_REASON�Ȃ��
			case UI_ID.GAUGE_REASON:

				//Reason�Q�[�W�������ăL�����o�X�̎q�ɐݒ�
				GameObject ListObjects_Reason = GameObject.Instantiate
                   (Resources.Load("Prefab/UI/Reason_ber")) as GameObject;

                ListObjects_Reason.transform.SetParent(canvasParent, false);
                ListObjects_Reason.transform.localPosition = pos.position;
                ListObjects_Reason.transform.localScale = Vector3.one;
                ListObjects_Reason.transform.rotation = Quaternion.identity;
                ui_list.Add(ListObjects_Reason);   //���X�g�ɒǉ�
                break;
        }
    }
}
