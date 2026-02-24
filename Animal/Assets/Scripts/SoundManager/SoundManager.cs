using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // インスペクターで2つの AudioSource を割り当てます
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    // SEのリスト（配列）
    [SerializeField] private AudioClip[] seList;




    // --- BGMの操作 ---
    // BGMを途中で変えたい時に使います
    public void ChangeBGM(AudioClip newBgm)
    {
        bgmSource.Stop();
        bgmSource.clip = newBgm;
        bgmSource.Play();
    }


    public void StartBGM(AudioClip bgm)
    {
        bgmSource.clip = bgm;
        bgmSource.Play();
    }

    // --- SEの操作 ---
    public void PlaySE(int id)
    {
        if (id >= 0 && id < seList.Length)
        {
            // seSource（SE用の出口）を使って鳴らす
            seSource.PlayOneShot(seList[id]);
        }
    }

}

