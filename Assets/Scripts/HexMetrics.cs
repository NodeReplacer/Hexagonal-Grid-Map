using UnityEngine;

//These are measurements of each individual cell but do not include things like coordinates.
public static class HexMetrics {
    
    //Hexagons have two radii. The first is measured from a corner and the second is measured from the middle of
    //an edge. The middle of the edge radius (inner radius) is shorter.
    
	public const float outerRadius = 10f;

	public const float innerRadius = outerRadius * 0.866025404f;
    
	public const float solidFactor = 0.75f;
	
	public const float blendFactor = 1f - solidFactor;
	
	public const float elevationStep = 5f;
	
	//Instead of a straight slope we will create a terraced/stepped land.
	public const int terracesPerSlope = 2;

	public const int terraceSteps = terracesPerSlope * 2 + 1;
	
	//For Horizontal interpolation. Straightforward if we know what the size of the step is.
	public const float horizontalTerraceStepSize = 1f / terraceSteps;
	
	//
	public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);
	
    static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius), //This makes it top corner up.
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(0f, 0f, outerRadius) // 7th corner which is the same as the first corner to prevent out of bounds exceptions
        //when looping.
	};
	
	public static Vector3 GetFirstCorner (HexDirection direction) {
		return corners[(int)direction];
	}

	public static Vector3 GetSecondCorner (HexDirection direction) {
		return corners[(int)direction + 1];
	}
	
	//The "solid" part of a solid corner refers to the color. We blend colors usually so the first solid corner is the corner where the color
	//is truly solid or is meant to be solid instead of blended or faded.
	public static Vector3 GetFirstSolidCorner (HexDirection direction) {
		return corners[(int)direction] * solidFactor;
	}

	public static Vector3 GetSecondSolidCorner (HexDirection direction) {
		return corners[(int)direction + 1] * solidFactor;
	}
	
	//The very ends of the edge (next to the corners) cause an issue with the blending. We need to shave them off
	//which ends with creating a rectangle.
	public static Vector3 GetBridge (HexDirection direction) {
		return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
	}
	
	//Ideally we can try interpolating each step. The y value of each step takes place on every other step to allow for a
	//flat, you know, step. If we always go up we'll just have a slope.
	public static Vector3 TerraceLerp (Vector3 a, Vector3 b, int step) {
		
		float h = step * HexMetrics.horizontalTerraceStepSize;
		a.x += (b.x - a.x) * h;
		a.z += (b.z - a.z) * h;
		
		float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
		a.y += (b.y - a.y) * v;
		return a;
	}
	
	//Interpolate the color as if the connection is flat.
	public static Color TerraceLerp (Color a, Color b, int step) {
		float h = step * HexMetrics.horizontalTerraceStepSize;
		return Color.Lerp(a, b, h);
	}
	
	//Programatically figure out what edge type we want.
	public static HexEdgeType GetEdgeType (int elevation1, int elevation2) {
		if (elevation1 == elevation2) {
			return HexEdgeType.Flat;
		}
		int delta = elevation2 - elevation1;
		if (delta == 1 || delta == -1) {
			return HexEdgeType.Slope;
		}
		return HexEdgeType.Cliff;
	}
}