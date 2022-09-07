using UnityEngine;

public static class HexMetrics {
    
    //Hexagons have two radii. The first is measured from a corner and the second is measured from the middle of
    //an edge. The middle of the edge radius (inner radius) is shorter.
    
	public const float outerRadius = 10f;

	public const float innerRadius = outerRadius * 0.866025404f;
    
	public const float solidFactor = 0.75f;
	
	public const float blendFactor = 1f - solidFactor;
	
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
}