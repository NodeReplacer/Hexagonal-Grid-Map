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
	
	void Awake () {
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
	
	public void ColorCell (Vector3 position, Color color) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.color = color;
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
		
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
	}
	
}