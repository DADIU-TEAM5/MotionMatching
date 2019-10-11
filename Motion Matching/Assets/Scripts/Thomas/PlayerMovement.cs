using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float Speed = 5f;
    public float GroundDistance = 0.2f;
    public bool showPred = true;
    public bool showHist = true;

    private bool CoolDownOver;
    private bool Once;
    private int maxCountPred;
    private int maxCountHist;

    public float numberOfFramesInTrajectory = 3;
    public float trajectoryLimitInSeconds = 1;

    public GameObject dotPred;
    public GameObject dotHist;
    private List<GameObject> dotInstPred = new List<GameObject>();
    private List<GameObject> dotInstHist = new List<GameObject>();
    public Vector3VariableList predictedPosition;
    public Vector3VariableList currentPosition;
    public Vector3VariableList historyPosition;

    public float offsetY = -0.01f;

    private Rigidbody _body;
    private Vector3 _inputs;
    private bool _isGrounded;
    private Transform _groundChecker;
    
    // Start is called before the first frame update
    void Start()
    {
        _body = GetComponent<Rigidbody>();
        //_groundChecker = transform.GetChild(0);

        predictedPosition.List.Clear();
        currentPosition.List.Clear();
        historyPosition.List.Clear();

        numberOfFramesInTrajectory += 1;

        currentPosition.List.Add(transform.position);

        StartCoroutine(KillPrediction());

    }

    private void Update()
    {
        //_isGrounded = Physics.CheckSphere(_groundChecker.position, GroundDistance, 8, QueryTriggerInteraction.Ignore);
        CreateInstPred();

        _inputs = Vector3.zero;
        _inputs.x = Input.GetAxis("Horizontal");
        _inputs.z = Input.GetAxis("Vertical");
        if (_inputs != Vector3.zero)
            transform.forward = _inputs;
 
        currentPosition.List[0] = transform.position;

    }


    private void CreateInstPred()
    {
        if (CoolDownOver == true && Once == true && showPred)
        {
            dotInstPred.Add(Instantiate(dotPred, transform.position + (transform.forward * Speed) - (transform.up*(transform.position.y+offsetY)), Quaternion.identity));

            predictedPosition.List.Add(dotInstPred[maxCountHist].transform.localPosition);

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
            /*if (dotInstHist.Count == 0)
            {
                dotInstHist.Add(Instantiate(dotHist, transform.position - (transform.up * transform.position.y), Quaternion.identity, transform));
                histPos[maxCountHist] = new Vector3();
                histPos[maxCountHist] = dotInstHist[maxCountHist].transform.position;
                Debug.Log(histPos[maxCountHist]);
            } else
            {
                dotInstHist.Add(Instantiate(dotHist, transform.position - (transform.up * transform.position.y), Quaternion.identity, transform));
                histPos[maxCountHist] = dotInstHist[maxCountHist].transform.position;

                dotInstHist[maxCountHist - 1].transform.position = histPos[maxCountHist - 1];
            }*/

            dotInstHist.Add(Instantiate(dotHist, transform.position - (transform.up * (transform.position.y+offsetY)), Quaternion.identity));

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


    void FixedUpdate()
    {
        _body.MovePosition(_body.position + _inputs * Speed * Time.fixedDeltaTime);
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
