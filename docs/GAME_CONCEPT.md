# Game Concept

## 1. What Game Is This?

A 2D top-down real-time survival action game.
Survive as long as possible. No stage clear; every run restarts.

## 2. Core Loop

1. Enemies continuously spawn and approach.
2. Screen density and pressure rise.
3. Player survives via movement, attack, and dodge.
4. At high pressure, game pauses for one growth choice.
5. Pick one, then continue at faster tempo.
6. Death -> instant restart.

## 3. Player

- Visual: simple circle, no narrative setup.
- HP: fixed 3.
- Hit: lose 1 HP per hit.
- No passive regen (only via run upgrades).

## 4. Controls

- Move: WASD / left stick.
- Aim direction: mouse / right stick.
- Three action families:
  - Primary attack (rhythm)
  - Secondary action (high impact, longer cooldown)
  - Mobility/dodge

## 5. Combat Feel

- Fast and clear feedback.
- Kill feedback: slight zoom, brief pause, light burst.
- No combo system, no score pop-up dependency.

## 6. Enemy Philosophy

- Geometric enemies, readable behavior.
- Pressure comes from count, proximity, and reduced movement space.

## 7. Pressure Upgrade Moment

Hidden pressure rises from:
- Enemy count
- Low HP
- Long no-kill windows
- Space compression

At threshold:
1. Full pause
2. Clean screen
3. Two choices
4. Pick one (or random fallback)
5. Resume at faster tempo

## 8. Growth Rules

- In-run only; resets next run.
- Must be immediately noticeable after choice.

## 9. Visual Direction

- Geometric, high saturation, heavy bloom.
- Dark background, bright enemies, brightest attacks.
- Keep primary color count limited for readability.
