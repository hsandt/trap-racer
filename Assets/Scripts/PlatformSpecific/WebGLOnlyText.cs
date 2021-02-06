using System.Collections;
using System.Collections.Generic;
using CommonsHelper;
using TMPro;
using UnityEngine;

public class WebGLOnlyText : MonoBehaviour
{
#if UNITY_WEBGL
    [TextArea(4, 10), Tooltip("Text for WebGL build only (e.g. disable gamepad)")]
    public string webGLText;
    
    [Tooltip("Victory text component")]
    private TextMeshProUGUI m_Text;

    void Awake()
    {
        // exceptionally don't fail if not found, to avoid adding errors to WebGL build when inadvertently
        // adding this script to an object without TMP text
        m_Text = GetComponent<TextMeshProUGUI>();
    }
    
    void Start()
    {
        if (m_Text)
        {
            m_Text.text = webGLText;
        }
    }
#endif
}
