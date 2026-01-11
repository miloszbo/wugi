extends HBoxContainer

signal command_clicked(command_text: String)

@onready var text_node: Node = $PanelContainer/MarginContainer/Text
@onready var margin: MarginContainer = $PanelContainer/MarginContainer


func set_text(t: String) -> void:
	if text_node == null:
		push_warning("Nie znaleziono noda PanelContainer/MarginContainer/Text")
		return

	text_node.visible = true

	if text_node is RichTextLabel:
		(text_node as RichTextLabel).text = t
	elif text_node is Label:
		(text_node as Label).text = t
	else:
		if text_node.has_method("set_text"):
			text_node.call("set_text", t)
		elif "text" in text_node:
			text_node.set("text", t)
		else:
			push_warning("GPTBubble: node nie obsługuje tekstu: %s" % text_node.get_class())


func set_command_input(t: String) -> void:
	if margin == null:
		push_warning("GPTBubble: Brak MarginContainer")
		return

	if text_node:
		text_node.visible = false

	var button := Button.new()
	button.text = t
	button.focus_mode = Control.FOCUS_NONE

	button.flat = true
	button.custom_minimum_size = Vector2(140, 30)
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT

	var normal_style := StyleBoxFlat.new()
	normal_style.bg_color = Color(0.13, 0.14, 0.18)
	normal_style.corner_radius_top_left = 6
	normal_style.corner_radius_top_right = 6
	normal_style.corner_radius_bottom_left = 6
	normal_style.corner_radius_bottom_right = 6
	normal_style.content_margin_left = 10
	normal_style.content_margin_right = 10
	normal_style.content_margin_top = 4
	normal_style.content_margin_bottom = 4
	button.add_theme_stylebox_override("normal", normal_style)

	var hover_style := normal_style.duplicate() as StyleBoxFlat
	hover_style.bg_color = Color(0.6, 0.686, 0.8, 1.0)
	button.add_theme_stylebox_override("hover", hover_style)

	button.add_theme_color_override("font_color", Color(0.9, 0.9, 0.95))
	button.add_theme_color_override("font_hover_color", Color(1.0, 1.0, 1.0))

	button.pressed.connect(func():
		emit_signal("command_clicked", t)
	)

	margin.add_child(button)
