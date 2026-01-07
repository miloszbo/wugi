extends HBoxContainer

@onready var text_node: Control = $PanelContainer/MarginContainer/Text


func _ready() -> void:
	size_flags_horizontal = Control.SIZE_EXPAND_FILL

func set_text(t: String) -> void:
	if text_node == null:
		return

	if text_node is RichTextLabel:
		var rtl := text_node as RichTextLabel
		rtl.text = t
		rtl.fit_content = true
		rtl.scroll_active = false
	elif text_node is Label:
		var lbl := text_node as Label
		lbl.text = t
		lbl.autowrap_mode = TextServer.AUTOWRAP_WORD

	await get_tree().process_frame
	queue_sort() # ✅ TO ZAMIAST minimum_size_changed()
