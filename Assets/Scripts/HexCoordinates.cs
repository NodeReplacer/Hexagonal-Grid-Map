//Made for hex coordinates to parse normal coordinates.
//Allows us to consistently describe movement and offsets in four directions.
using UnityEngine;

[System.Serializable] //So Unity can store it between play modes.
public struct HexCoordinates {
    
    [SerializeField]
	private int x, z;
    
	public int X {
		get {
			return x;
		}
	}

	public int Z {
		get {
			return z;
		}
	}

	public HexCoordinates (int x, int z) {
		this.x = x;
		this.z = z;
	}
    
    public static HexCoordinates FromOffsetCoordinates (int x, int z) {
		return new HexCoordinates(x - z / 2, z);
	}
    
    public int Y {
		get {
			return -X - Z;
		}
	}
    
    public override string ToString () {
		return "(" +
			X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	public string ToStringOnSeparateLines () {
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}
    
    public static HexCoordinates FromPosition (Vector3 position) {
		float x = position.x / (HexMetrics.innerRadius * 2f); //Find the x position by dividing x by the horizontal
        //width of one hexagon.
        float y = -x; //y is a mirror of x
		
        //but y is only a mirror of x if Z were zero. We have to shift as we move along z.
		float offset = position.z / (HexMetrics.outerRadius * 3f); 
		x -= offset;
		y -= offset;
		
        //Our x and y values end up as whole numbers at the center of each cell.
        //By rounding them to integers we ought to get the coordinates.
		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x -y); //Z is derived from the other two to create the coordinates.
        
        //With Z in the correct place, the three coordinates added together should be 0. If they are not then we
        //can assume there's been a rounding error.
        if (iX + iY + iZ != 0) {
            //Which coordinate gets rounded in the wrong direction? Well the further from the cell center the more rounding occurs
            //Assuming the coordinate that got rounded the most is incorrect.
            //We can discard the the coordinate with the largest rounding error and reconstruct it using the other two.
            float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x -y - iZ);
            
			if (dX > dY && dX > dZ) {
				iX = -iY - iZ;
			}
			else if (dZ > dY) {
				iZ = -iX - iY;
			}
		}
		return new HexCoordinates(iX, iZ);
	}
}