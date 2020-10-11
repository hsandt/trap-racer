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
    private Transform m_FlagTr;

    private void Start()
    {
        m_FlagTr = null;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.gameObject.layer == Layers.Character)
        {
            // transfer flag to opponent
            var opponentFlagBearer = other.collider.GetComponentOrFail<FlagBearer>();
            opponentFlagBearer.BearFlag(m_FlagTr);
            m_FlagTr = null;
        }
    }

    public void BearFlag(Transform flagTr)
    {
        flagTr.SetParent(flagAnchor, worldPositionStays: false);
    }
}
