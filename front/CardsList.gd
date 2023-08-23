extends VBoxContainer

var selectable_card_li_ps: PackedScene= preload("res://SelectableCardListItem.tscn")
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_main_card_added(card):
	var child = selectable_card_li_ps.instantiate()
	add_child(child)
	child.Load(card)
