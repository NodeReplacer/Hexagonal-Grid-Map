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
        //Get the center of the currently considered cell
        Vector3 center = cell.transform.localPosition;
        //Add each triangle
		for (int i = 0; i < 6; i++) {
			AddTriangle(
				center,
				center + HexMetrics.corners[i],
				center + HexMetrics.corners[i + 1]
			);
            AddTriangleColor(cell.color);
		}
    }
    
    void AddTriangleColor (Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
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
}