using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerInput : ScriptableObject
{
    public float Velocity;
    public float Acceleration;
    public float Brake;
    public float Turn;
    public bool Crouch;
    public bool Jump;


    public Vector3 Direction;
    public float AngularVelocity;

    public float m_TurnSpeed;
    public float m_ForwardAmount;
}
