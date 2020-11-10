using UnityEngine;
using System.Collections;

public class FixedUpdateAnimationScript : MonoBehaviour, ISetupable {

    /* Parameters */
    
    public bool isAnimated = false;

    public bool isRotating = false;
    public bool isFloating = false;
    public bool isScaling = false;

    public Vector3 rotationAngle;
    public float rotationSpeed;

    public float floatSpeed;
    public float floatRate;
    
    public Vector3 startScale;
    public Vector3 endScale;
    
    public float scaleSpeed;
    public float scaleRate;
    
    /* Init state */

    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;
    private Vector3 initialLocalScale;
    
    /* State */
    
    private bool goingUp = true;
    private float floatTimer;
   
    private bool scalingUp = true;
    private float scaleTimer;

    void Awake()
    {
        initialLocalRotation = transform.localRotation;
        initialLocalPosition = transform.localPosition;
        initialLocalScale = transform.localScale;
    }

	public void Setup ()
	{
	    // will do nothing on first run, but useful on restart 
	    transform.localRotation = initialLocalRotation;
	    transform.localPosition = initialLocalPosition;
	    transform.localScale = initialLocalScale;

	    goingUp = true;
	    floatTimer = 0f;

	    scalingUp = true;
	    scaleTimer = 0f;
	}
	
	void FixedUpdate () {
        if(isAnimated)
        {
            if(isRotating)
            {
                transform.Rotate(rotationAngle * rotationSpeed * Time.deltaTime);
            }

            if(isFloating)
            {
                floatTimer += Time.deltaTime;
                Vector3 moveDir = new Vector3(0.0f, 0.0f, floatSpeed);
                transform.Translate(moveDir);

                if (goingUp && floatTimer >= floatRate)
                {
                    goingUp = false;
                    floatTimer = 0;
                    floatSpeed = -floatSpeed;
                }

                else if(!goingUp && floatTimer >= floatRate)
                {
                    goingUp = true;
                    floatTimer = 0;
                    floatSpeed = +floatSpeed;
                }
            }

            if(isScaling)
            {
                scaleTimer += Time.deltaTime;

                if (scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, endScale, scaleSpeed * Time.deltaTime);
                }
                else if (!scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, startScale, scaleSpeed * Time.deltaTime);
                }

                if(scaleTimer >= scaleRate)
                {
                    if (scalingUp) { scalingUp = false; }
                    else if (!scalingUp) { scalingUp = true; }
                    scaleTimer = 0;
                }
            }
        }
	}
}
