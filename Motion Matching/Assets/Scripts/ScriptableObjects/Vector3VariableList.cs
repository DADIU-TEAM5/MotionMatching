using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Vector3List", menuName = "Stranger Objects/Vector3List", order = 1)]
public class Vector3VariableList : ScriptableObject
{
    public List<Vector3> List;
}
