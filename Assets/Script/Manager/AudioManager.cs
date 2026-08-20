using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// 게임 전체의 BGM과 효과음 관리
/// 씬이 바뀌어도 유지, 음량 설정은 PlayerPrefs에 저장
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string BgmVolumeKey = "BGM_VOLUME";
    private const string SfxVolumeKey = "SFX_VOLUME";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM")]
    [SerializeField] private AudioClip mainBgm;
    [SerializeField] private AudioClip shopBgm;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainScene";
    [SerializeField] private string shopSceneName = "ShopScene";

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip popupClip;
    [SerializeField] private AudioClip buyClip;
    [SerializeField] private AudioClip errorClip;

    public float BgmVolume { get; private set; } = 0.5f;
    public float SfxVolume { get; private set; } = 0.7f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSetting();
        ApplyVolume();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        PlayBgmByScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 씬 로드가 완료됐을 때 호출
    /// 현재 씬 이름을 기준으로 재생할 BGM을 결정
    /// </summary>
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBgmByScene(scene.name);
    }

    /// <summary>
    /// 씬 이름에 맞는 BGM을 선택해 재생
    /// </summary>
    private void PlayBgmByScene(string sceneName)
    {
        if (sceneName == mainMenuSceneName)
        {
            PlayBgm(mainBgm);
            return;
        }

        if (sceneName == shopSceneName)
        {
            PlayBgm(shopBgm);
            return;
        }
    }

    /// <summary>
    /// 기본 BGM을 반복 재생
    /// AudioManager가 처음 생성될 때 호출
    /// </summary>
    public void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null || clip == null)
        {
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = BgmVolume;
        bgmSource.Play();
    }

    /// <summary>
    /// 버튼 클릭 효과음을 재생
    /// 메뉴 버튼, 타이틀 이동, 게임 종료 같은 UI 입력에서 호출
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip);
    }

    /// <summary>
    /// 팝업 또는 메뉴가 열릴 때 효과음을 재생
    /// 일시정지 메뉴, 결과창, 옵션창 표시 시 호출
    /// </summary>
    public void PlayPopup()
    {
        PlaySfx(popupClip);
    }

    /// <summary>
    /// 구매 성공 효과음을 재생
    /// 상품 구매가 정상 처리됐을 때 호출
    /// </summary>
    public void PlayBuy()
    {
        PlaySfx(buyClip);
    }

    /// <summary>
    /// 실패 또는 불가능 효과음을 재생
    /// 돈 부족, 불가능한 상호작용 같은 상황에서 호출
    /// </summary>
    public void PlayError()
    {
        PlaySfx(errorClip);
    }

    /// <summary>
    /// 전달받은 효과음을 한 번 재생
    /// </summary>
    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, SfxVolume);
    }

    /// <summary>
    /// 전달받은 효과음을 한번 재생
    /// </summary>
    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        ApplyVolume();

        PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 효과음 음량을 변경하고 저장
    /// 옵션 메뉴의 SFX 슬라이더에서 호출
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        ApplyVolume();

        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 저장된 음량 설정을 불러옴
    /// 저장값이 없으면 기본값을 사용
    /// </summary>
    private void LoadVolumeSetting()
    {
        BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, BgmVolume);
        SfxVolume = PlayerPrefs.GetFloat (SfxVolumeKey, SfxVolume);
    }

    /// <summary>
    /// 현재 음량 값을 AudioSource에 적용
    /// </summary>
    private void ApplyVolume()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = BgmVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = SfxVolume;
        }
    }
}
