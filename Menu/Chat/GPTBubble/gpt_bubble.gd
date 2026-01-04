extends HBoxContainer

@onready var text_node: Node = $PanelContainer/MarginContainer/Text

func set_text(t: String) -> void:
	if text_node == null:
		push_warning("GPTBubble: Nie znaleziono noda Bubble/Margin/Text")
		return

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
			push_warning("GPTBubble: Text node nie obsługuje tekstu: %s" % text_node.get_class())
