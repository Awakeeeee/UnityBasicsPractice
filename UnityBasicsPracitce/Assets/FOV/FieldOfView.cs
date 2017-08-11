using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Three condition of sight:
//1. within view radius
//2. in range of view angle
//3. not blocked by obstacle
public class FieldOfView : MonoBehaviour 
{
	[Header("Implementation")]
	public float viewRadius;
	public float viewAngle;
	public LayerMask blockMask;
	public LayerMask enemyMask;

	[Header("Visulization")]
	public float meshResolution;
	public float blockCutOut = 0.1f;
	public MeshFilter viewMeshFilter;
	private Mesh viewMesh;

	[HideInInspector]public List<Transform> targetsInView = new List<Transform>();

	void Start()
	{
		viewMesh = new Mesh();
		viewMesh.name = "ViewMesh";
		viewMeshFilter.mesh = viewMesh;
		InvokeRepeating("See", 0.0f, 0.2f);
	}

	void LateUpdate()
	{
		DrawFOV();
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

	//cast rays to cover the view range, and use raycast info to draw a almost-fan-shepe-mesh
	void DrawFOV()
	{
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float anglePerStep = viewAngle / stepCount;
		List<Vector3> meshPoints = new List<Vector3>();

		//cast ray to get points
		for(int i = 0; i < stepCount + 1; i++)
		{
			float angle = -viewAngle / 2f + i * anglePerStep;
			Vector3 dir = DirectionFromAngle(angle, false);
			//Debug.DrawLine(transform.position, transform.position + dir * viewRadius, Color.white);

			ViewCastInfo info = new ViewCastInfo();
			info = ViewRaycast(angle);
			meshPoints.Add(transform.InverseTransformPoint(info.endPoint + dir * blockCutOut));
		}

		//use points to draw mesh
		Vector3[] vertices = new Vector3[meshPoints.Count + 1];
		int[] triangles = new int[(vertices.Length - 2) * 3];
		vertices[0] = Vector3.zero; 	//local position

		for(int i = 0; i < vertices.Length - 1; i++)
		{
			vertices[i + 1] = meshPoints[i];

			if(i < vertices.Length - 2)
			{
				triangles[i * 3] = 0;
				triangles[i * 3 + 1] = i + 1;
				triangles[i * 3 + 2] = i + 2;	
			}
		}

		viewMesh.Clear();
		viewMesh.vertices = vertices;
		viewMesh.triangles = triangles;
		viewMesh.RecalculateNormals();
	}

	ViewCastInfo ViewRaycast(float angleInsideViewFan)
	{
		Vector3 dir = DirectionFromAngle(angleInsideViewFan, false);
		RaycastHit hit;
		if(Physics.Raycast(transform.position, dir, out hit, viewRadius, blockMask, QueryTriggerInteraction.Ignore))
		{
			return new ViewCastInfo(true, hit.point, hit.distance, angleInsideViewFan);
		}else
		{
			return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, angleInsideViewFan);
		}
	}

	struct ViewCastInfo
	{
		public bool blocked;
		public Vector3 endPoint;
		public float distance;
		public float angleInsideViewAngle;

		public ViewCastInfo(bool isBlocked, Vector3 theEndPoint, float rayLength, float innerAngle)
		{
			blocked = isBlocked;
			endPoint = theEndPoint;
			distance = rayLength;
			angleInsideViewAngle = innerAngle;
		}
	}
}
