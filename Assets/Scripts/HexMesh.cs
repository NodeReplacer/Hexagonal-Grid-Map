using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {
    //Makes the hexagon mesh here.
    
	Mesh hexMesh;
	MeshCollider meshCollider; //For the ray cast by our mouse clicking to hit. Probably other stuff too.
    List<Vector3> vertices;
	List<int> triangles;
    
    List<Color> colors;
    
	void Awake () {
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
		meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
		vertices = new List<Vector3>();
        colors = new List<Color>();
		triangles = new List<int>();
	}
    
    public void Triangulate (HexCell[] cells) {
		//Triangulate can be invoked at any time, even when the mesh is drawn, so let's clear old data before doing anything else.
        hexMesh.Clear();
		vertices.Clear();
        colors.Clear();
		triangles.Clear();
        //Then loop through each cell figuring out ther vertices and triangles individually.
		for (int i = 0; i < cells.Length; i++) {
			Triangulate(cells[i]);
		}
		hexMesh.vertices = vertices.ToArray();
        hexMesh.colors = colors.ToArray();
		hexMesh.triangles = triangles.ToArray();
		hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
	}
    
    void Triangulate (HexCell cell) {
        //Add each triangle
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
			Triangulate(d, cell);
		}
    }
    
	void Triangulate(HexDirection direction, HexCell cell) {
		//Get the center of the currently considered cell
        Vector3 center = cell.transform.localPosition;
		Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
		Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);
		
		AddTriangle(center, v1, v2);
		AddTriangleColor(cell.color);
		
		if (direction <= HexDirection.SE) {
			TriangulateConnection(direction, cell, v1, v2);
		}
	}
	
	//We use a bridge to blend the colors. We need: The direction of the cell we are blending, the current cell, 
	//and two vectors (typically the SolidCorner of the current cell) 
	void TriangulateConnection (HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2) {
		
		HexCell neighbor = cell.GetNeighbor(direction); //Find the neighbor in the direction passed to us.
		if (neighbor == null) {
			return;
		}
		
		Vector3 bridge = HexMetrics.GetBridge(direction); //Get the bridge that will stretch between this cell and the
		//neighbor cell.
		Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;

		AddQuad(v1, v2, v3, v4);
		AddQuadColor(cell.color, neighbor.color);
		
		//The above leaves triangle holes, so we have to plug them.
		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= HexDirection.E && nextNeighbor != null) {
			//The third vertex is the hard one to find but each edge of the triangle connects to a bridge, so it was trial and error
			//until it was discovered that the bridge we needed came from the direction of the "Next" neighbouring hex.
			//If I need a reminder: The "next" hex is pointed to by literally taking the direction enumerator and adding 1 which
			//is the next edge in the clockwise direction.
			//
			//Three cells share one triangular so we need only two connections to get all the triangles.
			//So we can keep the North and Northeast connection and not bother calculating for more than that.
			AddTriangle(v2, v4, v2 + HexMetrics.GetBridge(direction.Next()));
			AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
		}
	}
	
	//Add a color to a triangle for each vertex. With all three vertices having the same color it will be a solid fill.
	//This means that we will be reserving this for the inside triangle coloring.
    void AddTriangleColor (Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}
	//This is a triangle color function that blends the colors instead.
	void AddTriangleColor (Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}
    
    void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}
	
	//The four vertices of AddQuad should outline a trapezoid that encompasses the bottom (wider) part of the triangle.
	void AddQuad (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		vertices.Add(v4);
		//Add the indexes for the vertices to the triangles list. 
		//The addition of a whole number to each vertex index is what arranges the triangle correctly in the eyes of the computer.
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
	}

	void AddQuadColor (Color c1, Color c2, Color c3, Color c4) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
		colors.Add(c4);
	}
	
	//A quad colouring that takes two colors.
	void AddQuadColor (Color c1, Color c2) {
		colors.Add(c1);
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c2);
	}
}