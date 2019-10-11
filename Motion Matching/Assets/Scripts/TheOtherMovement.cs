using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheOtherMovement : MonoBehaviour
{
    List<Vector3> previousPositions;
    List<Vector3> futurePositions;

    // Start is called before the first frame update
    void Start()
    {
        previousPositions = new List<Vector3>();
        futurePositions = new List<Vector3>();


    }

    // Update is called once per frame
    void Update()
    {

        previousPositions.Add(transform.position);

        if (previousPositions.Count > 10)
            previousPositions.RemoveAt(0);


    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < previousPositions.Count; i++)
        {
            Gizmos.DrawSphere(previousPositions[i], 0.01f);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < futurePositions.Count; i++)
        {
            Gizmos.DrawSphere(futurePositions[i], 0.01f);
        }

    }
}
