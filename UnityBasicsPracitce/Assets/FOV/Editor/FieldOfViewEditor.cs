using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
	FieldOfView fov;

	void OnEnable()
	{
		fov = target as FieldOfView;
	}

	void OnSceneGUI()
	{
		Handles.color = Color.cyan;
		Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.right, 360f, fov.viewRadius);

		Handles.color = Color.white;
		Handles.DrawLine(fov.transform.position, fov.transform.position + fov.DirectionFromAngle(-fov.viewAngle / 2f, false) * fov.viewRadius);
		Handles.DrawLine(fov.transform.position, fov.transform.position + fov.DirectionFromAngle(fov.viewAngle / 2f, false) * fov.viewRadius);

		Handles.color = Color.red;
		if(fov.targetsInView != null)
		{
			foreach(Transform t in fov.targetsInView)
			{
				Handles.DrawLine(fov.transform.position, t.position);
			}
		}
	}
}
