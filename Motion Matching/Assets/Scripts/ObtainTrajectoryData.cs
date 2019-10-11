using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObtainTrajectoryData : MonoBehaviour
{

    public AnimClip animationClip;

    public AnimationClipTrajectory trajectoryData;
   


    public float LengthOfPast,LengthOfFuture = 1;
    public int PastSteps,FutureSteps = 10;



    public int amountOfFramesPerSecond = 100;


    List<Vector3[]> listOfTrajectoryPositions;
    List<Quaternion[]> lisOfTrajectoryRotations;



    // Start is called before the first frame update
    void Start()

    {

        listOfTrajectoryPositions = new List<Vector3[]>();
        lisOfTrajectoryRotations = new List<Quaternion[]>();


        getTrajectorydataFromClip();

        

    }

    // Update is called once per frame
    void Update()
    {

       

    }


    void getTrajectorydataFromClip()
    {
        print("starting to get trajectory");

        for (int i =(int)( 10 + LengthOfPast * amountOfFramesPerSecond); i < animationClip.Frames.Count - (LengthOfFuture * amountOfFramesPerSecond); i++)
        {
                //print("doing interation " + i);

                Vector3[] pastTrajectory = new Vector3[PastSteps];
            Quaternion[] pastTrajectoryRotation = new Quaternion[PastSteps];

                Vector3[] futureTreajectory = new Vector3[FutureSteps];
            Quaternion[] futureTrajectoryRotation = new Quaternion[FutureSteps];



            int animationIncrementPast = (int) LengthOfPast* amountOfFramesPerSecond / PastSteps;
                int animationIncrementFuture = (int)LengthOfFuture * amountOfFramesPerSecond / FutureSteps;

            AnimationJointPoint currentJoint = animationClip.Frames[i].JointPoints.Find(x => x.Name.Contains("Hips"));
                Vector3 currentPosition = currentJoint.Position;
            Quaternion currentRotation = currentJoint.Rotation;


            //Debug.DrawLine(currentPosition, Vector3.up*0.1f, Color.blue, 20);

                for (int j = 0; j < PastSteps; j++)
                {
                AnimationJointPoint jointPoint = animationClip.Frames[i - (animationIncrementPast * (j + 1))].JointPoints.Find(x => x.Name.Contains("Hips"));
                    Vector3 pasPosition = jointPoint.Position;
                Quaternion pastRotation = jointPoint.Rotation;


               // print(pasPosition);
                    

                    Vector3 relativePosition = pasPosition - currentPosition;
                Quaternion relativeRotation = pastRotation * Quaternion.Inverse(currentRotation);

                    pastTrajectory[j] = relativePosition;
                pastTrajectoryRotation[j] = relativeRotation;
                }

                for (int j = 0; j < FutureSteps; j++)
                {
                AnimationJointPoint jointPoint = animationClip.Frames[i + (animationIncrementFuture * (j + 1))].JointPoints.Find(x => x.Name.Contains("Hips"));

                    Vector3 futurePosition = jointPoint.Position;
                Quaternion futureRotation = jointPoint.Rotation;
                    

                    

                    Vector3 relativePosition = futurePosition - currentPosition;
                Quaternion relativeRotation = futureRotation * Quaternion.Inverse(currentRotation);

                futureTreajectory[j] = relativePosition;
                futureTrajectoryRotation[j] = relativeRotation;

                }

                Vector3[] fullTrajectory = new Vector3[FutureSteps + PastSteps];
            Quaternion[] fullRotationTrajectory = new Quaternion[FutureSteps + PastSteps];


                pastTrajectory.CopyTo(fullTrajectory, 0);
            pastTrajectoryRotation.CopyTo(fullRotationTrajectory, 0);

                futureTreajectory.CopyTo(fullTrajectory, PastSteps);
            futureTrajectoryRotation.CopyTo(fullRotationTrajectory, PastSteps);


                listOfTrajectoryPositions.Add(fullTrajectory);
            lisOfTrajectoryRotations.Add(fullRotationTrajectory);
                
                    
                }


            

        
        trajectoryData.name = animationClip.name;
        trajectoryData.StartFrame =(int) (10 + LengthOfPast * amountOfFramesPerSecond);
        trajectoryData.EndFrame = (int)(animationClip.Frames.Count - (LengthOfFuture * amountOfFramesPerSecond));


        trajectoryData.trajectoriesPositions = listOfTrajectoryPositions;
        trajectoryData.trajectoryRotations = lisOfTrajectoryRotations;

        print("trajectories stored");
        }


    /*
    private void OnDrawGizmos()
    {
        


        for (int i = 0; i < animationClip.Frames.Count; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(animationClip.Frames[i].JointPoints.Find(x => x.Name.Contains("Hips")).Position, 0.01f);

            if(i  >= trajectoryData.StartFrame && i <= trajectoryData.EndFrame)
            {
                for (int j = 0; j < trajectoryData.trajectories[0].Length; j++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(trajectoryData.trajectories[i - trajectoryData.StartFrame][j], 0.02f);
                }
                
            }
        }
        

    }*/


}

