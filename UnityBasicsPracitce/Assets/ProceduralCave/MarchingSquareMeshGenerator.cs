using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquareMeshGenerator : MonoBehaviour 
{
	public float wallHeight;
	public MeshFilter wallMeshFilter;

	SquareGrid squareGrid;

	List<Vector3> vertices;
	List<int> triangles;
	MeshFilter meshFilter;
	Mesh marchingSqaureMesh;

	//During mesh construction, this infomation is needed: which triangles does this vertex belongs to?
	//Build up this dictionary to hold such information, the info is used to determine wall edge
	//That is: given 2 vertices, if there is, and there is Only 1 triangle shared betwwen the 2 vertices, these 2 vertices line an edge
	Dictionary<int, List<Triangle>> vertexOfTriangleDic;
	//An outline is the whole bound, a loop-over-circle of a continent, which is described as a List<int>
	List<List<int>> outlines;
	//A checked vertice means this vertex has been added into an outline
	HashSet<int> checkedVertices;

	void Awake()
	{
		meshFilter = GetComponent<MeshFilter>();
	}

	//--------------------------Display method 3. By Mesh.------------------------------
	public void GenerateMesh(int[,] mapData, float size)
	{
		squareGrid = new SquareGrid(mapData, size);
		vertices = new List<Vector3>();
		triangles = new List<int>();
		vertexOfTriangleDic = new Dictionary<int, List<Triangle>>();
		outlines = new List<List<int>>();
		checkedVertices = new HashSet<int>();

		int lx = squareGrid.sGrid.GetLength(0);
		int ly = squareGrid.sGrid.GetLength(1);

		for(int x = 0; x < lx; x++)
		{
			for(int y = 0; y < ly; y++)
			{
				ConfigureTriangles(squareGrid.sGrid[x,y]);
			}
		}

		SetupMesh();
		SetupWallMesh();
	}

	///Apply the constructed vertices and triangles to form a mesh
	void SetupMesh()
	{
		marchingSqaureMesh = new Mesh();
		marchingSqaureMesh.name = "MarchingSquareMesh";
		marchingSqaureMesh.vertices = vertices.ToArray();
		marchingSqaureMesh.triangles = triangles.ToArray();
		marchingSqaureMesh.RecalculateNormals();

		meshFilter.mesh = marchingSqaureMesh;
	}

	void SetupWallMesh()
	{
		BuildOutlines();

		Mesh wallMesh;
		wallMesh = new Mesh();
		wallMesh.name = "Wall Mesh";

		List<Vector3> wallVertices = new List<Vector3>();
		List<int> wallTriangles = new List<int>();

		for(int o = 0; o < outlines.Count; o++)
		{
			List<int> outline = outlines[o];
			for(int i = 0; i < outline.Count - 1; i++)
			{
				int triangleStartIndex = wallVertices.Count;
				//add for vertices, start from the bottom left one
				wallVertices.Add(vertices[outline[i]]);	//+0
				wallVertices.Add(vertices[outline[i + 1]]);	//+1
				wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight);	//+2 toward lower, so that the cave mesh is the wall cap
				wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight);	//+3
				//create a quad, triangles in anti-clockwise order
				wallTriangles.Add(triangleStartIndex + 0);
				wallTriangles.Add(triangleStartIndex + 2);
				wallTriangles.Add(triangleStartIndex + 3);

				wallTriangles.Add(triangleStartIndex + 0);
				wallTriangles.Add(triangleStartIndex + 3);
				wallTriangles.Add(triangleStartIndex + 1);
			}
		}

		wallMesh.vertices = wallVertices.ToArray();
		wallMesh.triangles = wallTriangles.ToArray();
		wallMesh.RecalculateNormals();
		wallMeshFilter.mesh = wallMesh;
	}

	///For each square, determine which configuration case it is out of the 16 kinds
	void ConfigureTriangles(Square s)
	{
		switch(s.configuration)
		{
		case 0:
			break;
		case 1:
			ConnectMesh(s.cn_bottomLeft, s.n_midLeft, s.n_midBottom);	//TODO case 1,2,4,8: to show [inner outline triangles] correctly, these outline vertex needs to be in order, outline vertex order is initially defined in here
			//I think, when find next outline vertex, function loop through triangle vertex, and return the first one, so this order here decides the loop order
			break;
		case 2:
			ConnectMesh(s.cn_bottomRight, s.n_midBottom, s.n_midRight);
			break;
		case 3:
			ConnectMesh(s.n_midRight, s.cn_bottomRight, s.cn_bottomLeft, s.n_midLeft);
			break;
		case 4:
			ConnectMesh(s.cn_topRight, s.n_midRight, s.n_midTop);
			break;
		case 5:
			ConnectMesh(s.n_midTop, s.cn_topRight, s.n_midRight, s.n_midBottom, s.cn_bottomLeft, s.n_midLeft);
			break;
		case 6:
			ConnectMesh(s.n_midTop, s.cn_topRight, s.cn_bottomRight, s.n_midBottom);
			break;
		case 7:
			ConnectMesh(s.n_midTop, s.cn_topRight, s.cn_bottomRight, s.cn_bottomLeft, s.n_midLeft);
			break;
		case 8:
			ConnectMesh(s.cn_topLeft, s.n_midTop, s.n_midLeft);
			break;
		case 9:
			ConnectMesh(s.cn_topLeft, s.n_midTop, s.n_midBottom, s.cn_bottomLeft);
			break;
		case 10:
			ConnectMesh(s.cn_topLeft, s.n_midTop, s.n_midRight, s.cn_bottomRight, s.n_midBottom, s.n_midLeft);
			break;
		case 11:
			ConnectMesh(s.cn_topLeft, s.n_midTop, s.n_midRight, s.cn_bottomRight, s.cn_bottomLeft);
			break;
		case 12:
			ConnectMesh(s.cn_topLeft, s.cn_topRight, s.n_midRight, s.n_midLeft);
			break;
		case 13:
			ConnectMesh(s.cn_topLeft, s.cn_topRight, s.n_midRight, s.n_midBottom, s.cn_bottomLeft);
			break;
		case 14:
			ConnectMesh(s.cn_topLeft, s.cn_topRight, s.cn_bottomRight, s.n_midBottom, s.n_midLeft);
			break;
		case 15:
			ConnectMesh(s.cn_topLeft, s.cn_topRight, s.cn_bottomRight, s.cn_bottomLeft);
			//these vertex cannot be outline vertex, this is an optimization
			checkedVertices.Add(s.cn_topLeft.id);
			checkedVertices.Add(s.cn_topRight.id);
			checkedVertices.Add(s.cn_bottomLeft.id);
			checkedVertices.Add(s.cn_bottomRight.id);
			break;
		default:
			break;
		}
	}

	///Given the turned-on nodes of a square, then accordingly add them to mesh vertices, add triangles of them to mesh triangles 
	void ConnectMesh(params Node[] nodes)
	{
		AddVertices(nodes);

		if(nodes.Length >= 3)	//everyone is
			AddTriangle(nodes[0], nodes[1], nodes[2]);
		if(nodes.Length >= 4)	//example 6
			AddTriangle(nodes[0], nodes[2], nodes[3]);
		if(nodes.Length >= 5)	//9
			AddTriangle(nodes[0], nodes[3], nodes[4]);
		if(nodes.Length >= 6)	//10 the most complecated
			AddTriangle(nodes[0], nodes[4], nodes[5]);
	}

	void AddVertices(Node[] nodes)
	{
		for(int i = 0; i < nodes.Length; i++)
		{
			if(nodes[i].id == -1)	//no duplication
			{
				nodes[i].id = vertices.Count;
				vertices.Add(nodes[i].pos);
			}
		}
	}

	void AddTriangle(Node a, Node b, Node c)
	{
		triangles.Add(a.id);
		triangles.Add(b.id);
		triangles.Add(c.id);

		Triangle thisTriangle = new Triangle(a.id, b.id, c.id);
		StepToBuildDictionary(a.id, thisTriangle);
		StepToBuildDictionary(b.id, thisTriangle);
		StepToBuildDictionary(c.id, thisTriangle);
	}
		
	///Add a new triangle(or maybe a new vertex) to dictionary, each time call this, we go one more step to build up the vertexOfTriangleDic
	void StepToBuildDictionary(int v, Triangle t)
	{
		if(vertexOfTriangleDic.ContainsKey(v))
		{
			vertexOfTriangleDic[v].Add(t);
		}else
		{
			List<Triangle> newVertexTriangles = new List<Triangle>();
			vertexOfTriangleDic.Add(v, newVertexTriangles);
			vertexOfTriangleDic[v].Add(t);
		}
	}

	///Given 2 vertices, check if they connect an outline edge
	bool IsOutlineEdge(int v1, int v2)
	{
		int sharedTriangleCount = 0;
		List<Triangle> triangles1 = vertexOfTriangleDic[v1];

		for(int i = 0; i < triangles1.Count; i++)
		{
			if(triangles1[i].Contain(v2))
			{
				sharedTriangleCount++;	
			}

			if(sharedTriangleCount > 1)
			{
				return false;
			}
		}

		return sharedTriangleCount == 1;
	}

	///Given a vertex, loop its triangles vertices, return the first valid vertex which form an outline with given vertex, if there's none return -1
	int GetNextOutlineVertex(int givenVertex)
	{
		List<Triangle> tris = vertexOfTriangleDic[givenVertex];

		for(int t = 0; t < tris.Count; t++)
		{
			Triangle thisTri = tris[t];

			for(int v = 0; v < 3; v++)
			{
				if(thisTri[v] != givenVertex && !checkedVertices.Contains(thisTri[v]))
				{
					if(IsOutlineEdge(givenVertex, thisTri[v]))
					{
						return thisTri[v];
					}
				}
			}
		}

		return -1; //no outline from given vertex
	}

	///Recursively get next outline vertex and connect to build a complete outline
	void ConnectAnOutline(int thisVertex, int outlineIndex)
	{
		int nextVertex = GetNextOutlineVertex(thisVertex);
		if(nextVertex != -1)
		{
			checkedVertices.Add(nextVertex);
			outlines[outlineIndex].Add(nextVertex);
			ConnectAnOutline(nextVertex, outlineIndex);
		}
	}

	void BuildOutlines()
	{
		for(int i = 0; i < vertices.Count; i++)	//as specified by squareGid constructor, i is also the vertex index
		{
			if(checkedVertices.Contains(i))
				continue;

			int next = GetNextOutlineVertex(i);
			if(next != -1)	//start to build a new outline, all the outline vertices will be marked as checked
			{
				List<int> newOutline = new List<int>();
				newOutline.Add(i);
				outlines.Add(newOutline);
				checkedVertices.Add(i);
				ConnectAnOutline(i, outlines.Count - 1);

				newOutline.Add(i);	//add the very start vertex at the end, to 'seal' the outline
			}
		}
		//Debug.Log(outlines.Count);
	}

	void OnDrawGizmos()
	{
		//-----------------Display method 2. Draw the square and its nodes-------------------
//		if(squareGrid == null)
//			return;
//
//		int lx = squareGrid.sGrid.GetLength(0);
//		int ly = squareGrid.sGrid.GetLength(1);
//
//		for(int x = 0; x < lx; x++)
//		{
//			for(int y = 0; y < ly; y++)
//			{
//				//control nodes
//				Gizmos.color = squareGrid.sGrid[x,y].cn_bottomLeft.isOn ? Color.black : Color.white;
//				Gizmos.DrawCube(squareGrid.sGrid[x,y].cn_bottomLeft.pos, Vector3.one * 0.5f);
//
//				//normal nodes
//				Gizmos.color = Color.gray;
//				Gizmos.DrawCube(squareGrid.sGrid[x,y].n_midBottom.pos, Vector3.one * 0.3f);
//				Gizmos.DrawCube(squareGrid.sGrid[x,y].n_midLeft.pos, Vector3.one * 0.3f);
//
//				//edge case
//				if(x == lx - 1 || y == ly - 1)
//				{
//					Gizmos.color = squareGrid.sGrid[x,y].cn_bottomRight.isOn ? Color.black : Color.white;
//					Gizmos.DrawCube(squareGrid.sGrid[x,y].cn_bottomRight.pos, Vector3.one * 0.5f);
//
//					Gizmos.color = squareGrid.sGrid[x,y].cn_topRight.isOn ? Color.black : Color.white;
//					Gizmos.DrawCube(squareGrid.sGrid[x,y].cn_topRight.pos, Vector3.one * 0.5f);
//
//					Gizmos.color = squareGrid.sGrid[x,y].cn_topLeft.isOn ? Color.black : Color.white;
//					Gizmos.DrawCube(squareGrid.sGrid[x,y].cn_topLeft.pos, Vector3.one * 0.5f);
//
//					Gizmos.color = Color.gray;
//					Gizmos.DrawCube(squareGrid.sGrid[x,y].n_midRight.pos, Vector3.one * 0.3f);
//					Gizmos.DrawCube(squareGrid.sGrid[x,y].n_midTop.pos, Vector3.one * 0.3f);
//				}
//			}
//		}

//		if(outlines == null)
//			return;
//		Gizmos.color = Color.red;
//		for(int i = 0; i < outlines.Count; i++)
//		{
//			List<int> outline = outlines[i];
//			for(int j = 0; j < outline.Count; j++)
//			{
//				Gizmos.DrawSphere(vertices[outline[j]], 0.2f);
//			}
//		}
	}
}

public class Node
{
	public Vector3 pos;
	public int id = -1;

	public Node(Vector3 _pos)
	{
		pos = _pos;
		id = -1;
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

		//at each map data point, create a control node
		ControlNode[,] nodes = new ControlNode[lengthX, lengthY];
		for(int x = 0; x < lengthX; x++)
		{
			for(int y = 0; y < lengthY; y++)
			{
				Vector3 pos = new Vector3((-lengthX/2f + x) * size, 0f, (-lengthY/2f + y) * size);
				nodes[x,y] = new ControlNode(pos, map[x,y] == 1, size);	//the place where map data is used
			}
		}

		//at each control node, create a square(look the control node as the bottom left one)
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

public struct Triangle
{
	int vertexIndexA;
	int vertexIndexB;
	int vertexIndexC;

	int[] vertices;

	public Triangle(int a, int b, int c)
	{
		vertexIndexA = a;
		vertexIndexB = b;
		vertexIndexC = c;

		vertices = new int[3];
		vertices[0] = vertexIndexA;
		vertices[1] = vertexIndexB;
		vertices[2] = vertexIndexC;
	}

	public int this[int index]{
		get{
			return vertices[index];
		}
	}

	public bool Contain(int vertexID)
	{
		return vertexIndexA == vertexID || vertexIndexB == vertexID || vertexIndexC == vertexID;
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