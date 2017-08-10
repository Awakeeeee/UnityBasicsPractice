using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Three condition of sight:
//1. within view radius
//2. in range of view angle
//3. not blocked by obstacle
public class FieldOfView : MonoBehaviour 
{
	public float viewRadius;
	public float viewAngle;
	public LayerMask blockMask;
	public LayerMask enemyMask;

	[HideInInspector]public List<Transform> targetsInView = new List<Transform>();

	void Start()
	{
		InvokeRepeating("See", 0.0f, 0.2f);
	}

	void See()
	{
		//1
		targetsInView.Clear();
		Collider[] potentials = Physics.OverlapSphere(transform.position, viewRadius, enemyMask, QueryTriggerInteraction.Ignore);

		foreach(Collider p in potentials)
		{
			Transform target = p.transform;
			Vector3 toTargetVec = (target.position - transform.position).normalized;
			//2
			if(Vector3.Angle(transform.forward, toTargetVec) < viewAngle / 2f)
			{
				float distance = Vector3.Distance(transform.position, target.position);
				//3
				if(!Physics.Raycast(transform.position, toTargetVec, distance, blockMask, QueryTriggerInteraction.Ignore))
				{
					targetsInView.Add(target);
				}
			}
		}
	}

	/*
	 * Coordinate axis of trigonometric in Unity is like this (while normally in math 0 degree is at the right)
	 * if is global angle, the angle starts at 0
	 * 					Z-Axis
	 * 					|0    /1
	 *                  |    / `
	 *                  |   /  ` 
	 * 					|a /   `
	 * 					| /    `
	 * ----270----------Y---------------90 X-Axis
	 * 					|     sin(a)
	 * 					|
	 * 					|
	 * 					|
	 * 					|
	 * 					180
	*/
	//Get the oblique line direction according to a given angle a
	public Vector3 DirectionFromAngle(float angleInDegree, bool isGlobalAngle)
	{
		if(!isGlobalAngle)
		{
			angleInDegree += transform.eulerAngles.y;	//add the rotation of player input
		}

		return new Vector3(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
	}
}
