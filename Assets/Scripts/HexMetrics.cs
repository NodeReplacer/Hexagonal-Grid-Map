using UnityEngine;

//These are measurements of each individual cell but do not include things like coordinates.
public static class HexMetrics {
    
    //Hexagons have two radii. The first is measured from a corner and the second is measured from the middle of
    //an edge. The middle of the edge radius (inner radius) is shorter.
	public const float outerRadius = 10f;
	public const float innerRadius = outerRadius * 0.866025404f;
	public const float solidFactor = 0.8f;
	public const float blendFactor = 1f - solidFactor;
	public const float elevationStep = 3f;
	
	//Instead of a straight slope we will create a terraced/stepped land.
	public const int terracesPerSlope = 2;
	public const int terraceSteps = terracesPerSlope * 2 + 1;
	public const float horizontalTerraceStepSize = 1f / terraceSteps;//For Horizontal interpolation. 
	//Straightforward if we know what the size of the step is.
	public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);
	public static Texture2D noiseSource; //do a bit of noise randomization to make the terrain more visually interesting.
	public const float cellPerturbStrength = 4f;
	public const float noiseScale = 0.003f; //Why scale the noise? Because without the scale fitting the entire map the
	//noise will destroy our terraces, steps, slopes, and other elevation differences.
	public const float elevationPerturbStrength = 1.5f; //We're choosing to perturb vertically per cell instead of on each vertex.
	//This means each cell will remain flat but with variation between cells.
	
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
	
	//Taking a given position in world space we bilinear filter it using x and z as UV coordinates.
	//this will return a color at whatever pixel we were pointed to. Remembering that the colours of our noise are
	//nonsense other than the fact that each color represent 4 values.
	public static Vector4 SampleNoise (Vector3 position) {
		return noiseSource.GetPixelBilinear(
			position.x * noiseScale,
			position.z * noiseScale
		);
	}
}