using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float Speed = 3f;
    public float RotationSpeed = 0.25f;
    public float GroundDistance = 0.2f;
    public bool showPred = true;
    public bool showHist = true;

    private bool CoolDownOver;
    private bool Once;
    private int maxCountPred = 0;
    private int maxCountHist = 0;

    public float numberOfFramesInTrajectory = 3;
    public float trajectoryLimitInSeconds = 1;

    public GameObject dotPred;
    public GameObject dotHist;
    public GameObject dotTemp;
    private List<GameObject> dotInstPred = new List<GameObject>();
    private List<GameObject> dotInstHist = new List<GameObject>();
    public Vector3VariableList predictedPosition;
    public Vector3VariableList currentPosition;
    public Vector3VariableList historyPosition;

    public GameObject predictedTarget;
    public Vector3[] predictedPointTemp;

    public float offsetY = -0.01f;
    const float fps = 30f;
    private float targetLinearVelocity;

    public Vector3 velocity;
    public Vector3 predictedPoint;

    private Vector3 test;

    public GameObject testingTarget;

    private GameObject _body;
    private Vector3 _inputs;
    private bool _isGrounded;
    private Transform _groundChecker;

    public Vector3 _prevPosition;
    private Vector3 inputVel;

    // Start is called before the first frame update
    void Start()
    {
        //_groundChecker = transform.GetChild(0);

        predictedPosition.List.Clear();
        currentPosition.List.Clear();
        historyPosition.List.Clear();

        numberOfFramesInTrajectory += 1;

        currentPosition.List.Add(transform.position);

        StartCoroutine(KillPrediction());

    }


    void FixedUpdate()
    {

    }


    private void Update()
    {

        _inputs = Vector3.zero;
        _inputs.z = Input.GetAxis("Vertical");

        transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed);
        inputVel = _inputs * Speed * Time.deltaTime;
        transform.Translate(inputVel);

        currentPosition.List[0] = transform.position;

        test = PredictPosition();

        CreateInstPred();

        //testingTarget.transform.position = test;

    }


    private Vector3 PredictPosition()
    {
        Vector3 currentPos = currentPosition.List[0];
        Vector3 predictedTransform = currentPos;

        //float linearVelocity = _inputs.z;
        //float linearVelocityDecay;
        Vector3 linearVelocity;
        Vector3 _prevZ = currentPos;
        float prevVel = inputVel.magnitude;

        linearVelocity = transform.position - _prevZ / Time.deltaTime;
        _prevZ = transform.position;

        predictedTarget.transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed);
        predictedTarget.transform.Translate(_inputs * linearVelocity.magnitude / 200);



        float elapsedTime = 0.0f;
        const float timeHorizon = 1.0f;
        const float deltaTime = (1.0f / 30.0f);

        while (elapsedTime < timeHorizon)
        {
            predictedTransform += linearVelocity * deltaTime;

            linearVelocity *= ExponentialDecay(prevVel, inputVel.magnitude, deltaTime / (timeHorizon - elapsedTime));

            elapsedTime += Time.deltaTime;
        }


        return predictedTransform;
    }

    private float ExponentialDecay(float current, float target, float lambda)
    {
        return target + (current - target) / (1.0f + lambda + 0.5f * lambda * lambda);
    }


    private void CreateInstPred()
    {
        if (CoolDownOver && Once && showPred)
        {
            //GameObject tempDot = Instantiate(dotTemp, new Vector3(test.x, 0.01f, test.z), Quaternion.identity);

            dotInstPred.Add(Instantiate(dotPred, transform.position - (transform.up * (transform.position.y + offsetY)), transform.rotation));
            predictedPosition.List.Add(dotInstPred[maxCountPred].transform.position);

            if(dotInstPred.Count > 1)
            { 
            dotInstPred[maxCountPred - 1].transform.position = predictedPosition.List[maxCountHist - 1];
            }

            maxCountPred++;

            if (maxCountPred == numberOfFramesInTrajectory)
            {
                Destroy(dotInstPred[0]);

                dotInstPred.Remove(dotInstPred[0]);
                predictedPosition.List.Remove(predictedPosition.List[0]);

                maxCountPred--;
            }
        }

        CreateInstHist();

    }


    private void CreateInstHist()
    {
        if (CoolDownOver == true && Once == true && showHist)
        {
            dotInstHist.Add(Instantiate(dotHist, transform.position - (transform.up * (transform.position.y+offsetY)), Quaternion.FromToRotation(transform.eulerAngles,new Vector3(0,90,90))));

            historyPosition.List.Add(dotInstHist[maxCountHist].transform.localPosition);

            Once = false;
            CoolDownOver = false;
            maxCountHist++;

        }

        if (maxCountHist == numberOfFramesInTrajectory)
        {
            Destroy(dotInstHist[0]);

            dotInstHist.Remove(dotInstHist[0]);
            historyPosition.List.Remove(historyPosition.List[0]);

            maxCountHist--;
        }

    }


    IEnumerator KillPrediction()
    {
        while (true)
        {
            yield return new WaitForSeconds(trajectoryLimitInSeconds/numberOfFramesInTrajectory);
            CoolDownOver = !CoolDownOver;
            Once = true;
        }

    }

}
