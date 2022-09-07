public enum HexDirection {
	NE, E, SE, SW, W, NW
}

public static class HexDirectionExtensions {

	public static HexDirection Opposite (this HexDirection direction) { //Used in HexCell. Finds the opposite direction
		//Finds the opposite direction by adding 3 (for the first three directions this works)
        //For the other 3 we subtract 3 instead.
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}
    
    public static HexDirection Previous (this HexDirection direction) {
		return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
	}

	public static HexDirection Next (this HexDirection direction) {
		return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
	}
}