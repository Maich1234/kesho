extends Node3D
const District = preload("res://scripts/district.gd")
const GOLD := Color("e6b961")
const CREAM := Color("f3eddd")
const MUTED := Color("a5b8ae")
const INK := Color("122526")
var world: Node3D
var ui: Control
var hud: Control
var modal: Control
var menu: Control
var title_font: Font
var body_font: Font
var status_label: Label
var goals_label: Label
var toast_label: Label
var save_clock := 0.0
var story: Dictionary
var joystick_visible := false
var language := "en"
var joystick_pad: Control
var joystick_knob: ColorRect
var joystick_finger := -1
var reduce_motion := false

func _ready() -> void:
	title_font=load("res://assets/fonts/Title.ttf")
	body_font=load("res://assets/fonts/Body.ttf")
	story=JSON.parse_string(FileAccess.get_file_as_string("res://data/opening.json"))
	world=District.new(); add_child(world)
	world.arrived.connect(open_location)
	var canvas:=CanvasLayer.new(); add_child(canvas)
	ui=Control.new(); ui.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT); ui.mouse_filter=Control.MOUSE_FILTER_IGNORE
	canvas.add_child(ui)
	Life.changed.connect(update_hud)
	show_menu()

func tr_ui(en: String, sw: String) -> String:
	return sw if language=="sw" else en

func panel_style(colour: Color, border: Color = Color("344b46"), radius: int = 12) -> StyleBoxFlat:
	var s:=StyleBoxFlat.new(); s.bg_color=colour
	s.border_color=border; s.set_border_width_all(1); s.set_corner_radius_all(radius)
	s.content_margin_left=20; s.content_margin_right=20; s.content_margin_top=16; s.content_margin_bottom=16
	return s

func text_label(value: String, size: int = 18, colour: Color = CREAM, serif: bool = false) -> Label:
	var l:=Label.new(); l.text=value; l.add_theme_font_override("font",title_font if serif else body_font)
	l.add_theme_font_size_override("font_size",size); l.add_theme_color_override("font_color",colour)
	l.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
	l.mouse_filter=Control.MOUSE_FILTER_IGNORE
	return l

func button(value: String, action: Callable, primary: bool = false) -> Button:
	var b:=Button.new(); b.text=value; b.custom_minimum_size.y=48
	b.add_theme_font_override("font",body_font); b.add_theme_font_size_override("font_size",16)
	b.add_theme_color_override("font_color", INK if primary else CREAM)
	b.add_theme_color_override("font_hover_color", INK if primary else GOLD)
	b.add_theme_color_override("font_disabled_color",Color("788a80"))
	b.add_theme_stylebox_override("normal",panel_style(GOLD if primary else Color("1d3533")))
	b.add_theme_stylebox_override("hover",panel_style(Color("f0cb83") if primary else Color("2a4740"),GOLD))
	b.add_theme_stylebox_override("pressed",panel_style(Color("b38c46") if primary else Color("35564b"),GOLD))
	b.add_theme_stylebox_override("focus",panel_style(Color(0,0,0,0),GOLD))
	b.add_theme_stylebox_override("disabled",panel_style(Color("21302e"),Color("30423b")))
	b.pressed.connect(action)
	return b

func column(parent: Node, gap: int = 10) -> VBoxContainer:
	var v:=VBoxContainer.new(); v.add_theme_constant_override("separation",gap); parent.add_child(v); return v

func row(parent: Node, gap: int = 10) -> HBoxContainer:
	var h:=HBoxContainer.new(); h.add_theme_constant_override("separation",gap); parent.add_child(h); return h

func clear_modal() -> void:
	if is_instance_valid(modal): modal.free()
	modal=null
	world.paused=false

func modal_box(width: int = 720) -> VBoxContainer:
	clear_modal(); world.paused=true; world.joystick=Vector2.ZERO
	modal=Control.new(); modal.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT); ui.add_child(modal)
	var shade:=ColorRect.new(); shade.color=Color(0.025,.06,.055,.73); shade.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT); modal.add_child(shade)
	var panel:=PanelContainer.new(); panel.add_theme_stylebox_override("panel",panel_style(Color("142b2a"),Color("567066"),16))
	panel.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	panel.offset_left=-width/2.0; panel.offset_right=width/2.0; panel.offset_top=-306; panel.offset_bottom=306
	modal.add_child(panel)
	var scroll:=ScrollContainer.new(); scroll.horizontal_scroll_mode=ScrollContainer.SCROLL_MODE_DISABLED; panel.add_child(scroll)
	var content:=column(scroll,12); content.size_flags_horizontal=Control.SIZE_EXPAND_FILL
	return content

func show_menu() -> void:
	clear_modal(); world.paused=true
	if is_instance_valid(hud): hud.free()
	if is_instance_valid(menu): menu.free()
	menu=Control.new(); menu.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT); ui.add_child(menu)
	var shade:=ColorRect.new(); shade.color=Color(.035,.09,.085,.93); shade.position=Vector2.ZERO; shade.size=Vector2(530,720); menu.add_child(shade)
	var v:=column(menu,18); v.position=Vector2(48,45); v.size=Vector2(420,630)
	v.add_child(text_label("WABUNIFU  /  INTERACTIVE STORIES",12,GOLD))
	v.add_child(text_label("KESHO",72,CREAM,true))
	v.add_child(text_label("Every tomorrow\nhas a price.",29,CREAM,true))
	v.add_child(text_label("An ordinary arrival. A city of favours.\nWhat will you owe when you finally make it?",17,MUTED))
	var space:=Control.new(); space.custom_minimum_size.y=12; v.add_child(space)
	v.add_child(button("Begin a life",confirm_new,true))
	var resume:=button("Continue saved life",resume_life); resume.disabled=not FileAccess.file_exists(Life.SAVE) and not FileAccess.file_exists(Life.BACKUP); v.add_child(resume)
	v.add_child(button("About this first playable",show_about))
	v.add_child(text_label("CHAPTER 01  •  A PLACE BEFORE DARK",12,GOLD))
	v.add_child(text_label("18+ concept • Offline prototype v0.1\nOriginal placeholder 3D art. No purchases or ads.",12,MUTED))
	var tag:=text_label("MAWENI DISTRICT\n10:00  /  A NEW BEGINNING",15,INK)
	tag.position=Vector2(880,605); tag.size=Vector2(330,70); menu.add_child(tag)

func confirm_new() -> void:
	if not FileAccess.file_exists(Life.SAVE): create_character(); return
	var v:=modal_box(620)
	v.add_child(text_label("Start another life?",30,CREAM,true))
	v.add_child(text_label("This prototype has one save slot. Starting again replaces it, including your journal. Continue your current life to keep it.",18,MUTED))
	v.add_child(button("Create a new life",create_character,true)); v.add_child(button("Keep my current life",func(): clear_modal(); world.paused=true))

func selector(parent: Node, label_text: String, values: Array) -> OptionButton:
	parent.add_child(text_label(label_text,14,MUTED))
	var opt:=OptionButton.new(); opt.custom_minimum_size.y=44
	opt.add_theme_font_override("font",body_font); opt.add_theme_font_size_override("font_size",16)
	for value in values: opt.add_item(value)
	parent.add_child(opt); return opt

func create_character() -> void:
	var v:=modal_box(810)
	v.add_child(text_label("01 / YOUR FIRST TOMORROW",13,GOLD))
	v.add_child(text_label("The city does not know you. Yet.",30,CREAM,true))
	v.add_child(text_label("You are 19. You have C1,200, one bag and a contact waiting at the square. C is fictional city currency.",16,MUTED))
	var h:=row(v,24)
	var left:=column(h,7); left.size_flags_horizontal=Control.SIZE_EXPAND_FILL
	var right:=column(h,7); right.size_flags_horizontal=Control.SIZE_EXPAND_FILL
	left.add_child(text_label("NAME",14,MUTED)); var name_input:=LineEdit.new(); name_input.text="Amani"; name_input.max_length=24; name_input.custom_minimum_size.y=44; left.add_child(name_input)
	var background:=selector(left,"FAMILY BACKGROUND",["Family household","Independent parent","Community upbringing"])
	var gender:=selector(left,"GENDER",["Man","Woman","Non-binary"])
	var belief:=selector(left,"BELIEFS",["Undecided","Christian","Muslim","Traditional beliefs","Spiritual","Non-religious"])
	var skin:=selector(right,"SKIN TONE",["Deep brown","Rich brown","Warm brown","Golden brown"])
	var outfit:=selector(right,"CLOTHING",["Ochre","Forest","Terracotta","Slate"])
	var family_personality:=selector(right,"KEY RELATIVE'S PUBLIC PERSONALITY",["Supportive","Outspoken","Reserved","Ambitious"])
	var mode:=selector(right,"LIFE MODE",["Checkpoints","Permanent consequences"])
	v.add_child(text_label("This short chapter contains financial pressure and manipulation. No graphic scenes. The adult relative and full-life death systems are planned, not playable here.",13,MUTED))
	v.add_child(button("Arrive in Maweni",func():
		Life.fresh(name_input.text,background.get_item_text(background.selected),skin.selected,outfit.selected,mode.get_item_text(mode.selected),gender.get_item_text(gender.selected),belief.get_item_text(belief.selected),family_personality.get_item_text(family_personality.selected))
		start_play()
		open_intro(),true))
	v.add_child(button("Back",func(): clear_modal(); world.paused=true))

func resume_life() -> void:
	if Life.load_life(): start_play()
	else:
		var v:=modal_box(600); v.add_child(text_label("Your save could not be opened",27,CREAM,true)); v.add_child(text_label("The main file and its backup were unavailable or invalid. Your files have not been replaced.",18,MUTED)); v.add_child(button("Back",func(): clear_modal(); world.paused=true))

func start_play() -> void:
	clear_modal()
	if is_instance_valid(menu): menu.free()
	world.set_avatar(int(Life.state.skin),int(Life.state.outfit))
	world.avatar.position=Vector3(float(Life.state.position[0]),0,float(Life.state.position[1]))
	world.route.clear(); world.pending=""
	build_hud(); world.paused=false
	if Life.state.complete: show_ending()

func open_intro() -> void:
	var v:=modal_box(720)
	v.add_child(text_label("CHAPTER 01  /  MAWENI",13,GOLD))
	v.add_child(text_label("A place before dark",36,CREAM,true))
	var contact_text: String={"Family household":"Your aunt Sifa has sent her neighbour to meet you. Her spare room is crowded, but it is a beginning.","Independent parent":"Your old school friend Kito is waiting. Your parent gave you C1,200 and made you promise to call.","Community upbringing":"Leila, a community coordinator, has arranged an introduction. She can point you towards a hostel and legitimate applications."}[Life.state.background]
	v.add_child(text_label(contact_text,19,CREAM))
	v.add_child(text_label("Three days to find your footing. Meet your contact, secure a bed and explore a way forward. Tap a destination below to walk there, or tap the street. Time advances through decisions, never while you read.",17,MUTED))
	v.add_child(text_label("Look for clues in what people say. No percentages will tell you who deserves your trust.",16,GOLD))
	v.add_child(button("Step into the city",clear_modal,true))

func build_hud() -> void:
	if is_instance_valid(hud): hud.free()
	hud=Control.new(); hud.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT); hud.mouse_filter=Control.MOUSE_FILTER_IGNORE; ui.add_child(hud)
	var top:=PanelContainer.new(); top.position=Vector2(20,16); top.size=Vector2(1240,70)
	top.add_theme_stylebox_override("panel",panel_style(Color(.055,.13,.13,.97))); hud.add_child(top)
	var h:=row(top,20)
	var brand:=text_label("KESHO",28,CREAM,true); brand.autowrap_mode=TextServer.AUTOWRAP_OFF; brand.custom_minimum_size.x=120; h.add_child(brand)
	status_label=text_label("",16,MUTED); status_label.autowrap_mode=TextServer.AUTOWRAP_OFF; status_label.clip_text=true; status_label.size_flags_horizontal=Control.SIZE_EXPAND_FILL; h.add_child(status_label)
	h.add_child(button(tr_ui("Journal","Shajara"),show_journal))
	h.add_child(button(tr_ui("Settings","Mipangilio"),show_settings))
	h.add_child(button(tr_ui("Save & exit","Hifadhi & toka"),save_and_exit))
	var goals:=PanelContainer.new(); goals.position=Vector2(20,103); goals.size=Vector2(285,150)
	goals.add_theme_stylebox_override("panel",panel_style(Color(.055,.13,.13,.92))); hud.add_child(goals)
	var gv:=column(goals,7); gv.add_child(text_label(tr_ui("A PLACE BEFORE DARK","MAHALI KABLA YA GIZA"),12,GOLD))
	goals_label=text_label("",15,CREAM); gv.add_child(goals_label)
	var bottom:=PanelContainer.new(); bottom.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	bottom.offset_left=20; bottom.offset_right=-20; bottom.offset_top=-100; bottom.offset_bottom=-16
	bottom.add_theme_stylebox_override("panel",panel_style(Color(.055,.13,.13,.97))); hud.add_child(bottom)
	var destinations:=row(bottom,10)
	var labels:={"contact":tr_ui("Contact","Mwenyeji"),"courtyard":tr_ui("Courtyard","Makazi"),"work":tr_ui("Work yard","Kazi"),"learning":tr_ui("Learning centre","Elimu"),"kiosk":tr_ui("Tea kiosk","Kibanda")}
	for key in labels:
		var place: String=key
		var b:=button(labels[key],func(): world.go_to(place)); b.size_flags_horizontal=Control.SIZE_EXPAND_FILL; destinations.add_child(b)
	var rest:=button(tr_ui("End day","Maliza siku"),end_day,true); destinations.add_child(rest)
	toast_label=text_label("",15,GOLD); toast_label.position=Vector2(340,560); toast_label.size=Vector2(650,42); hud.add_child(toast_label)
	build_joystick(); update_hud()

func update_hud() -> void:
	if not is_instance_valid(status_label) or Life.state.is_empty(): return
	status_label.text="%s  •  %s %d / 3  •  %02d:00  •  C%d  •  %s %d" % [Life.state.name,tr_ui("Day","Siku"),int(Life.state.day),int(Life.state.hour),int(Life.state.money),tr_ui("Energy","Nguvu"),int(Life.state.energy)]
	var a: String="✓" if Life.has_flag("met_contact") else "○"
	var b: String="✓" if Life.has_flag("shelter") else "○"
	var direction:=Life.has_flag("worked") or Life.has_flag("training_application") or Life.has_flag("campus_lead") or Life.has_flag("shortcut_taken") or Life.has_flag("delivery_done")
	goals_label.text="%s Meet your city contact\n%s Arrange somewhere to sleep\n%s Explore a way forward" % [a,b,"✓" if direction else "○"]
	if not Life.last_error.is_empty() and is_instance_valid(toast_label): toast_label.text=Life.last_error

func contact_name() -> String:
	return {"Family household":"Sifa's neighbour • family contact","Independent parent":"Kito • old school friend","Community upbringing":"Leila • community coordinator"}.get(Life.state.background,"Your contact")

func open_location(place: String) -> void:
	if Life.state.complete: show_ending(); return
	var scene: Dictionary=story[place]
	var v:=modal_box(830)
	var h:=row(v)
	var tag:=text_label("MAWENI / " + world.locations[place].title,12,GOLD); tag.size_flags_horizontal=Control.SIZE_EXPAND_FILL; h.add_child(tag)
	h.add_child(button("Close",clear_modal))
	v.add_child(text_label(scene.title,30,CREAM,true))
	v.add_child(text_label(contact_name() if place=="contact" else scene.speaker,14,GOLD))
	v.add_child(text_label(scene.text,17,MUTED))
	var any_choice:=false
	for choice in scene.choices:
		if choice.has("background") and choice.background!=Life.state.background: continue
		var already:=false
		for flag in choice.get("excludes",[]):
			if Life.has_flag(flag): already=true
		if already: continue
		var visible_choice:=true
		for flag in choice.get("requires",[]):
			if not Life.has_flag(flag): visible_choice=false
		if not visible_choice: continue
		any_choice=true
		var label_text: String=choice.label
		var details: Array[String]=[]
		if int(choice.get("hours",0))>0: details.append("%dh" % int(choice.hours))
		if int(choice.get("effort",0))>0: details.append("−%d energy" % int(choice.effort))
		if not details.is_empty(): label_text+="  ["+" · ".join(details)+"]"
		var selected: Dictionary=choice
		var b:=button(label_text,func(): take_choice(place,selected))
		b.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
		b.custom_minimum_size.y=58; b.disabled=not Life.available(choice); v.add_child(b)
	if not any_choice: v.add_child(text_label("Nothing more to arrange here today. Your other leads are waiting.",17,MUTED))
	v.add_child(text_label("Unavailable choices need more time, money or energy. The day ends at 22:00.\nReading and walking do not consume your planning hours.",12,MUTED))

func take_choice(place: String, choice: Dictionary) -> void:
	Life.state.position=[world.avatar.position.x,world.avatar.position.z]
	var result:=Life.apply(choice,place)
	var v:=modal_box(740)
	v.add_child(text_label("A DECISION MADE",12,GOLD))
	v.add_child(text_label(story[place].title,29,CREAM,true))
	v.add_child(text_label(result.text,20,CREAM))
	v.add_child(text_label("The city remembers. Your journal has been updated.",14,MUTED))
	v.add_child(button("Continue conversation",func(): open_location(place),true))
	v.add_child(button("Back to the street",clear_modal))

func end_day() -> void:
	var v:=modal_box(680)
	v.add_child(text_label("NIGHT IN MAWENI",12,GOLD))
	if not Life.has_flag("shelter"):
		v.add_child(text_label("Find a bed first",30,CREAM,true))
		v.add_child(text_label("Speak to your contact, then visit the courtyard. You still have time to arrange accommodation; the city will wait while you read or walk.",18,MUTED))
		v.add_child(button("Return to the street",clear_modal,true)); return
	v.add_child(text_label("%s day %d?" % ["Finish" if int(Life.state.day)==3 else "Rest after",int(Life.state.day)],30,CREAM,true))
	v.add_child(text_label("Your room is covered for these three nights. Rest restores energy. Unused hours do not carry forward. Ending day three completes this prototype, not the character’s full life.",18,MUTED))
	v.add_child(button("End chapter" if int(Life.state.day)==3 else "Sleep until morning",func():
		Life.state.position=[world.avatar.position.x,world.avatar.position.z]
		Life.next_day(); clear_modal()
		if Life.state.complete: show_ending()
		else: toast_label.text="A new morning. Your relationships and decisions carry forward.",true))
	v.add_child(button("Keep exploring",clear_modal))

func show_ending() -> void:
	var v:=modal_box(780)
	v.add_child(text_label("CHAPTER 01 / A BEGINNING, NOT A VERDICT",12,GOLD))
	v.add_child(text_label("You found a foothold.",36,CREAM,true))
	var lines: Array[String]=[]
	if Life.has_flag("worked") or Life.has_flag("delivery_done"): lines.append("You earned money through documented work. You also learned what it costs in time and energy.")
	if Life.has_flag("training_application"): lines.append("Your training application is in the system. Admission and the next chapter are still ahead.")
	if Life.has_flag("campus_lead"): lines.append("A campus sponsorship lead is open. It needs documents and follow-through.")
	if Life.has_flag("owed_favour"): lines.append("The envelope errand paid. A stranger now considers you someone he can call on.")
	if Life.has_flag("broker_missing"): lines.append("The broker took your money. The complaint and the people you trusted may matter later.")
	if lines.is_empty(): lines.append("You secured somewhere to sleep, but your next step is still undecided. Surviving the arrival is a beginning too.")
	v.add_child(text_label("\n\n".join(lines),18,CREAM))
	v.add_child(text_label("C%d remaining • %d recorded decisions\nThis is a short systems prototype. Full-life careers, online play, cloud signup and later consequences are not implemented yet." % [int(Life.state.money),Life.state.history.size()],15,MUTED))
	v.add_child(button("Read your private journal",show_journal,true))
	v.add_child(button("Return to title",save_and_exit))

func show_journal() -> void:
	var v:=modal_box(850)
	v.add_child(text_label("PRIVATE / ON THIS DEVICE",12,GOLD))
	v.add_child(text_label("What will you remember?",30,CREAM,true))
	v.add_child(text_label("Who benefited? Who carried the cost? What did you know then—and what do you know now? Your notes stay out of public summaries.",16,MUTED))
	var notes:=TextEdit.new(); notes.text=Life.state.notes; notes.placeholder_text="Your reflection…"; notes.custom_minimum_size=Vector2(0,115); notes.add_theme_font_size_override("font_size",17); v.add_child(notes)
	v.add_child(button("Save reflection",func(): Life.state.notes=notes.text.left(12000); Life.commit(); show_journal(),true))
	for i in range(Life.state.history.size()-1,-1,-1):
		var entry: Dictionary=Life.state.history[i]
		v.add_child(text_label("DAY %d · %02d:00 / %s" % [int(entry.day),int(entry.hour),str(entry.place).to_upper()],12,GOLD))
		v.add_child(text_label(entry.choice,16,CREAM))
		v.add_child(text_label(entry.result,15,MUTED))
	if Life.state.history.is_empty(): v.add_child(text_label("Your first decision is still ahead.",17,MUTED))
	v.add_child(button("Back",func():
		Life.state.notes=notes.text.left(12000); Life.commit(); clear_modal()
		if Life.state.complete: show_ending()))

func show_settings() -> void:
	var v:=modal_box(730)
	v.add_child(text_label("MAKE YOURSELF COMFORTABLE",12,GOLD))
	v.add_child(text_label("Controls & presentation",30,CREAM,true))
	v.add_child(button("Joystick: " + ("ON" if joystick_visible else "OFF"),func(): joystick_visible=not joystick_visible; build_hud(); show_settings()))
	v.add_child(button("Interface: " + ("English" if language=="en" else "Kiswahili"),func(): language="sw" if language=="en" else "en"; build_hud(); show_settings()))
	v.add_child(text_label("Kiswahili currently covers navigation and status labels. Story text is English with contextual Swahili expressions. Full localisation is planned.",15,MUTED))
	v.add_child(text_label("Tap destinations to walk there. Keyboard: arrow keys. With the joystick, tap a destination marker to interact after approaching. Game time advances only through choices.",16,CREAM))
	v.add_child(text_label("Sensitive-content controls will accompany later mature story chapters. This opening contains financial pressure, not graphic violence or sexual scenes.\n\nCloud accounts and online sessions are not connected in this build.",15,MUTED))
	v.add_child(button("Return to the city",clear_modal,true))

func show_about() -> void:
	var v:=modal_box(730)
	v.add_child(text_label("KESHO / FIRST PLAYABLE",13,GOLD))
	v.add_child(text_label("A life before the headlines.",32,CREAM,true))
	v.add_child(text_label("Explore a small original 3D district, manage three days, test several paths and keep a private journal. The city, people, organisations and events in this chapter are fictional. These scenes have not been presented as adaptations of researched news cases.",18,CREAM))
	v.add_child(text_label("This is an offline prototype with placeholder art, local saving and no connected account system. It is not the complete 30–50-hour game. Beliefs, gender and family personality are saved identity fields; their deeper story effects are future work.",16,MUTED))
	v.add_child(button("Third-party licences",show_licences))
	v.add_child(button("Back",func(): clear_modal(); world.paused=true,true))

func show_licences() -> void:
	var v:=modal_box(800)
	v.add_child(text_label("CREDITS & LICENCES",13,GOLD))
	v.add_child(text_label(FileAccess.get_file_as_string("res://THIRD_PARTY_NOTICES.md"),15,CREAM))
	v.add_child(text_label(FileAccess.get_file_as_string("res://assets/fonts/LICENSE.txt"),13,MUTED))
	v.add_child(button("Back",show_about,true))

func save_and_exit() -> void:
	if not Life.state.is_empty():
		Life.state.position=[world.avatar.position.x,world.avatar.position.z]
		if not Life.commit():
			var v:=modal_box(650); v.add_child(text_label("Save needs attention",28,CREAM,true)); v.add_child(text_label(Life.last_error,18,MUTED)); v.add_child(button("Keep playing",clear_modal)); return
	show_menu()

func build_joystick() -> void:
	joystick_pad=Control.new(); joystick_pad.position=Vector2(35,407); joystick_pad.size=Vector2(150,150); joystick_pad.visible=joystick_visible
	hud.add_child(joystick_pad)
	var base:=Panel.new(); base.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT); base.add_theme_stylebox_override("panel",panel_style(Color(.08,.18,.16,.7),Color("718d78"),75)); base.mouse_filter=Control.MOUSE_FILTER_IGNORE; joystick_pad.add_child(base)
	joystick_knob=ColorRect.new(); joystick_knob.color=GOLD; joystick_knob.size=Vector2(32,32); joystick_knob.position=Vector2(59,59); joystick_knob.mouse_filter=Control.MOUSE_FILTER_IGNORE; joystick_pad.add_child(joystick_knob)
	joystick_pad.gui_input.connect(func(event: InputEvent):
		if event is InputEventScreenTouch:
			if event.pressed and joystick_finger==-1: joystick_finger=event.index; set_stick(event.position)
			elif not event.pressed and event.index==joystick_finger: reset_stick()
		elif event is InputEventScreenDrag and event.index==joystick_finger: set_stick(event.position)
		elif event is InputEventMouseButton and event.button_index==MOUSE_BUTTON_LEFT:
			if event.pressed: joystick_finger=-2; set_stick(event.position)
			else: reset_stick()
		elif event is InputEventMouseMotion and joystick_finger==-2: set_stick(event.position)
	)

func set_stick(pos: Vector2) -> void:
	var vector: Vector2=((pos-Vector2(75,75))/55).limit_length(1)
	world.joystick=vector; joystick_knob.position=Vector2(59,59)+vector*45

func reset_stick() -> void:
	joystick_finger=-1; world.joystick=Vector2.ZERO
	if is_instance_valid(joystick_knob): joystick_knob.position=Vector2(59,59)

func _input(event: InputEvent) -> void:
	if event is InputEventScreenTouch and not event.pressed and event.index==joystick_finger: reset_stick()
	if event is InputEventMouseButton and not event.pressed and joystick_finger==-2: reset_stick()
	if event.is_action_pressed("ui_cancel") and not Life.state.is_empty():
		if is_instance_valid(modal): clear_modal()
		else: show_settings()

func _process(delta: float) -> void:
	if Life.state.is_empty() or world.paused: return
	save_clock+=delta
	if save_clock>8:
		save_clock=0; Life.state.position=[world.avatar.position.x,world.avatar.position.z]; Life.commit()

func _notification(what: int) -> void:
	if what==NOTIFICATION_APPLICATION_PAUSED or what==NOTIFICATION_APPLICATION_FOCUS_OUT:
		reset_stick()
		if not Life.state.is_empty() and is_instance_valid(world):
			Life.state.position=[world.avatar.position.x,world.avatar.position.z]; Life.commit()
