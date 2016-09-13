using UnityEngine;
using System.Collections;
using System;

public abstract class DropContainer : MonoBehaviour
{
	public abstract void OnStartHover(Draggable.DragInfo dragData, Draggable.DropInfo dropData);
	public abstract void OnHover(Draggable.DragInfo dragData, Draggable.DropInfo dropData);
	public abstract void OnStopHover(Draggable.DragInfo dragData, Draggable.DropInfo dropData);
	public abstract void OnDrop(Draggable.DragInfo dragData, Draggable.DropInfo dropData);

}