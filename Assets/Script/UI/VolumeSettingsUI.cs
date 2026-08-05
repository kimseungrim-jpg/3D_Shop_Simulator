using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 옵션 메뉴의 음량 슬라이더와 AudioManager를 연결
/// 옵션 패널이 열릴 때 현재 음량 값을 슬라이더에 반영
/// </summary>
public class VolumeSettingsUI : MonoBehaviour
{
    [Header("음량 슬라이더")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        if (AudioManager.instance == null)
        {
            return;
        }

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(AudioManager.instance.BgmVolume);
            bgmSlider.onValueChanged.AddListener(AudioManager.instance.SetBgmVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(AudioManager.instance.SfxVolume);
            sfxSlider.onValueChanged.AddListener(AudioManager.instance.SetSfxVolume);
        }
    }

    private void OnDisable()
    {
        if (AudioManager.instance == null)
        {
            return;
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(AudioManager.instance.SetBgmVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(AudioManager.instance.SetSfxVolume);
        }
    }
}
