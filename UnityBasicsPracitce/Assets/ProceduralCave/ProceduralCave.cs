using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCave : MonoBehaviour 
{
	[Header("Map")]
	public int width;
	public int height;
	public int outerWallThickness = 5;

	[Header("Randomize")]
	public bool autoRandomSeed;
	public string seed;
	[Range(0, 100)]public int fillPercentage;

	[Header("Region")]
	[Tooltip("Any region that has tile less than this number will be removed - turn 0 to 1 or 1 to 0.")]
	public int smallRegionThreshold = 10;

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
		SmoothRegion();

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
				if(IsInMapBound(x, y))	//check if this is a valid tile index, handle argument out of bound error
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

	bool IsInMapBound(int x, int y)
	{
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	//Flood-Fill algorithm, think it as paint bucket tool
	//Ingredients: 1.target tile 2.result list 3.checking queque 4.checked list
	List<Coord> FloodFillGetRegion(Coord startCoord)
	{
		int targetTile = cave[startCoord.x, startCoord.y];	//In case of paint tool bucket, this maybe pixel color, in here it is either wall(1) or pass(0)
		List<Coord> region = new List<Coord>();	//result list
		Queue<Coord> openList = new Queue<Coord>();
		bool[,] mapCheckedMarks = new bool[width, height];	//mark which coordinate has been checked. Checked means it is already in openlist

		//start
		openList.Enqueue(startCoord);
		mapCheckedMarks[startCoord.x, startCoord.y] = true;
		//loop
		while(openList.Count > 0)
		{
			Coord checkCoord = openList.Dequeue();
			region.Add(checkCoord);

			for(int x = checkCoord.x - 1; x < checkCoord.x + 2; x++)
			{
				for(int y = checkCoord.y - 1; y < checkCoord.y + 2; y++)
				{
					if(!IsInMapBound(x, y))
						continue;

					if(x != checkCoord.x && y != checkCoord.y)	//dont check diagonal coord
						continue;

					if(x == checkCoord.x && y == checkCoord.y)	//dont check itself
						continue;

					if(mapCheckedMarks[x,y] == true)
						continue;

					if(cave[x, y] != targetTile)
						continue;

					Coord newCoord = new Coord(x, y);
					openList.Enqueue(newCoord);
					mapCheckedMarks[newCoord.x, newCoord.y] = true;
				}
			}
		}

		return region;
	}

	///Use flood fill get region to find all regions of the map, a region is a list of tiles, all regions is a list of region tile list.
	List<List<Coord>> FindAllRegions()
	{
		List<List<Coord>> regions = new List<List<Coord>>();
		bool[,] checkedCoord = new bool[width, height];

		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				Coord targetCoord = new Coord(x, y);

				if(checkedCoord[targetCoord.x, targetCoord.y] == false)	//new region found
				{
					List<Coord> newRegion = FloodFillGetRegion(targetCoord);
					for(int i = 0; i < newRegion.Count; i++)	//mark all coord in this region as checked
					{
						checkedCoord[newRegion[i].x, newRegion[i].y] = true;
					}
					regions.Add(newRegion);
				}
			}
		}

		return regions;
	}

	///Find the 'small' wll regions and remove them from map
	void SmoothRegion()
	{
		List<List<Coord>> regions = FindAllRegions();

		for(int i = 0; i < regions.Count; i++)
		{
			List<Coord> checkRegion = regions[i];

			if(checkRegion.Count < smallRegionThreshold)	//remove such small region
			{
				//decide type
				int regionType = cave[checkRegion[0].x, checkRegion[0].y];
				int targetType = -1;
				if(regionType == 1)
				{
					targetType = 0;
				}else if(regionType == 0)
				{
					targetType = 1;
				}else{
					Debug.LogError("Unexpected region tile type.");
				}

				//change
				for(int c = 0; c < checkRegion.Count; c++)
				{
					cave[checkRegion[c].x, checkRegion[c].y] = targetType;
				}
			}
		}
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

public struct Coord
{
	public int x;
	public int y;

	public Coord(int _x, int _y)
	{
		x = _x;
		y = _y;
	}
}
