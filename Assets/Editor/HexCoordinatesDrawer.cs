using UnityEngine;
using UnityEditor;

//Made to modify the unity interface so that coordinates cannot be changed on the prefab.
[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer {
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        HexCoordinates coordinates = new HexCoordinates(
			property.FindPropertyRelative("x").intValue,
			property.FindPropertyRelative("z").intValue
		);
		
		EditorGUI.LabelField( position, label ); //Adding a label field for our coordinates.
		position = EditorGUI.PrefixLabel(position, label);
		GUI.Label(position, coordinates.ToString());
	}
}
