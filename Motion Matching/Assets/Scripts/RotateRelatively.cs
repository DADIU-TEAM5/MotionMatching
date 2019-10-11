using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRelatively : MonoBehaviour
{
    public Transform startRotation, rotationToAdd;

    public Transform cubeToRotate;

    // Start is called before the first frame update
    void Start()
    {
        Quaternion relative = Quaternion.Inverse(startRotation.rotation) * rotationToAdd.rotation;

        cubeToRotate.rotation = relative;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
