using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialHolder : MonoBehaviour 
{
	public Material baseMat;

	private Material[] mats = new Material[7];

	void Awake()
	{
		for(int i = 0; i < mats.Length; i++)
		{
			mats[i] = new Material(baseMat);
		}

		mats[0].color = Color.red;
		mats[1].color = new Color(1f, 0.5f, 0f);
		mats[2].color = Color.yellow;
		mats[3].color = Color.green;
		mats[4].color = Color.cyan;
		mats[5].color = Color.blue;
		mats[6].color = Color.magenta;
	}

	public Material GetColoredMat()
	{
		int x = Random.Range(0, mats.Length);

		return mats[x];
	}
}
