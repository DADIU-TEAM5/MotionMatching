using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringDamp : MonoBehaviour
{
    // Start is called before the first frame update
    public CapsuleScriptObject PlayerTrajectoryCapusule;
    public int SaveInSecond;
    public int RotationSpeed;
    public int Speed;

    ////for debug
    //public Vector3 Velocity;
    //public Vector3 Direction;

    private Queue<Vector3> _history = new Queue<Vector3>();
    private List<Vector3> _future = new List<Vector3>();
    private Queue<Vector3> _historyDirection = new Queue<Vector3>();
    private List<Vector3> _futureDirection = new List<Vector3>();

    private float _timer;
    private Vector3 _lastVel;
    void Start()
    {
        InitializeTrajectory();
        PlayerTrajectoryCapusule.Capsule = new Capsule();
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        var inputs = Vector3.zero;
        //var rotationPlayer = Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed;

        Vector3 inputVel = UpdatePlayerState(inputs);
        GetRelativeTrajectory(inputVel);
    }

    private Vector3 UpdatePlayerState(Vector3 inputs)
    {
        inputs.z = Input.GetAxis("Vertical");
        //inputs.x = Input.GetAxis("Horizontal");
        //get input velocity to move
        var inputVel = inputs * Speed;
        transform.Translate(inputVel * Time.deltaTime);
        transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed);

        return inputVel;
    }

    //todo add attack motion
    private void GetRelativeTrajectory(Vector3 inputVel)
    {
        var currentPos = transform.position;
        var currentRot = transform.rotation;


        HistoryTrajectory(currentPos, currentRot);
        PlayerTrajectoryCapusule.Capsule.TrajectoryHistory = _history.ToArray();
        PlayerTrajectoryCapusule.Capsule.TrajectoryDirctionHistory = _historyDirection.ToArray();


        FuturePredict(currentPos, inputVel, currentRot);
        PlayerTrajectoryCapusule.Capsule.TrajectoryFuture = _future.ToArray();
        PlayerTrajectoryCapusule.Capsule.TrajectoryDirctionFuture = _futureDirection.ToArray();

        transToRelative(PlayerTrajectoryCapusule.Capsule.TrajectoryHistory, currentPos);
        transToRelative(PlayerTrajectoryCapusule.Capsule.TrajectoryFuture, currentPos);
    }

    private void transToRelative(Vector3[] vector3s, Vector3 current)
    {
        for (int i = 0; i < vector3s.Length; i++)
        {
            vector3s[i] = transform.InverseTransformDirection((vector3s[i] - current)); //change it to relative
        }
    }
    private void HistoryTrajectory(Vector3 currentPos, Quaternion currentRot)
    {
        //save History only in the gap
        if (_timer > (1f / 30))
        {
            _timer = 0;
            var lastPos = _history.ToArray()[SaveInSecond - 1];

            _history.Dequeue();
            _history.Enqueue(currentPos);

            _historyDirection.Dequeue();
            _historyDirection.Enqueue(Quaternion.Inverse(currentRot) * (currentPos - lastPos));
        }
    }


    private void FutureWithSpring(Vector3 inputVel)
    {
        var inputRot = Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed;
        var angularVelocity = Quaternion.Euler(inputRot / (1 / 30));

    }

    private void FuturePredict(Vector3 currentPos, Vector3 inputVel,
                               Quaternion currentRot)
    {
        //_future[0] = currentPos;
        int someThing = 1;
        var rotation = Quaternion.Euler(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed * 30);


        var initVel = inputVel;
        //Debug.Log("last" + _lastVel.z);
        //Debug.Log("input" + inputVel.z);

        //Debug.Log("sum" + _lastVel.z * inputVel.z);
        for (int i = 0; i < SaveInSecond; i++)
        {
            var increase = 1f / 30 * i;//Second / SaveInSecond * i;
            var gap_increase = Quaternion.ToEulerAngles(rotation) * increase;
            var angle_increase = Quaternion.EulerRotation(gap_increase);
            var gap = (initVel * increase * someThing);
            var futureP = (currentPos + angle_increase * currentRot * gap);
            _future[i] = futureP;
        }

        for (int i = 0; i < SaveInSecond - 1; i++)
        {
            _futureDirection[i] = Quaternion.Inverse(currentRot) * (_future[i + 1] - _future[i]) ;
        }

        {
            var increase = 1f / 30 * SaveInSecond;//Second / SaveInSecond * i;
            var gap_increase = Quaternion.ToEulerAngles(rotation) * increase;
            var angle_increase = Quaternion.EulerRotation(gap_increase);
            var gap = (initVel * increase* someThing);
            var futureP = (currentPos + angle_increase * currentRot * gap);
            _futureDirection[SaveInSecond - 1] = Quaternion.Inverse(currentRot) * (futureP - _future[SaveInSecond - 1]);
        }
       
    }

    private void InitializeTrajectory()
    {
        while (_history.Count < SaveInSecond)
        {
            _history.Enqueue(transform.localPosition);
            _historyDirection.Enqueue(Vector3.zero);
            _future.Add(transform.localPosition);
            _futureDirection.Add(Vector3.zero);
        }
        _lastVel = Vector3.zero;
    }
}
