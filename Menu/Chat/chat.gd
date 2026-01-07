extends Control

@export var user_bubble_scene: PackedScene
@export var gpt_bubble_scene: PackedScene

@export var messages_vbox_path: NodePath
@export var scroll_path: NodePath
@export var input_path: NodePath
@export var send_btn_path: NodePath

@onready var messages_vbox: VBoxContainer = _req(messages_vbox_path) as VBoxContainer
@onready var scroll: ScrollContainer = _req(scroll_path) as ScrollContainer
@onready var input: TextEdit = _req(input_path) as TextEdit
@onready var send_btn: Button = _req(send_btn_path) as Button


func _ready() -> void:
	send_btn.focus_mode = Control.FOCUS_NONE
	send_btn.pressed.connect(_on_send_pressed)
	input.grab_focus()


func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_ENTER:
			if event.shift_pressed:
				return
			if get_viewport().gui_get_focus_owner() != input:
				return
			get_viewport().set_input_as_handled()
			_send_message()


func _on_send_pressed() -> void:
	_send_message()


func _send_message() -> void:
	var text := input.text.strip_edges()
	if text.is_empty():
		return

	_add_user_bubble(text)
	input.text = ""
	input.grab_focus()
	_add_gpt_bubble("hello world")


func _add_user_bubble(text: String) -> void:
	if not user_bubble_scene:
		return

	var bubble := user_bubble_scene.instantiate()
	messages_vbox.add_child(bubble)

	if bubble.has_method("set_text"):
		bubble.call("set_text", text)

	_scroll_to_bottom()


func _add_gpt_bubble(text: String) -> void:
	if not gpt_bubble_scene:
		return

	var bubble := gpt_bubble_scene.instantiate()
	messages_vbox.add_child(bubble)

	if bubble.has_method("set_text"):
		bubble.call("set_text", text)

	_scroll_to_bottom()


func _scroll_to_bottom() -> void:
	await get_tree().process_frame
	scroll.scroll_vertical = int(scroll.get_v_scroll_bar().max_value)


func _req(path: NodePath) -> Node:
	if path == NodePath():
		return null
	return get_node_or_null(path)
