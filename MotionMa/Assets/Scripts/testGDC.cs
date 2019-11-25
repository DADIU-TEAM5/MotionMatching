using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testGDC : MonoBehaviour
{
    struct Stance
    {

    }
    struct TrajectoryPoint
    {
        //position in the world
        Vector3 m_Position;
        // the oritation??? the facing???
        float m_Sight;
        //you want there at a specific time in the future
        float m_TimeDelay;
    }
    
    //future position
    //future orientation
    //future velocity/acceleration

    //disired goal, sent by gameplay each frame
    struct Goal
    {
        //4 or 5 future trajectory for next second in the future
        List<TrajectoryPoint> m_DesiredTrajectory;

        //... find what you think is important
        Stance m_DesiredStance;
    }

    //everything we need to compute the cost function
    struct Pose
    {
        public int AnimationIndex;
        public float AnimationTime;

        public List<AnimationJointPoint> KeyPoints;
    }

    int m_CurrentAnimIndex;
    float m_CurrentAnimTime;
    List<Pose> m_Poses;

    void AmoUpdate(Goal goal, float dt)
    {
        m_CurrentAnimTime += dt;
        //would lerp between two gap animation
        //!!! currentPose is the lerp from these animation
        Pose currentPose = EvaluateLerpedPoseFromData(m_CurrentAnimIndex,m_CurrentAnimTime);

        float bestCost = float.MaxValue;
        //do we need the pose
        Pose bestPose = m_Poses[0];

        //m_pose seems the mocap data
        for(int i = 0; i < m_Poses.Count; i++)
        {
            Pose candidatePose = m_Poses[i];

            //every candidate jumping point has a cost
            float thisCost = ComputerCost(currentPose, candidatePose, goal);

            if(thisCost < bestCost)
            {
                bestCost = thisCost;
                bestPose = candidatePose;
            }
        }

        bool theWinnerIsAtTheSameLocation =
            m_CurrentAnimIndex == bestPose.AnimationIndex && Math.Abs(m_CurrentAnimTime - bestPose.AnimationIndex) < 0.2f;

        if (!theWinnerIsAtTheSameLocation)
        {
            //blend to the winning location
            m_CurrentAnimIndex = bestPose.AnimationIndex;
            m_CurrentAnimTime = bestPose.AnimationTime;

            //0.25f is the blend time
            PlayAnimStartingAtTime(m_CurrentAnimIndex,m_CurrentAnimTime, 0.25f);
        }
    }

    private void PlayAnimStartingAtTime(int m_CurrentAnimIndex, float m_CurrentAnimTime, float v)
    {
        throw new NotImplementedException();
    }



    //Trick2: Just check where a piece of animation brings you if you play it
    //Seems we could use greedy search
    private float ComputerCost(Pose currentPose, Pose candidatePose, Goal goal)
    {
        float cost = 0.0f;

        //how much the candidate jumping position matches the current situation
        cost += ComputeCurrentCost(currentPose, candidatePose);

        //this is our responsivity slider
        float resposivity = 1.0f;

        //how much the candidate piece of motion matches the desired trajectory
        cost += resposivity * ComputeFutureCost(candidatePose, goal);

        return cost;
    }

    private float ComputeFutureCost(Pose candidatePose, Goal goal)
    {
        throw new NotImplementedException();
    }


    //Pose/Velocity Matching
    //Local Velocity
    //feet position 
    //feet velocities
    //option if you have Weapon Position
    // the hips oritation

    //Trick, precompute and save with the animation for fast cost computation
    private float ComputeCurrentCost(Pose currentPose, Pose candidatePose)
    {
        //need to do a trick here, only match the keyBones
        throw new NotImplementedException();
    }

    private Pose EvaluateLerpedPoseFromData(int m_CurrentAnimIndex, float m_CurrentAnimTime)
    {
        throw new NotImplementedException();
    }


    //Trajectory simulation choices
    //displacement from animation??
    //displacement from Simulation Code??
    //we can combine??? 

    //Spring damper on velocity
    //-> predictable and comfortable

    //Trick3: Clamp the entity 15cm aroudn the simulation??

    //!!rotation correction
    //orientation corrections
    //!!!!let anim decide rotation but
    //correct to match future desired position or desired orientation


}
