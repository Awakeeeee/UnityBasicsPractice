using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
	[Header("Resourses")]
	public Mesh[] meshes;
	public MaterialHolder mats;
	public enum GenerationType {Instantly, Gradually}
	public GenerationType generationType;

	[Header("Depth")]
	public int maxDepth;
	[SerializeField]private int depth;

	[Header("Shape")]
	public float childScaler = 0.5f;
	[Range(0f, 1f)]public float perfectRate = 1.0f;
	public float maxTwistAngle = 30f;

	[Header("Rotate")]
	public bool useRotation = false;
	public float maxRotateSpeed = 90f;
	[SerializeField]private float rotateSpeed;

	//for a parent cube, how many face direct out of all six directions has a child 
	private static ChildGroup[] childGroups = {
		new ChildGroup(Vector3.up, Quaternion.identity),
		new ChildGroup(Vector3.right, Quaternion.Euler(0f, 0f, -90f)),
		new ChildGroup(Vector3.left, Quaternion.Euler(0f, 0f, 90f)),
		new ChildGroup(Vector3.forward, Quaternion.Euler(90f, 0f, 0f)),
		new ChildGroup(Vector3.back, Quaternion.Euler(-90, 0f, 0f))
	};
		
	void Start()
	{
		this.gameObject.AddComponent<MeshFilter>().mesh = meshes[Random.Range(0, meshes.Length)];
		this.gameObject.AddComponent<MeshRenderer>().material = mats.GetColoredMat();

		if(depth < maxDepth)
		{
			switch(generationType)
			{
			case GenerationType.Instantly:
				CreateAllChildInstantly();
				break;
			case GenerationType.Gradually:
				StartCoroutine(CreateAllChildGradually());
				break;
			default:
				break;
			}
		}
	}

	IEnumerator CreateAllChildGradually()
	{
		for(int i = 0; i < childGroups.Length; i++)
		{
			yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
			CreateSingleChild(i);
		}
	}

	void CreateAllChildInstantly()
	{
		for(int i = 0; i < childGroups.Length; i++)
		{
			CreateSingleChild(i);
		}
	}

	void CreateSingleChild(int index)
	{
		if(Random.Range(0f, 1.0f) > perfectRate)
			return;

		GameObject next = new GameObject("Child Node");
		next.AddComponent<Fractal>().Init(this, index);	//For new component, exexution order is : Awake - OnEnable - Init - Start
	}

	void Init(Fractal parent, int index)
	{
		this.meshes = parent.meshes;
		this.mats = parent.mats;
		this.maxDepth = parent.maxDepth;
		this.perfectRate = parent.perfectRate;
		this.maxRotateSpeed = parent.maxRotateSpeed;
		this.maxTwistAngle = parent.maxTwistAngle;
		this.useRotation = parent.useRotation;
		this.depth = parent.depth + 1;
		this.generationType = parent.generationType;

		this.transform.SetParent(parent.transform);

		this.childScaler = parent.childScaler;
		this.transform.localScale = Vector3.one * childScaler;
		this.transform.localPosition = childGroups[index].direction * (0.5f + childScaler / 2f);	//note the 0.5f
		this.transform.localRotation = childGroups[index].orientation;

		this.rotateSpeed = Random.Range(0f, maxRotateSpeed);
		this.transform.Rotate(Random.Range(-maxTwistAngle, maxTwistAngle), 0f, 0f);
	}

	void Update()
	{
		if(!useRotation)
			return;
		
		transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);	//this is NOT changing ratation Y value, BUT roate around Y axis
	}
}

public class ChildGroup
{
	public Vector3 direction;
	public Quaternion orientation;

	public ChildGroup(Vector3 d, Quaternion o)
	{
		direction = d;
		orientation = o;
	}
}
