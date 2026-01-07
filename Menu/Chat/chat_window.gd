extends Window

@onready var chat: Control = $Chat

func _ready() -> void:
	visible = true
	close_requested.connect(_on_close_requested)
	popup_centered()
	grab_focus()

	if chat and chat.has_method("open_focus_center"):
		chat.call("open_focus_center")

func _on_close_requested() -> void:
	queue_free()
