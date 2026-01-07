extends PanelContainer
class_name InputNavbar

signal request_close
signal open_chat_requested

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	hide()

func close() -> void:
	hide()
	request_close.emit()


func _on_chat_butt_pressed() -> void:
	print("test")
	open_chat_requested.emit()
