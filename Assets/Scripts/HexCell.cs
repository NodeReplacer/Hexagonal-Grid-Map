using UnityEngine;

//A list of things that make up a hexCell. Elevation, its neighbours,coordinates, and color.
public class HexCell : MonoBehaviour {
    public HexCoordinates coordinates;
    public Color color;
    
    int elevation;
    
    [SerializeField]
	HexCell[] neighbors;
    
    public RectTransform uiRect; //We need a reference to the Rectangle that restrains the labels that displays 
    //the coordinates over each hex cell. After all, we are changing our elevations.
    
    public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}
    
    //Public facing Elevation method. We'll be working this a lot.
    public int Elevation {
		get {
			return elevation;
		}
		set {
			elevation = value;
            Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.elevationStep; //Our new y position is a combo of the value given by the slider
            //and how large each step of value is supposed to be.
			transform.localPosition = position;
            
            //Expanding our set elevation section to raise our coordinate label to stay on top of the
            Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = elevation * -HexMetrics.elevationStep;
			uiRect.localPosition = uiPosition;
		}
	}
    
    public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this; //Neighbour relations go both ways
        //So set the neighbour for the cell we just considered.
	}
    
    //The get edge type function here is very unsafe, if we get a nullReferenceException our missing reference
	//is 100-to-1 odds neighbors is missing and direction is pointing over the edge.
	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}
    
    //Convenience method to discover the slope between this cell and another cell. Yes you can pitch it a cell that's nowhere near it.
    //You're gonna have to just not do it.
    public HexEdgeType GetEdgeType (HexCell otherCell) {
		return HexMetrics.GetEdgeType(
			elevation, otherCell.elevation
		);
	}
}