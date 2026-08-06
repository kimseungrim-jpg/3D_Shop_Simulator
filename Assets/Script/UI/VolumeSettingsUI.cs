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
        if (AudioManager.Instance == null)
        {
            return;
        }

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(AudioManager.Instance.BgmVolume);
            bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBgmVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
            sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);
        }
    }

    private void OnDisable()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetBgmVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetSfxVolume);
        }
    }
}
