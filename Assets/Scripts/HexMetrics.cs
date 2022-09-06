using UnityEngine;

public static class HexMetrics {
    
    //Hexagons have two radii. The first is measured from a corner and the second is measured from the middle of
    //an edge. The middle of the edge radius (inner radius) is shorter.
    
	public const float outerRadius = 10f;

	public const float innerRadius = outerRadius * 0.866025404f;
    
    public static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius), //This makes it top corner up.
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(0f, 0f, outerRadius) // 7th corner which is the same as the first corner to prevent out of bounds exceptions
        //when looping.
	};
}