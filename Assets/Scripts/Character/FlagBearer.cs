using System.Collections;
using System.Collections.Generic;
using UnityConstants;
using UnityEngine;

using CommonsHelper;

public class FlagBearer : MonoBehaviour
{
    /* Child references */
    [Tooltip("Anchor to attach flag to")]
    public Transform flagAnchor;
    
    /* State vars */

    /// Hold flag transform. Null if flag is not hold.
    private Transform m_FlagTr = null;

    private void OnCollisionEnter2D(Collision2D other)
    {
        // only react to collision in this direction: give flag to the opponent
        //  (this block won't be entered on flag stealer's side)
        // we may also use a dedicated flag hitbox on a layer Flag that collides with layer Character
        //  so we know we are always checking collision between stealing character and flag
        if (other.collider.gameObject.layer == Layers.Character && m_FlagTr != null)
        {
            // transfer flag to opponent
            var opponentFlagBearer = other.collider.GetComponentOrFail<FlagBearer>();
            opponentFlagBearer.BearFlag(m_FlagTr);
            m_FlagTr = null;
        }
    }

    public bool HasFlag()
    {
        return m_FlagTr != null;
    }

    public void BearFlag(Transform flagTr)
    {
        flagTr.SetParent(flagAnchor, worldPositionStays: false);
        m_FlagTr = flagTr;
    }
}
