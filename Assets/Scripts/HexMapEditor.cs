using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public Color[] colors;

	public HexGrid hexGrid;

	private Color activeColor;
	
	int activeElevation; //By changing this we allow the mapEditor the change elevations for a target cell.
	
	void Awake () {
		SelectColor(0);
	}

	void Update () {
		if (Input.GetMouseButton(0) &&
        !EventSystem.current.IsPointerOverGameObject()) {
			HandleInput();
		}
	}
    
	void HandleInput () {
		//Cast a ray to mouse position. We're looking for a hex cell.
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			EditCell(hexGrid.GetCell(hit.point));
		}
	}
	
	void EditCell (HexCell cell) {
		cell.color = activeColor;
		cell.Elevation = activeElevation;
		hexGrid.Refresh();
	}
	
	//This elevation needs to be linked to the UI.
	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}
	
	public void SelectColor (int index) {
		activeColor = colors[index];
	}
}