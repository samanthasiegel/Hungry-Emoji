using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class FoodScript : Draggable {
	#region implemented abstract members of Draggable

	protected override void HandleBeginDrag (DragInfo dragData)
	{
		
	}

	protected override void HandleDrag (DragInfo dragData)
	{
		
	}

	protected override void HandleEndDrag (DragInfo dragData)
	{
		
	}

	protected override void OnDrop (DragInfo dragData, DropInfo dropData)
	{
		Debug.Log ("I got dropped into" + dropData.Container.gameObject.name);
		Destroy (dropData.Draggable.gameObject);
	}

	protected override void OnStartHovering (DragInfo dragData, DropInfo dropData)
	{
		
	}

	protected override void OnHovering (DragInfo dragData, DropInfo dropData)
	{
		
	}

	protected override void OnStopHovering (DragInfo dragData, DropInfo dropData)
	{
		
	}

	#endregion




}
