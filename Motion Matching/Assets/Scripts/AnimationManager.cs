using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

public class AnimationManager : MonoBehaviour
{
    public TextAsset CSVFile;

    public Transform Skeleton;

    private AnimationClip AnimationClip;

    private Dictionary<string, Transform> SkeletonJoints = new Dictionary<string, Transform>();

    private float localTimer = 0f;

    void Awake() {
        AnimationCSVImporter importer = new AnimationCSVImporter(CSVFile);
        AnimationClip = importer.ParseCSV();

        GetAllChildren(Skeleton);
    }

    void Start()
    {
        //ApplyFrameToJoints(AnimationClip.Frames[0]);
        StartCoroutine(PlayAnimationOnce());
    }

    private IEnumerator WaitThenPlay() {

        yield return new WaitForSeconds(5);

        StartCoroutine(PlayAnimationOnce());
    }

    public IEnumerator PlayAnimationOnce() {
        localTimer = 0f;
        var index = 0;

        while (index < AnimationClip.Frames.Count) {
            localTimer += Time.deltaTime;
            Debug.Log(localTimer);

            var frame = AnimationClip.Frames[index];
            if (localTimer * 1000 >= frame.Time) {
                ApplyFrameToJoints(frame);
                index++;
            }

            yield return null;
        }        
    }

    private void ApplyFrameToJoints(AnimationFrame frame) {
        foreach (var jointPoint in frame.JointPoints) {
            if (!SkeletonJoints.Keys.Contains(jointPoint.Name)) {
                //Debug.LogError($"{jointPoint.Name} is not in the {Skeleton.name}");
                continue;
            }

             
            var negativeJointPoint = AnimationClip.Frames[0].JointPoints.First(x => x.Name == jointPoint.Name);

            var joint = SkeletonJoints[jointPoint.Name];
            
            // Based on negative joint
            var newEulerRot = jointPoint.Rotation.eulerAngles + negativeJointPoint.Rotation.eulerAngles;
            joint.rotation = Quaternion.Euler(newEulerRot);
            joint.position = jointPoint.Position;

            // Default
            //joint.SetPositionAndRotation(jointPoint.Position, jointPoint.Rotation);
        } 
    }

    private void GetAllChildren(Transform trans) {
        foreach (Transform child in trans) {
            if (child.childCount > 0) GetAllChildren(child);
            SkeletonJoints.Add(child.name, child);
        }
    }
}
