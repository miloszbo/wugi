extends Window

#@onready var chat: Control = $Chat

func _ready() -> void:
	size = Vector2i(480, 434)      # ustaw tu SWÓJ rozmiar
	unresizable = true             # blokada zmiany rozmiaru
	popup_centered()
	visible = true
	close_requested.connect(_on_close_requested)
	grab_focus()

	#if chat and chat.has_method("open_focus_center"):
	#	chat.call("open_focus_center")

func _on_close_requested() -> void:
	queue_free()
