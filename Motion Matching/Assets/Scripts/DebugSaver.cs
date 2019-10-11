using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSaver : MonoBehaviour
{
    public int FrameSaveLength =10;


    public DebugData debugData;

    public MotionFrameVariable NextFrame;

    public KeyCode saveButton;

    MotionFrame[] frames;
    float[] horizontalInput;
    float[] verticalInput;


    // Start is called before the first frame update
    void Start()
    {
        frames = new MotionFrame[FrameSaveLength];
        horizontalInput = new float[FrameSaveLength];
        verticalInput = new float[FrameSaveLength];

        
    }

    // Update is called once per frame
    void Update()
    {



        recordData();


        if (Input.GetKeyDown(saveButton))
        {
            Debug.Log("data saved");

            debugData.frames = frames;
            debugData.horizontalInput = horizontalInput;
            debugData.verticalInput = verticalInput;
        }


    }

    void recordData()
    {
        for (int i = frames.Length - 2; i > 0; i--)
        {

            print(i);
            frames[i + 1] = frames[i];
            horizontalInput[i + 1] = horizontalInput[i];
            verticalInput[i + 1] = verticalInput[i];


        }
        frames[0] = NextFrame.Value;
        verticalInput[0] = Input.GetAxis("Vertical");
        horizontalInput[0] = Input.GetAxis("Horizontal");
    }
}
