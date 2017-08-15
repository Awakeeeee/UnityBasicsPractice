using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquareMeshGenerator : MonoBehaviour 
{
	SquareGrid squareGrid;

	public void GenerateMesh(int[,] mapData, float size)
	{
		squareGrid = new SquareGrid(mapData, size);

		int lx = squareGrid.sGrid.GetLength(0);
		int ly = squareGrid.sGrid.GetLength(1);

		for(int x = 0; x < lx; x++)
		{
			for(int y = 0; y < ly; y++)
			{
				ConfigureTriangles(squareGrid.sGrid[x,y]);
			}
		}
	}

	void ConfigureTriangles(Square s)
	{
		switch(s.configuration)
		{
		case 0:
			break;
		case 1:
			ConnectMeshPoint(s.n_midBottom, s.cn_bottomLeft, s.n_midLeft);
			break;
		case 2:
			ConnectMeshPoint(s.n_midRight, s.cn_bottomRight, s.n_midBottom);
			break;
		case 3:
			ConnectMeshPoint(s.n_midRight, s.cn_bottomRight, s.cn_bottomLeft, s.n_midLeft);
			break;
		case 4:
			ConnectMeshPoint(s.n_midTop, s.cn_topRight, s.n_midRight);
			break;
		case 5:
			ConnectMeshPoint(s.n_midTop, s.cn_topRight, s.n_midRight, s.n_midBottom, s.cn_bottomLeft, s.n_midLeft);
			break;
		case 6:
			ConnectMeshPoint(s.n_midTop, s.cn_topRight, s.cn_bottomRight, s.n_midBottom);
			break;
		case 7:
			ConnectMeshPoint(s.n_midTop, s.cn_topRight, s.cn_bottomRight, s.cn_bottomLeft, s.n_midLeft);
			break;
		case 8:
			ConnectMeshPoint(s.cn_topLeft, s.n_midTop, s.n_midLeft);
			break;
		case 9:
			ConnectMeshPoint(s.cn_topLeft, s.n_midTop, s.n_midBottom, s.cn_bottomLeft);
			break;
		case 10:
			ConnectMeshPoint(s.cn_topLeft, s.n_midTop, s.n_midRight, s.cn_bottomRight, s.n_midBottom, s.n_midLeft);
			break;
		case 11:
			ConnectMeshPoint(s.cn_topLeft, s.n_midTop, s.n_midRight, s.cn_bottomRight, s.cn_bottomLeft);
			break;
		case 12:
			ConnectMeshPoint(s.cn_topLeft, s.cn_topRight, s.n_midRight, s.n_midLeft);
			break;
		case 13:
			ConnectMeshPoint(s.cn_topLeft, s.cn_topRight, s.n_midRight, s.n_midBottom, s.cn_bottomLeft);
			break;
		case 14:
			ConnectMeshPoint(s.cn_topLeft, s.cn_topRight, s.cn_bottomRight, s.n_midBottom, s.n_midLeft);
			break;
		case 15:
			ConnectMeshPoint(s.cn_topLeft, s.cn_topRight, s.cn_bottomRight, s.cn_bottomLeft);
			break;
		default:
			break;
		}
	}

	void ConnectMeshPoint(params Node[] activePoints)
	{
		
	}

	void OnDrawGizmos()
	{
		//-----------------Display method 2. Draw the square and its nodes-------------------
		if(squareGrid == null)
			return;

		int lx = squareGrid.sGrid.GetLength(0);
		int ly = squareGrid.sGrid.GetLength(1);

		for(int x = 0; x < lx; x++)
		{
			for(int y = 0; y < ly; y++)
			{
				//control nodes
				Gizmos.color = squareGrid.sGrid[x,y].cn_bottomLeft.isOn ? Color.black : Color.white;
				Gizmos.DrawCube(squareGrid.sGrid[x,y].cn_bottomLeft.pos, Vector3.one * 0.5f);

				//normal nodes
				Gizmos.color = Color.gray;
				Gizmos.DrawCube(squareGrid.sGrid[x,y].n_midBottom.pos, Vector3.one * 0.3f);
				Gizmos.DrawCube(squareGrid.sGrid[x,y].n_midLeft.pos, Vector3.one * 0.3f);

				//edge case
				if(x == lx - 1 || y == ly - 1)
				{
					Gizmos.color = squareGrid.sGrid[x,y].cn_bottomRight.isOn ? Color.black : Color.white;
					Gizmos.DrawCube(squareGrid.sGrid[x,y].cn_bottomRight.pos, Vector3.one * 0.5f);

					Gizmos.color = squareGrid.sGrid[x,y].cn_topRight.isOn ? Color.black : Color.white;
					Gizmos.DrawCube(squareGrid.sGrid[x,y].cn_topRight.pos, Vector3.one * 0.5f);

					Gizmos.color = squareGrid.sGrid[x,y].cn_topLeft.isOn ? Color.black : Color.white;
					Gizmos.DrawCube(squareGrid.sGrid[x,y].cn_topLeft.pos, Vector3.one * 0.5f);

					Gizmos.color = Color.gray;
					Gizmos.DrawCube(squareGrid.sGrid[x,y].n_midRight.pos, Vector3.one * 0.3f);
					Gizmos.DrawCube(squareGrid.sGrid[x,y].n_midTop.pos, Vector3.one * 0.3f);
				}
			}
		}
	}
}

public class Node
{
	public Vector3 pos;

	public Node(Vector3 _pos)
	{
		pos = _pos;
	}
}

public class ControlNode : Node
{
	//on = 1 = wall, off = 0 = passable
	public bool isOn;
	//each control node owns two normal nodes
	public Node top;
	public Node right;
	public float size;

	public ControlNode(Vector3 _pos, bool _isOn, float _size) : base(_pos)
	{
		isOn = _isOn;
		size = _size;

		top = new Node(new Vector3(pos.x, pos.y, pos.z + size/2f));
		right = new Node(new Vector3(pos.x + size/2f, pos.y, pos.z));
	}
}

public class Square
{
	public ControlNode cn_topLeft, cn_topRight, cn_bottomRight, cn_bottomLeft;
	public Node n_midTop, n_midRight, n_midBottom, n_midLeft;
	public Vector3 centerPos;
	public float size;
	public int configuration;	//the decimal number out of binary combination. eg 1010 = 10

	public Square(float _size, ControlNode _tl, ControlNode _tr, ControlNode _br, ControlNode _bl)
	{
		size = _size;

		cn_topLeft = _tl;
		cn_topRight = _tr;
		cn_bottomLeft = _bl;
		cn_bottomRight = _br;

		n_midTop = cn_topLeft.right;
		n_midRight = cn_bottomRight.top;
		n_midBottom = cn_bottomLeft.right;
		n_midLeft = cn_bottomLeft.top;

		centerPos = new Vector3(cn_bottomLeft.pos.x + size/2f, cn_bottomLeft.pos.y, cn_bottomLeft.pos.z + size/2f);

		configuration = 0;

		if(cn_topLeft.isOn)
			configuration += 8;	//1000
		if(cn_topRight.isOn)
			configuration += 4; //0100
		if(cn_bottomRight.isOn)
			configuration += 2; //0010
		if(cn_bottomLeft.isOn)
			configuration += 1;	//0001
	}
}

public class SquareGrid
{
	public Square[,] sGrid;

	public SquareGrid(int[,] map, float size)
	{
		int lengthX = map.GetLength(0);
		int lengthY = map.GetLength(1);

		ControlNode[,] nodes = new ControlNode[lengthX, lengthY];
		for(int x = 0; x < lengthX; x++)
		{
			for(int y = 0; y < lengthY; y++)
			{
				Vector3 pos = new Vector3((-lengthX/2f + x) * size, 0f, (-lengthY/2f + y) * size);	//TODO what about Y?
				nodes[x,y] = new ControlNode(pos, map[x,y] == 1, size);
			}
		}

		sGrid = new Square[lengthX - 1, lengthY - 1];
		for(int x = 0; x < lengthX - 1; x++)
		{
			for(int y = 0; y < lengthY - 1; y++)
			{
				sGrid[x,y] = new Square(size, nodes[x, y+1], nodes[x+1, y+1], nodes[x+1, y], nodes[x, y]);
			}
		}
	}
}

//---------------------------------------------
/*
 *     A----------------e-----------------B (4 control nodes, either 0 or 1, form 16 combinations - 0000,0001,1000 etc)  
 *     |                                  | 
 *     |                                  |
 *     |                                  |
 *     |                                  | 
 *     |                                  | 
 *     |                                  | 
 *     h           SQUARE                 f (4 normal nodes, help to draw triangle, a control node owns its top&right normal node)
 *     |                                  |
 *     |                                  |
 *     |                                  |
 *     |                                  |
 *     |                                  |
 *     |                                  |
 *     |                                  |
 *     D---------------g------------------C
 * 
 */ 