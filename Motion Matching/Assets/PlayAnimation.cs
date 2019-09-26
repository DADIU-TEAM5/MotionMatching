using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    public MotionManager MotionManager;

    private MotionClipData ClipToPlay;


    // Start is called before the first frame update
    void Start()
    {
        ClipToPlay = MotionManager.MotionClips.First();
        StartCoroutine(Play());
    }

    private IEnumerator Play() {
        for (int i = 0; i < ClipToPlay.MotionFrames.Length; i++) {
            Debug.Log(i);
            MotionManager.NextFrame.Value = ClipToPlay.MotionFrames[i];
            yield return null;
        }
    }

}
