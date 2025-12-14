extends Control

@export var target: NodePath
@export var hide_on_start := true
@export var show_offset := Vector2(8, 8)
@export var hide_on_left_click_outside := true

func _ready() -> void:
	if hide_on_start:
		hide()

	if target == NodePath(""):
		push_warning("Menu: target is empty")
		return

	var t := get_node_or_null(target)
	if t is Control:
		(t as Control).gui_input.connect(_on_target_gui_input)
	else:
		push_warning("Menu: target must be a Control (TextureRect/Button/etc.)")

func _input(event: InputEvent) -> void:
	if hide_on_left_click_outside and visible and event is InputEventMouseButton and event.pressed:
		var mb := event as InputEventMouseButton
		if mb.button_index == MOUSE_BUTTON_LEFT and not _is_mouse_inside():
			hide()

func _on_target_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed:
		var mb := event as InputEventMouseButton
		if mb.button_index == MOUSE_BUTTON_RIGHT:
			show_at(get_viewport().get_mouse_position())
			get_viewport().set_input_as_handled()

func show_at(mouse_pos: Vector2) -> void:
	var vp := get_viewport().get_visible_rect()
	global_position = mouse_pos + show_offset

	show()
	await get_tree().process_frame

	var pad := 8.0
	var s := size
	global_position.x = clamp(global_position.x, vp.position.x + pad, vp.position.x + vp.size.x - s.x - pad)
	global_position.y = clamp(global_position.y, vp.position.y + pad, vp.position.y + vp.size.y - s.y - pad)

func _is_mouse_inside() -> bool:
	return Rect2(global_position, size).has_point(get_viewport().get_mouse_position())
