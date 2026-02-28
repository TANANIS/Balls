# Stage Event Background
Last Synced: 2026-02-28

## Scope
- This document defines narrative background and presentation beats for each run stage.
- This is a narrative and UX copy spec only; it does not introduce new runtime systems.
- Canonical phase spike markers remain stage-tail miniboss spawns.
- Mechanical scheduling rules are owned by `docs/EVENT_SCHEDULING_META_CONTAINMENT_V0_3.md`.

## Timeline Contract (Canonical)
- `00:00-03:45`: Stage 1 (`Ramp-In`)
- `03:45-07:30`: Stage 2 (`First Stress Cycle`)
- `07:30-11:15`: Stage 3 (`Build Check`)
- `11:15-15:00`: Stage 4 (`Final Climb`)
- Tail miniboss markers: `03:45`, `07:30`, `11:15`, `14:30~15:00`.

## Run Narrative Arc
- The battlefield is an old ward-zone built to seal an abyssal fracture.
- Each stage represents one more layer of that seal breaking.
- A final fragmented `Order` deity temporarily anchors causality and lets the player pre-schedule event slots.
- The player is not "saving the world in one run"; the run goal is to survive and delay collapse to `15:00`.

## Stage 1 - Ramp-In (`00:00-03:45`)
### Story Role
- Name: `Outer Ward Perimeter`
- Background: the outer seal still holds, but scouting packs test weak points.
- Player fantasy: establish rhythm and read enemy patterns before true pressure begins.

### Event Beats
- `00:00`: Ward lanterns are lit; the field looks stable but tense.
- `01:45`: Minor cracks appear and enemy frequency visibly increases.
- `03:15`: A focused breach signal warns of a command-unit approach.
- `03:45`: Tail miniboss arrival (`Lancer_Stage1`) through the first breach lane.

### Presentation Direction
- Palette mood: more grass/earth visibility, lower threat saturation.
- Audio mood: restrained percussion, clear space between hit cues.
- Camera/VFX note: keep peak effects low so first miniboss impact reads as a step-up.

### Banner Copy Candidates
- EN: `Outer Ward Breach`
- zh_TW: `外圍結界破口`

## Stage 2 - First Stress Cycle (`03:45-07:30`)
### Story Role
- Name: `Ash Bell Corridor`
- Background: warning bells once used for evacuation now ring as distortion amplifiers.
- Player fantasy: transition from comfort to deliberate route planning.

### Event Beats
- `03:45`: Stage opens after first lancer pressure release.
- `05:30`: Distortion pulses shorten safe movement windows.
- `06:50`: Bell resonance peaks and ranged threats stack with melee chasers.
- `07:30`: Tail miniboss arrival (`Lancer_Stage2`) with tighter pack support.

### Presentation Direction
- Palette mood: warmer warning tones mixed into neutral terrain.
- Audio mood: layered bells and short alarm motifs under combat rhythm.
- Readability note: keep lane telegraphing clear; difficulty should come from density, not ambiguity.

### Banner Copy Candidates
- EN: `Bell Resonance Rising`
- zh_TW: `警鐘共鳴上升`

## Stage 3 - Build Check (`07:30-11:15`)
### Story Role
- Name: `Fractured Cloister`
- Background: inner walls are split; enemy flow becomes organized around exposed fault lines.
- Player fantasy: build check point where upgrade choices must show clear value.

### Event Beats
- `07:30`: Stage starts with a brief pressure reset, then a faster climb.
- `09:00`: Fault-line eruptions imply unstable traversal corridors.
- `10:30`: Containment glyphs fail; elite pressure cadence becomes noticeable.
- `11:15`: Tail miniboss arrival (`Lancer_Stage3`) as the cloister loses integrity.

### Presentation Direction
- Palette mood: higher contrast terrain scars and stronger threat highlights.
- Audio mood: denser low-end and rhythmic tension, less silence between pulses.
- UX note: stage messaging should emphasize "build validation", not pure panic.

### Banner Copy Candidates
- EN: `Containment Failure`
- zh_TW: `封鎖失效`

## Stage 4 - Final Climb (`11:15-15:00`)
### Story Role
- Name: `Abyss Gate Approach`
- Background: only the core gate remains; the run is now a survival holdout.
- Player fantasy: sustained maximum pressure and final execution test.

### Event Beats
- `11:15`: Stage opens at the highest baseline threat so far.
- `12:45`: Core-gate pressure waves reduce recovery space between engagements.
- `14:30`: Final breach warning; field enters terminal state.
- `14:30~15:00`: Tail miniboss arrival (`Lancer_Stage4`) in the run-tail window.
- `15:00`: If player survives, collapse is delayed and run is marked clear.

### Presentation Direction
- Palette mood: darker terrain values and strong warm threat accents.
- Audio mood: sustained tension bed with minimal release until run end.
- HUD note: countdown visibility is critical; "time survived" is the final objective signal.

### Banner Copy Candidates
- EN: `Last Gate Under Siege`
- zh_TW: `終門遭圍攻`

## Implementation Notes (Non-Mechanic)
- Trigger source for stage copy should be stage transition timestamps already owned by director/stability systems.
- Tail warning banners should be short and high contrast; avoid long narrative paragraphs during combat.
- Keep localization keys centralized in `Data/Localization/UI.csv` when these banners are implemented.
- If future random events are reintroduced, this document should remain the base arc and only add optional overlays.
