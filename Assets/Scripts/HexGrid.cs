using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	public int width = 6; //Not the width of the cell but the width of the grid. The units used are the cells.
	public int height = 6;
	
	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;
	
	public HexCell cellPrefab;
	
	HexCell[] cells;
	
	public Text cellLabelPrefab;

	Canvas gridCanvas;
	
	HexMesh hexMesh;
	
	public Texture2D noiseSource; //Our noise is not a component so we can't assign it through our editor.
	//As such, we are going to use HexGrid to do it for us. It is the first to act so we'll pass it to our Awake method.
	
	void Awake () {
		HexMetrics.noiseSource = noiseSource; //The very first thing we do is establish HexMetric's noiseSource using
		//this script HexGrid as the go-between.
		
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();
		
		cells = new HexCell[height * width];

		for (int z = 0, i = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z, i++);
			}
		}
	}
	
	void Start () {
		hexMesh.Triangulate(cells); //We must triangulate the cells ONLY after the hex mesh components have awoken as well.
		//We'll keep things in Start to ensure that occurs.
	}
	
	//Keeping hte noiseSource in Awake will mean it won't survive recompiles in play mode. Enable will be invoked after
	//a recompilation so we don't need to go searching for that if a bug occurs.
	void OnEnable () {
		HexMetrics.noiseSource = noiseSource;
	}
	
	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		return cells[index];
	}
	
	//It is now up to the editor to adjust the cell and once that's done the cell must be triangulated again.
	public void Refresh () {
		hexMesh.Triangulate(cells);

		//Debug.Log("touched at " + coordinates.ToString());
	}
	
	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f); //Hexagons don't make perfectly rectangular grids.
		//each row is offset in the x direction by the inner radius. But every second row needs to undo the offset or
		//we'll make a rhombus shaped grid. subtracting z/2 before multiplying will do it.
		//For hexagons, the distance between the center of two adjacent hexagons is twice the inner radius.
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f); //The distance to the next row of cells is 1.5x the outer radius.

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.color = defaultColor;
		
		//connect the cell with the previous cell in the row (if x==0 then we're at the edge and no cell will be waiting for us
		//at i - 1)
		if (x > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		//Same as above but in the z direction so we need to find the correct cell.
		if (z > 0) {
			//The rows zigzag we are first dealing with even rows.
			if ((z & 1) == 0) {
				//So we are looking for the cell in the south east direction
				cell.SetNeighbor(HexDirection.SE, cells[i - width]);
				//Like with x above, once we hit the edge there will not be a cell to our bottom right.
				if (x > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
				}
			}
			else {
				cell.SetNeighbor(HexDirection.SW, cells[i - width]);
				if (x < width - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
				}
			}
		}
		
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		
		cell.uiRect = label.rectTransform;
		
		cell.Elevation = 0;
	}
	
}