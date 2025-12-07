using Godot;
using System;

public partial class Main : Node2D
{
	[Export]
	public PopupMenu ContextMenu { get; set; }

	[Export]
	public Area2D TargetArea { get; set; }

	public override void _Ready()
	{
		ContextMenu ??= GetNode<PopupMenu>("ContextMenu");
		TargetArea ??= GetNode<Area2D>("TargetArea"); // zmień ścieżkę/nazwę jeśli inna

		ContextMenu.Clear();
		ContextMenu.AddItem("Opcja 1", 0);
		ContextMenu.AddItem("Opcja 2", 1);
		ContextMenu.AddSeparator();
		ContextMenu.AddItem("Wyjście", 2);

		ContextMenu.IdPressed += OnMenuItemPressed;

		TargetArea.InputEvent += OnTargetAreaInputEvent;
	}

	private void OnTargetAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mb &&
			mb.ButtonIndex == MouseButton.Right &&
			mb.Pressed)
		{
			Vector2 mousePos = GetViewport().GetMousePosition();
			ContextMenu.Position = (Vector2I)mousePos;
			ContextMenu.Popup();
		}
	}

	private void OnMenuItemPressed(long id)
	{
		switch (id)
		{
			case 0:
				GD.Print("Opcja 1");
				break;
			case 1:
				GD.Print("Opcja 2");
				break;
			case 2:
				GD.Print("Wyjście");
				GetTree().Quit();
				break;
		}
	}
}
