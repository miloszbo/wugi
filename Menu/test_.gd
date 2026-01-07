extends Node2D

var is_dragging: bool = false
var drag_offset: Vector2i = Vector2i.ZERO
var is_suprised: bool = false

@onready var _MainWindow: Window = get_window()
@onready var animated_sprite_2d: AnimatedSprite2D = $AnimatedSprite2D

@export var navbar_scene: PackedScene
@export var chat_window_scene: PackedScene

const MARGIN := 8.0

var ui_layer: CanvasLayer
var navbar: InputNavbar
var chat_window: Window

func _ready() -> void:
	_MainWindow.borderless = true
	_MainWindow.unresizable = false
	_MainWindow.always_on_top = true
	_MainWindow.gui_embed_subwindows = false
	_MainWindow.transparent = true
	_MainWindow.transparent_bg = true

	animated_sprite_2d.play("idle")

	ui_layer = CanvasLayer.new()
	add_child(ui_layer)

	_spawn_navbar()

func _spawn_navbar() -> void:
	if not navbar_scene:
		return

	navbar = navbar_scene.instantiate() as InputNavbar
	ui_layer.add_child(navbar)
	navbar.hide()

	if navbar.has_signal("open_chat_requested"):
		navbar.open_chat_requested.connect(_on_open_chat_requested)

func _input(event: InputEvent) -> void:
	if _mouse_over_ui():
		return

	var window := get_window()

	if event is InputEventMouseButton:
		var mb := event as InputEventMouseButton

		if mb.button_index == MOUSE_BUTTON_LEFT:
			if mb.pressed:
				is_dragging = true
				drag_offset = DisplayServer.mouse_get_position()
				animated_sprite_2d.play("grab")
			else:
				is_dragging = false
				animated_sprite_2d.play("place")

		if mb.button_index == MOUSE_BUTTON_RIGHT and mb.pressed:
			if not is_suprised:
				is_suprised = true
				animated_sprite_2d.play("suprise")
			else:
				is_suprised = false
				animated_sprite_2d.play("idle")
			_toggle_navbar()

	if is_dragging:
		var mouse_pos := DisplayServer.mouse_get_position()
		var delta := mouse_pos - drag_offset
		drag_offset = mouse_pos

		var new_position := window.position + delta
		var usable_rect := DisplayServer.screen_get_usable_rect(window.current_screen)
		var win_size: Vector2i = window.size

		new_position.x = clamp(new_position.x, usable_rect.position.x, usable_rect.end.x - win_size.x)
		new_position.y = clamp(new_position.y, usable_rect.position.y, usable_rect.end.y - win_size.y)

		window.position = new_position

		if navbar and navbar.visible:
			call_deferred("_reposition_navbar")

func _mouse_over_ui() -> bool:
	var hovered := get_viewport().gui_get_hovered_control()
	if hovered == null:
		return false
	if navbar and navbar.visible and (hovered == navbar or navbar.is_ancestor_of(hovered)):
		return true
	return false

func _toggle_navbar() -> void:
	if not navbar:
		return

	if navbar.visible:
		navbar.hide()
		return

	navbar.show()
	call_deferred("_reposition_navbar")

func _reposition_navbar() -> void:
	if not navbar:
		return

	var window := get_window()
	var screen_rect := DisplayServer.screen_get_usable_rect(window.current_screen)

	var nav_size: Vector2 = navbar.size
	var spr_size: Vector2 = _get_sprite_size()
	var spr_center_screen: Vector2 = get_viewport().get_screen_transform() * animated_sprite_2d.global_position

	var spr_left: float = spr_center_screen.x - spr_size.x * 0.5
	var spr_right: float = spr_center_screen.x + spr_size.x * 0.5
	var spr_top: float = spr_center_screen.y - spr_size.y * 0.5

	var right_x: float = spr_right + MARGIN
	var left_x: float = spr_left - nav_size.x - MARGIN
	var y: float = spr_top

	var screen_left: float = float(screen_rect.position.x)
	var screen_right: float = float(screen_rect.position.x + screen_rect.size.x)

	if right_x + nav_size.x <= screen_right:
		navbar.global_position = Vector2(right_x, y)
	elif left_x >= screen_left:
		navbar.global_position = Vector2(left_x, y)
	else:
		navbar.global_position = Vector2(clamp(right_x, screen_left, screen_right - nav_size.x), y)

func _get_sprite_size() -> Vector2:
	var tex := animated_sprite_2d.sprite_frames.get_frame_texture(animated_sprite_2d.animation, animated_sprite_2d.frame)
	if tex:
		return tex.get_size() * animated_sprite_2d.scale
	return Vector2(64, 64)

func _on_open_chat_requested() -> void:
	_open_chat_window()

func _open_chat_window() -> void:
	if not chat_window_scene:
		return

	if is_instance_valid(chat_window):
		chat_window.popup_centered()
		chat_window.grab_focus()
		return

	chat_window = chat_window_scene.instantiate() as Window
	get_tree().root.add_child(chat_window)
	chat_window.popup_centered()
	chat_window.grab_focus()
