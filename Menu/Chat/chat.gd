extends Control

@export var user_bubble_scene: PackedScene
@export var gpt_bubble_scene: PackedScene

# Ustaw w Inspectorze (przeciągnij nody)
@export var messages_vbox_path: NodePath
@export var scroll_path: NodePath
@export var input_path: NodePath
@export var send_btn_path: NodePath

@onready var messages_vbox: VBoxContainer = _req(messages_vbox_path, "messages_vbox_path") as VBoxContainer
@onready var scroll: ScrollContainer = _req(scroll_path, "scroll_path") as ScrollContainer
@onready var input: TextEdit = _req(input_path, "input_path") as TextEdit
@onready var send_btn: Button = _req(send_btn_path, "send_btn_path") as Button


func _ready() -> void:
	await get_tree().process_frame
	DisplayServer.window_move_to_foreground()

	# Fokus na właściwym TextEdit
	input.focus_mode = Control.FOCUS_ALL
	input.grab_focus()

	# Klik przycisku wysyła
	send_btn.pressed.connect(_on_send_pressed)

	# Drobna poprawka: nie pozwól przyciskowi kraść fokusu (opcjonalne, ale pomaga UX)
	send_btn.focus_mode = Control.FOCUS_NONE

	print("window focused:", DisplayServer.window_is_focused())
	print("focus owner:", get_viewport().gui_get_focus_owner())


# Enter wysyła TYLKO gdy fokus jest w TextEdit.
# Shift+Enter zostaje normalną nową linią.
func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_ENTER:
			if event.shift_pressed:
				return

			# wysyłamy tylko jeśli aktualnie piszemy w input
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

	# reset inputa
	input.text = ""
	input.grab_focus()

	# stała odpowiedź GPT
	_add_gpt_bubble("hello world")


func _add_user_bubble(text: String) -> void:
	if user_bubble_scene == null:
		push_warning("user_bubble_scene nie jest ustawione w Inspectorze")
		return

	var bubble: Node = user_bubble_scene.instantiate()
	messages_vbox.add_child(bubble)

	if bubble.has_method("set_text"):
		bubble.call("set_text", text)
	else:
		push_warning("UserBubble nie ma set_text(t). Podłącz user_bubble.gd do root dymka.")

	_scroll_to_bottom_deferred()


func _add_gpt_bubble(text: String) -> void:
	if gpt_bubble_scene == null:
		push_warning("gpt_bubble_scene nie jest ustawione w Inspectorze")
		return

	var bubble: Node = gpt_bubble_scene.instantiate()
	messages_vbox.add_child(bubble)

	if bubble.has_method("set_text"):
		bubble.call("set_text", text)
	else:
		push_warning("GPTBubble nie ma set_text(t). Podłącz gpt_bubble.gd do root dymka.")

	_scroll_to_bottom_deferred()


func _scroll_to_bottom_deferred() -> void:
	await get_tree().process_frame
	scroll.scroll_vertical = int(scroll.get_v_scroll_bar().max_value)


func _req(path: NodePath, field: String) -> Node:
	if path == NodePath():
		push_error("Brak ustawionego NodePath: %s (ustaw w Inspectorze)" % field)
		return null

	var n := get_node_or_null(path)
	if n == null:
		push_error("Nie znaleziono noda dla %s: %s" % [field, str(path)])
	return n
