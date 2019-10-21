using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrajectory : MonoBehaviour
{
    public float Speed = 5;
    public float RotationSpeed = 5;
    public float Second = 1f;
    public int SaveInSecond = 10;

    public CapsuleScriptObject capsuleScriptObject;

    private Queue<Vector3> history = new Queue<Vector3>();
    private List<Vector3> future = new List<Vector3>();
    private float timer;


    void Start()
    {
        timer += Time.deltaTime;
        InitializeTrajectory();
        capsuleScriptObject.Capsule = new Capsule();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        var inputs = Vector3.zero;
        Vector3 inputVel = UpdatePlayerState(inputs);

        //get player status now
        var currentPos = transform.localPosition;
        var currentRot = transform.rotation;

        HistoryTrajectory(currentPos);
        capsuleScriptObject.Capsule.TrajectoryHistory = history.ToArray();

        FuturePredict(currentPos, inputVel, currentRot);
        capsuleScriptObject.Capsule.TrajectoryFuture = future.ToArray();
        transToRelative(capsuleScriptObject.Capsule.TrajectoryHistory, currentPos);
        transToRelative(capsuleScriptObject.Capsule.TrajectoryFuture, currentPos);


    }

    private void transToRelative(Vector3[] vector3s, Vector3 current)
    {
        for(int i = 0; i<vector3s.Length; i++)
        {
            vector3s[i] = vector3s[i] - current;
        }
        //vector3s[0] = new Vector3(0, 0, 0);
    }

    private void InitializeTrajectory()
    {
        while (history.Count < SaveInSecond)
        {
            history.Enqueue(transform.localPosition);
            future.Add(transform.localPosition);
        }
    }

    private Vector3 UpdatePlayerState(Vector3 inputs)
    {
        inputs.z = Input.GetAxis("Vertical");
        transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed);

        //get input velocity to move
        var inputVel = inputs * Speed;
        transform.Translate(inputVel * Time.deltaTime);

        return inputVel;
    }

    private void HistoryTrajectory(Vector3 currentPos)
    {
        //save History only in the gap
        if (timer > (Second / SaveInSecond))
        {
            timer = 0;
            history.Dequeue();
            history.Enqueue(currentPos);
        }
    }

    //need to update every time by player input
    private void FuturePredict(Vector3 currentPos, Vector3 inputVel, Quaternion currentRot)
    {
        future[0] = currentPos;
        //var gap = currentRot*(inputVel * Second / SaveInSecond );
        //inputs.z = Input.GetAxis("Vertical");
        var rotation = Quaternion.Euler(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed * 10);

        for (int i = 0; i < SaveInSecond; i++)
        {
            /*
            var increasement = Second / SaveInSecond * i;
            var gap = currentRot * (inputVel * increasement);
            var inputs_increase = increasement * inputs;
            var angle_increase = Quaternion.EulerRotation(inputs_increase);

            var futureP = (currentPos + angle_increase *gap);

            future[i] = futureP;
            */
            var increase = Second / SaveInSecond * i;
            var gap_increase = Quaternion.ToEulerAngles(rotation) * increase;
            var angle_increase = Quaternion.EulerRotation(gap_increase);
            var gap = (inputVel * increase);
            var futureP = (currentPos + angle_increase * currentRot * gap);
            future[i] = futureP;
           

        }

    }
}
