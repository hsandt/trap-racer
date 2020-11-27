using CommonsHelper;
using CommonsPattern;
using UnityEngine;

public class TitleUI : SingletonManager<TitleUI>
{
    /* Sibling components */
    
    private AudioSource m_AudioSource;
    
    /* Child references */
    
    [Tooltip("StageSelectUI root")]
    public GameObject stageSelectUI;
    
    /* Parameters */
    
    [Tooltip("Confirm SFX")]
    public AudioClip confirmSFX;

    protected override void Init()
    {
        m_AudioSource = this.GetComponentOrFail<AudioSource>();
    }

    private void Start()
    {
        stageSelectUI.SetActive(false);
    }

    public void ShowStageSelectUI()
    {
        stageSelectUI.SetActive(true);
    }

    public void PlayConfirmSFX()
    {
        // Audio
        m_AudioSource.PlayOneShot(confirmSFX);
    }
}
