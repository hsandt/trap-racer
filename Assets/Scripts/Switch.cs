#define DEBUG_SWITCH

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;

public class Switch : MonoBehaviour
{
    /* Animator parameter hashes */
    private static readonly int On = Animator.StringToHash("On");

    /* Assets */
    [Tooltip("Audio clip to play on Toggle Switch")]
    public AudioClip toggleSwitchClip;
    
    /* Sibling components */
    private Animator m_Animator;
    private AudioSource m_AudioSource;

    /* Parameters */
    
    [SerializeField, Tooltip("Color of switch and gates it toggles")]
    private GameColor color = GameColor.Purple;

    /* State vars */

    /// Is the switch on?
    private bool m_On;

    private void Awake()
    {
        m_Animator = this.GetComponentOrFail<Animator>();
        m_AudioSource = this.GetComponentOrFail<AudioSource>();
    }

    private void Start()
    {
        SetOn(true);
    }

    private void SetOn(bool value)
    {
        m_On = value;
        m_Animator.SetBool(On, value);
        
#if DEBUG_SWITCH
        Debug.LogFormat(this, "[Switch] #{0} SetOn: {1}", color, value);
#endif
    }

    private void Toggle()
    {
        SetOn(!m_On);
        m_AudioSource.PlayOneShot(toggleSwitchClip);
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        var characterRun = other.collider.GetComponentOrFail<CharacterRun>();
        characterRun.PlayToggleSwitchAnim();
        
        // this will play a transition animation that will also disable collider for a moment
        // preventing multiple interactions from being chained
        Toggle();
    }
}
