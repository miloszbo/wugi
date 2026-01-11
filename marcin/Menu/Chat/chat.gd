extends Control

@export var user_bubble_scene: PackedScene
@export var gpt_bubble_scene: PackedScene

@export var messages_vbox_path: NodePath
@export var scroll_path: NodePath
@export var input_path: NodePath
@export var send_btn_path: NodePath
@export var switch_butt_path: NodePath

@export var command_runner_path: NodePath

@onready var messages_vbox: VBoxContainer = _req(messages_vbox_path, "messages_vbox_path") as VBoxContainer
@onready var scroll: ScrollContainer = _req(scroll_path, "scroll_path") as ScrollContainer
@onready var input: TextEdit = _req(input_path, "input_path") as TextEdit
@onready var send_btn: Button = _req(send_btn_path, "send_btn_path") as Button
@onready var switch_butt: BaseButton = _req(switch_butt_path, "switch_butt_path") as BaseButton

@onready var command_runner: Node = _req(command_runner_path, "command_runner_path")


func _ready() -> void:
	if send_btn:
		send_btn.focus_mode = Control.FOCUS_NONE
		send_btn.pressed.connect(_on_send_pressed)

	if input:
		input.grab_focus()

	if switch_butt:
		switch_butt.toggled.connect(_on_switch_butt_toggled)

	# test
	if switch_butt:
		switch_butt.button_pressed = true


	# listen for responses from AgentManager
	if AgentManager:
		AgentManager.ChatResponse.connect(_on_chat_response)


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
	if not input:
		return

	var text := input.text.strip_edges()
	if text.is_empty():
		return

	_add_user_bubble(text)

	input.text = ""
	input.grab_focus()


# ==============================
# CHAT ELEMENT CREATORS
# ==============================

func _add_user_bubble(text: String) -> void:
	if user_bubble_scene == null or messages_vbox == null:
		return

	var bubble: Node = user_bubble_scene.instantiate()
	messages_vbox.add_child(bubble)

	if bubble.has_method("set_text"):
		bubble.call("set_text", text)

	# >>> COLLECT CHAT LINE HERE <<<
	if AgentManager:
		AgentManager.SubmitChat("[user] " + text)

	_scroll_to_bottom_deferred()


func _on_chat_response(text: String) -> void:
	var lines := text.split("\n", false)

	for line in lines:
		var clean := line.strip_edges()
		if clean.is_empty():
			continue

		if clean.begins_with("[cmd]"):
			var cmd := clean.substr(5).strip_edges()
			if cmd.ends_with("[/cmd]"):
				cmd = cmd.substr(0, cmd.length() - 6).strip_edges()
			if not cmd.is_empty():
				add_command_input(cmd)
			else:
				_add_gpt_bubble(clean)


func _add_gpt_bubble(text: String) -> void:
	if gpt_bubble_scene == null or messages_vbox == null:
		return

	var bubble: Node = gpt_bubble_scene.instantiate()
	messages_vbox.add_child(bubble)

	if bubble.has_method("set_text"):
		bubble.call("set_text", text)

	# >>> COLLECT CHAT LINE HERE <<<
	if AgentManager:
		AgentManager.SubmitChat("[wugi] " + text)

	_scroll_to_bottom_deferred()


func add_command_input(cmd: String) -> void:
	if switch_butt == null or not switch_butt.button_pressed:
		return

	if gpt_bubble_scene == null or messages_vbox == null:
		return

	var bubble: Node = gpt_bubble_scene.instantiate()
	messages_vbox.add_child(bubble)

	if bubble.has_method("set_command_input"):
		bubble.call("set_command_input", cmd)
	elif bubble.has_method("set_text"):
		bubble.call("set_text", cmd)

	if bubble.has_signal("command_clicked"):
		bubble.connect("command_clicked", Callable(self, "_on_command_bubble_clicked"))

	# >>> COLLECT CHAT LINE HERE <<<
	if AgentManager:
		AgentManager.SubmitChat("[cmd] " + cmd)

	_scroll_to_bottom_deferred()


# ==============================
# COMMAND EXECUTION (NO LOGGING)
# ==============================

func _on_command_bubble_clicked(cmd: String) -> void:
	var console := get_node_or_null("/root/SystemConsole")
	if console:
		console.Send(cmd)
	else:
		push_warning("SystemConsole autoload not found")


# ==============================
# UI UTILS
# ==============================

func _scroll_to_bottom_deferred() -> void:
	await get_tree().process_frame
	if scroll:
		scroll.scroll_vertical = int(scroll.get_v_scroll_bar().max_value)


func _on_switch_butt_toggled(toggled_on: bool) -> void:
	if toggled_on:
		print("Toolbar ON")
	else:
		print("Toolbar OFF")


func _req(path: NodePath, field: String) -> Node:
	if path == NodePath():
		return null
	return get_node_or_null(path)
