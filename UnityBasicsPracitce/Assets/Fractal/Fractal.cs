using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour 
{
	public Mesh mesh;
	public Material mat;

	public int maxDepth;
	[SerializeField]private int depth;
	public float childScaler = 0.5f;

	public enum GenerationType {Instantly, Gradually}
	public GenerationType generationType;

	void Start()
	{
		this.gameObject.AddComponent<MeshFilter>().mesh = mesh;
		this.gameObject.AddComponent<MeshRenderer>().material = mat;

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
		yield return new WaitForSeconds(0.5f);
		CreateSingleChild(Vector3.up);

		yield return new WaitForSeconds(0.5f);
		CreateSingleChild(Vector3.right);
	}

	void CreateAllChildInstantly()
	{
		CreateSingleChild(Vector3.up);
		CreateSingleChild(Vector3.right);
		CreateSingleChild(-Vector3.right);
		CreateSingleChild(-Vector3.up);
	}

	void CreateSingleChild(Vector3 dir)
	{
		GameObject next = new GameObject("Child Node");
		next.AddComponent<Fractal>().Init(this, dir);	//For new component, exexution order is : Awake - OnEnable - Init - Start
	}

	void Init(Fractal parent, Vector3 dir)
	{
		this.mesh = parent.mesh;
		this.mat = parent.mat;
		this.maxDepth = parent.maxDepth;
		this.depth = parent.depth + 1;
		this.generationType = parent.generationType;

		this.transform.SetParent(parent.transform);

		this.childScaler = parent.childScaler;
		this.transform.localScale = Vector3.one * childScaler;
		this.transform.localPosition = dir * (0.5f + childScaler / 2f);	//note the 0.5f
	}
}
