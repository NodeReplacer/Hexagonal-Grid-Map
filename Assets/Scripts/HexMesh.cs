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
	//and two vectors (the SolidCorner of the current cell)
	void TriangulateConnection (HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2) {
		
		HexCell neighbor = cell.GetNeighbor(direction); //Find the neighbor in the direction passed to us.
		if (neighbor == null) {
			return;
		}
		
		Vector3 bridge = HexMetrics.GetBridge(direction); //Get the bridge that will stretch between this cell and the
		//neighbor cell.
		Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;
		v3.y = v4.y = neighbor.Elevation * HexMetrics.elevationStep; //We take our neighbour's elevation
		//(keeping in mind that it is a unit that needs to be multiplied by each step) and attach the end of our bridge to
		//the neighbouring cell.
		
		//Whether we are actually creating an edge terrace.
		if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
			TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
		}
		else {
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(cell.color, neighbor.color);
		}
		
		//The above leaves triangle holes, so we have to plug them.
		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= HexDirection.E && nextNeighbor != null) {
			//The third vertex is the hard one to find but each edge of the triangle connects to a bridge, so it was trial and error
			//until it was discovered that the bridge we needed came from the direction of the "Next" neighbouring hex.
			//If I need a reminder: The "next" hex is pointed to by literally taking the direction enumerator and adding 1 which
			//is the next edge in the clockwise direction.
			//
			//Three cells share one triangle so we need only two connections to get all the triangles.
			//So we can keep the North and Northeast connection and not bother calculating for more than that.
			//
			//v5 is now part of the triangle too. It needs to be modified to account for our elevation otherwise we will find triangular
			//holes for each elevation difference.
			//With no change in elevation then we are basically just sending v2 to AddTriangle in place of v5.
			Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
			v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;
			
			//With TriangulateCorner's creation we need to figure out which cell is the lowest. We only have 3 cells max.
			if (cell.Elevation <= neighbor.Elevation) {
				//if this check fails then the next neighbour is the lowest cell (we only have 2 neighbors).
				if (cell.Elevation <= nextNeighbor.Elevation) {
					TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
				}
				else {
					//We have to rotate the triangle counterclockwise to keep it correctly oriented.
					TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
				}
			}
			//if the first check fails then it's a contest between the other two cells.
			else if (neighbor.Elevation <= nextNeighbor.Elevation) {
				//if the edge neighbour is lowest we rotate clockwise
				TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
			}
			else {
				//otherwise we rotate counterclockwise.
				TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
			}
			
			/*
			AddTriangle(v2, v4, v5);
			AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
			*/
		}
	}
	
	//Triangulating this stuff is becoming more and more complex so we this function is made to simplify.
	void TriangulateEdgeTerraces (
	Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
	Vector3 endLeft, Vector3 endRight, HexCell endCell
	) {
		Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
		Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1); //Remember we have two TerraceLerps. One does color, the other
		//does the vectors.
		
		//The first step of the Terrace is established and colored here. We're going to for loop the steps in between but leave out
		//the last step.
		AddQuad(beginLeft, beginRight, v3, v4);
		AddQuadColor(beginCell.color, c2);
		
		//In each intermediate step, the previous last two vertices become the new first two.
		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c2;
			v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
			v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
			c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2);
		}
		
		//The last step of the terrace.
		AddQuad(v3, v4, endLeft, endRight);
		AddQuadColor(c2, endCell.color);
	}
	
	//With the new edge types there are a lot of possible variations on the corners.
	//Let's order the connections starting with the cell that has the lowest elevation
	void TriangulateCorner (
		Vector3 bottom, HexCell bottomCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
		HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);
		
		//In the corner between three cells there lies a triangle. This triangle must have the qualities
		//of the three cells whose edges that this corner touches.
		//Given two edge types our third edge type is derived from the other two.
		//
		//Yes, as you suspect, this does not solve for cliff cases unless we call a function explicitly
		//to handle that
		if (leftEdgeType == HexEdgeType.Slope) {
			if (rightEdgeType == HexEdgeType.Slope) {
				//This arrangement is two slopes and a flat.
				//The direction matters so it should be said that the flat goes directly left and right between two cells
				//While the slopes complete the triangle. For the left cell the slope is on the bottom right
				//for the right cell, the slope is on the bottome left.
				TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
			else if (rightEdgeType == HexEdgeType.Flat) {
				TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
			}
			else {
				TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (rightEdgeType == HexEdgeType.Slope) {
			//In this case there are two slopes and one flat.
			if (leftEdgeType == HexEdgeType.Flat) {
				TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else {
				TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		//This handles double cliffs.
		else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
			if (leftCell.Elevation < rightCell.Elevation) {
				TriangulateCornerCliffTerraces(
					right, rightCell, bottom, bottomCell, left, leftCell
				);
			}
			else {
				TriangulateCornerTerracesCliff(
					left, leftCell, right, rightCell, bottom, bottomCell
				);
			}
		}
		else {
			AddTriangle(bottom, left, right);
			AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
		}
	}
	
	void TriangulateCornerTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		//Like in TriangulateEdgeTerraces, we start with the first step.
		Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
		Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
		Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

		AddTriangle(begin, v3, v4);
		AddTriangleColor(beginCell.color, c3, c4);
		
		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetrics.TerraceLerp(begin, left, i);
			v4 = HexMetrics.TerraceLerp(begin, right, i);
			c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
			c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2, c3, c4);
		}
		
		//Last step
		AddQuad(v3, v4, left, right);
		AddQuadColor(c3, c4, leftCell.color, rightCell.color);
	}
	
	void TriangulateCornerTerracesCliff (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		//This is a bit difficult to describe, but if a terrace and cliff edge meet then the corner triangle needs
		//to account for that. But we don't want them to meet in a single corner because that will not look great.
		//So what if we collapse the steps in the terrace to some boundary point on the edge of the triangle instead of
		//a corner?
		//
		//Let's place that boundary point one elevation level above the bottom cell.
		//This is found by interpolating based on the difference in elevation.
		float b = 1f / (rightCell.Elevation - beginCell.Elevation);
		//With double cliffs we triangulate from top to bottom which creates weird triangulation from our boundary
		//interpolators becoming negative. Let's ensure they're always positive.
		if (b < 0) {
			b = -b;
		}
		
		Vector3 boundary = Vector3.Lerp(begin, right, b);
		Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);
		
		//This creates the bottom series of terrace steps. Our top triangle is a slope.
		TriangulateBoundaryTriangle(
			begin, beginCell, left, leftCell, boundary, boundaryColor
		);
		
		//The slope is being established here.
		if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(
				left, leftCell, right, rightCell, boundary, boundaryColor
			);
		}
		else {
			AddTriangle(left, right, boundary);
			AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
		}
	}
	
	void TriangulateBoundaryTriangle (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 boundary, Color boundaryColor
	) {
		Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
		Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
		
		//First triangle step of the terrace
		AddTriangle(begin, v2, boundary);
		AddTriangleColor(beginCell.color, c2, boundaryColor);
		
		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v2;
			Color c1 = c2;
			v2 = HexMetrics.TerraceLerp(begin, left, i);
			c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
			AddTriangle(v1, v2, boundary);
			AddTriangleColor(c1, c2, boundaryColor);
		}
		
		//Last step, but this time it is a triangle.
		AddTriangle(v2, left, boundary);
		AddTriangleColor(c2, leftCell.color, boundaryColor);
	}
	
	//This is the mirror case with the cliff on the left
	void TriangulateCornerCliffTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		float b = 1f / (leftCell.Elevation - beginCell.Elevation);
		//With double cliffs we triangulate from top to bottom which creates weird triangulation from our boundary
		//interpolators becoming negative. Let's ensure they're always positive.
		if (b < 0) {
			b = -b;
		}
		
		Vector3 boundary = Vector3.Lerp(begin, left, b);
		Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);

		TriangulateBoundaryTriangle(
			right, rightCell, begin, beginCell, boundary, boundaryColor
		);

		if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(
				left, leftCell, right, rightCell, boundary, boundaryColor
			);
		}
		else {
			AddTriangle(left, right, boundary);
			AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
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