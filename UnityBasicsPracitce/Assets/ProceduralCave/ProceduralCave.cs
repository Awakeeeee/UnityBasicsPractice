using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCave : MonoBehaviour 
{
	public int width;
	public int height;
	public int outerWallThickness = 5;
	public bool autoRandomSeed;
	public string seed;
	[Range(0, 100)]public int fillPercentage;

	private int[,] cave;	//save 1 and 0, 1 indicates fill/wall, 0 indicates empty/passable
	private MarchingSquareMeshGenerator msGenerator;

	void Start()
	{
		msGenerator = GetComponent<MarchingSquareMeshGenerator>();
		GenerateCave();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.J))
		{
			GenerateCave();
		}
	}

	void GenerateCave()
	{
		//initial fill
		cave = new int[width, height];
		RandomlyFillCave();

		//smooth the cave several times
		for(int i = 0; i < 5; i++)
		{
			SmoothCave();
		}

		//more thickness on outline wall
		int[,] thickCave = new int[width + outerWallThickness * 2, height + outerWallThickness * 2];
		for(int x = 0; x < thickCave.GetLength(0); x++)
		{
			for(int y = 0; y < thickCave.GetLength(1); y++)
			{
				if(x < outerWallThickness || x >= width || y < outerWallThickness || y >= height)
				{
					thickCave[x,y] = 1;
				}else{
					thickCave[x,y] = cave[x - outerWallThickness, y - outerWallThickness];
				}
			}
		}

		//draw the crazy mesh
		if(msGenerator != null)
		{
			//msGenerator.GenerateMesh(cave, 1);
			msGenerator.GenerateMesh(thickCave, 1);
		}
	}

	void RandomlyFillCave()
	{
		if(autoRandomSeed)
		{
			seed = Random.value.ToString();
		}

		System.Random ranPicker = new System.Random(seed.GetHashCode());

		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				if(x == 0 || x == width - 1 || y == 0 || y == height - 1)
				{
					cave[x,y] = 1;
				}else
				{
					cave[x, y] = ranPicker.Next(0, 100) > fillPercentage ? 0 : 1;	
				}
			}
		}
	}

	//loop throught every tile, if most of its surroundings are walls, turn itself a wall, vise versa
	//finally each passable area and walls looks 'grouped'
	void SmoothCave()
	{
		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				int wallCount = GetSurroundingWallCount(x,y);
				if(wallCount > 4)
				{
					cave[x,y] = 1;
				}else if(wallCount < 4)
				{
					cave[x,y] = 0;
				}
				//else leave as what it is, this is important for the outcome
			}
		}
	}

	int GetSurroundingWallCount(int cx, int cy)
	{
		int wallCount = 0;

		for(int x = cx - 1; x < cx + 2; x++)
		{
			for(int y = cy - 1; y < cy + 2; y++)
			{
				if(x >= 0 && x < width && y >= 0 && y < height)	//check if this is a valid tile index, handle argument out of bound error
				{
					if(x != cx || y != cy)
					{
						wallCount += cave[x, y];
					}	
				}else	//if this is an out of bound tile index, just plus one, encourage the edge to be wall
				{
					wallCount ++;
				}
			}
		}

		return wallCount;
	}

	void OnDrawGizmos()
	{
//---------Display method 1. just use map data----------------
//		if(cave == null)
//			return;
//
//		for(int x = 0; x < width; x++)
//		{
//			for(int y = 0; y < height; y++)
//			{
//				Gizmos.color = cave[x,y] == 1 ? Color.black : Color.white;
//				Vector3 tileCenter = new Vector3(-width/2f + x + 0.5f, 0f, -height/2f + y + 0.5f);	//so that the center of map is 0,0,0
//				Gizmos.DrawCube(tileCenter, Vector3.one);
//			}
//		}
	}
}
