extends Node2D

var move_speed: int = 2
var direction: Vector2i = Vector2i(1,0)
var move_vector: Vector2i
var is_chilling: bool = false
var is_dragging: bool = false
var drag_offset: Vector2i = Vector2i.ZERO
var sprite_size

@onready var _MainWindow: Window = get_window()

func _input(event):
	var window: Window = get_window()
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				var mouse_pos = get_global_mouse_position()
				var sprite_rect = Rect2(Vector2.ZERO, sprite_size)
				is_dragging = true
				drag_offset = mouse_pos
			else:
				is_dragging = false
		
	if is_dragging:
		var new_position = DisplayServer.mouse_get_position() - drag_offset
		var usable_rect = DisplayServer.screen_get_usable_rect()
		new_position.x = clamp(new_position.x, usable_rect.position.x, usable_rect.end.x - sprite_size.x)
		new_position.y = clamp(new_position.y, usable_rect.position.y, usable_rect.end.y - sprite_size.y)
		window.position = new_position

func _ready():
	_MainWindow.borderless = true
	_MainWindow.unresizable = false
	_MainWindow.always_on_top = true
	_MainWindow.gui_embed_subwindows = false
	_MainWindow.transparent = true
	_MainWindow.transparent_bg = true
	
	_update_mouse_mask()
	
	$AnimatedSprite2D.frame_changed.connect(_update_mouse_mask)
	sprite_size = $AnimatedSprite2D.sprite_frames.get_frame_texture($AnimatedSprite2D.animation, $AnimatedSprite2D.frame).get_size()
		
func _update_mouse_mask():
	var anim = $AnimatedSprite2D
	
	var texture = anim.sprite_frames.get_frame_texture(anim.animation,anim.frame)
	var image = texture.get_image()
	
	if anim.flip_h:
		image.flip_x()
	
	var bitmap = BitMap.new()
	bitmap.create_from_image_alpha(image)
	
	var polygons = bitmap.opaque_to_polygons(Rect2(Vector2.ZERO, texture.get_size()), 0.1)
	
	DisplayServer.window_set_mouse_passthrough(polygons)
