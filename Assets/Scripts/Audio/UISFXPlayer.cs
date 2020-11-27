using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

public class UISFXPlayer : SingletonManager<UISFXPlayer>
{
    /* Sibling components */
    
    private AudioSource m_AudioSource;
    
    /* Parameters */
    
    [Tooltip("Menu SFX: open menu")]
    public AudioClip menuSFX;

    [Tooltip("Confirm SFX: click on button or press Confirm")]
    public AudioClip confirmSFX;

    protected override void Init()
    {
        m_AudioSource = this.GetComponentOrFail<AudioSource>();
    }
    
    public void PlayMenuSFX()
    {
        // Audio
        m_AudioSource.PlayOneShot(menuSFX);
    }
    
    public void PlayConfirmSFX()
    {
        // Audio
        m_AudioSource.PlayOneShot(confirmSFX);
    }
}
