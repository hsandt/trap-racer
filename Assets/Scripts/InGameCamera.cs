using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InGameCamera : MonoBehaviour
{
    /* External references */
    [Tooltip("_Characters parent transform")]
    public Transform charactersParent;
    
    /* State vars */
    // List of character transforms
    private List<Transform> characterTransforms = new List<Transform>();
    
    void Start()
    {
        foreach (Transform characterTr in charactersParent)
        {
            characterTransforms.Add(characterTr);
        }
        Debug.Assert(characterTransforms.Count > 0, "No character transforms found");
    }

    void LateUpdate()
    {
        // center position between all the characters on X, but preserve Y for stability
        transform.position = new Vector3(characterTransforms.Average(tr => tr.position.x), transform.position.y, transform.position.z);
    }
}
