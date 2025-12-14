extends Node2D

var move_speed: int = 2
var direction: Vector2i = Vector2i(1,0)
var move_vector: Vector2i
var is_chilling: bool = false
var is_dragging: bool = false
var drag_offset: Vector2i = Vector2i.ZERO
var sprite_size
var is_suprised: bool = false

@onready var _MainWindow: Window = get_window()
@onready var animated_sprite_2d: AnimatedSprite2D = $AnimatedSprite2D

func _input(event):
	var window: Window = get_window()
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				animated_sprite_2d.frame_changed.disconnect(_update_mouse_mask)
				var mouse_pos = get_global_mouse_position()
				var sprite_rect = Rect2(Vector2.ZERO, sprite_size)
				is_dragging = true
				drag_offset = mouse_pos
				animated_sprite_2d.play("grab")
			else:
				is_dragging = false
				animated_sprite_2d.play("place")
		if event.button_index == MOUSE_BUTTON_RIGHT:
			if event.pressed:
				if is_suprised == false:
					is_suprised = true
					animated_sprite_2d.play("suprise")
				else:
					is_suprised = false
					animated_sprite_2d.play("idle")
					
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
	
	animated_sprite_2d.play("idle")
	
	_update_mouse_mask()
	
	animated_sprite_2d.frame_changed.connect(_update_mouse_mask)
	sprite_size = animated_sprite_2d.sprite_frames.get_frame_texture(animated_sprite_2d.animation, animated_sprite_2d.frame).get_size()
		
func _update_mouse_mask():
	var anim = animated_sprite_2d
	
	var texture = anim.sprite_frames.get_frame_texture(anim.animation,anim.frame)
	var image = texture.get_image()
	
	if anim.flip_h:
		image.flip_x()
	
	var bitmap = BitMap.new()
	bitmap.create_from_image_alpha(image)
	
	var polygons = bitmap.opaque_to_polygons(Rect2(Vector2.ZERO, texture.get_size()), 0.1)
	
	DisplayServer.window_set_mouse_passthrough(polygons)
	


func _on_animated_sprite_2d_animation_finished() -> void:
	if animated_sprite_2d.animation == "place":
		animated_sprite_2d.frame_changed.connect(_update_mouse_mask)
		animated_sprite_2d.play("idle")
	if animated_sprite_2d.animation == "grab":
		animated_sprite_2d.play("hang")
