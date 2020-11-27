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

    [Tooltip("Cancel SFX: go back to title")]
    public AudioClip cancelSFX;

    [Tooltip("Start game SFX: confirm stage selection on title menu")]
    public AudioClip startGameSFX;

    [Tooltip("Countdown SFX: on each countdown")]
    public AudioClip countDownSFX;

    [Tooltip("Start race SFX: after countdown")]
    public AudioClip startRaceSFX;
    
    /// Enum of all SFX
    public enum SFX
    {
        Menu,
        Confirm,
        Cancel,
        StartGame,
        CountDown,
        StartRace
    }
    
    private AudioClip[] CreateAudioClipArray()
    {
        return new[]
        {
            menuSFX,
            confirmSFX,
            cancelSFX,
            startGameSFX,
            countDownSFX,
            startRaceSFX
        };
    }

    /// Array of SFX audio clip, by SFX enum index
    private AudioClip[] m_AudioClips;

    public AudioClip StartGameSfx
    {
        get => startGameSFX;
        set => startGameSFX = value;
    }

    protected override void Init()
    {
        m_AudioSource = this.GetComponentOrFail<AudioSource>();
        
        m_AudioClips = CreateAudioClipArray();
    }

    /// Generic method to play SFX by enum value
    public void PlaySFX(SFX sfxType)
    {
        // Audio
        m_AudioSource.PlayOneShot(m_AudioClips[(int)sfxType]);
    }
}
