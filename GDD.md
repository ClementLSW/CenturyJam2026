# FedUp — Game Design Document
### Outline v0.1

---

## 1. Project Overview

### 1.1 Concept Summary
- **High-Level Pitch:** FedUp is a Casual-Competitive Party-Style Puzzle game where 4 players have to finish packing a truck before it has to leave for its delivery route. Players will collaborate with each other to stuff and send it off early, but will compete to have most of their packages delivered.
- **Core Loop:** Pack a shared delivery truck grid with Tetris-Style boxes as fast and efficiently as possible across 3 one-minute delivery rounds.
- **Tone and Visual Identity:** frantic, colorful, lighthearted logistics chaos

### 1.2 Design Pillars
- **Collaborative-Competitive Pressure:** Players share one truck but act independently — shared mechanics like packing efficiency and early dispatch raise everyone's score, while individual mechanics like the color-ratio split create competition within that cooperation.
- **Readable Chaos:** Because 4 players act on the same grid simultaneously, the visual and audio layer must clearly disambiguate ownership, intent, and outcome for every action.
- **Reward Collaboration:** Deliberate coordination should always produce a better result than uncoordinated play, without making coordination mandatory. The early dispatch bonus and the 10% emptiness buffer are the primary expressions of this: teams that communicate are rewarded, but teams that don't can still have a satisfying match.
- **Every Player Counts:** No player should feel like a passenger — the color-ratio split and the 3-choice queue ensure every player has meaningful agency and visible reward regardless of their pace or the shapes they are dealt.

---

## 2. Core Gameplay Systems

### 2.1 The Truck Grid
##### General Information
- **Grid Size:** 6 × 4 (24 total squares) [Update Grid Size based on Playtesting]
##### Dev Context
- **Grid Coordinate System:** Each grid square is treated as an individual cell. When boxes covering multiple grid squares are placed, each individual grid cell should be able to independently return whether it is filled.
- **Cell Class**
  - `int cellId`
  - `bool cellFilled`
- **Box Placement Process**
  - Identify all *cellId* that placement will happen
  - Check each cell for whether *cellFilled*
  - If all checks return false, place the box, otherwise fail placement and return box to conveyor belt.
- **Race Conditions:** To avoid a race condition where two checks are concurrently running, resulting in a false-positive allowance for placement, all placement actions must be queued based on the principle of first-come-first-serve.
- ***Undo/Removing Boxes after a successful placement is not allowed***
##### Art Context
Refer to the following image for the core UI design.
[Insert UI Layout Image]
The grid will generally remain unchanging and non-interactive, and as such will not require states.

### 2.2 Boxes
##### General Information
The following shape variants of boxes exists:
- ⠁ (1)
- ⠃ (12)
- ⠇ (123)
- ⠋ (124)
- ⠏ (1234)
  - ⠹ (1456, Flip of 1234)
- ⠗ (1235)
- ⠛ (1245)
- ⠳ (1256)
  - ⠞ (2345, Flip of 1256)
For more information on how these are named refer to [Unicode Braille](https://www.unicode.org/charts/nameslist/n_2800.html)
##### Dev Context
- **Rotation:** Rotation will be handled within programmatically. A button press will rotate the shape positively (0°, 90°, 180°, 270°)
- **Color:** Color will be assigned through tinting in engine.
##### Art Context
- **Sprites Required:** Each of the above shape variants requires a unique sprite, including the flipped versions.
- **Sprite Colors:** Sprites should be drawn in greyscale. There should not be any tint in order for the sprites to be re-used for all four player colors.

### 2.3 Player Conveyor Belt
 
##### General Information
- **Queue Depth:** Each player has a personal conveyor belt displaying 3 upcoming boxes at all times.
- **Box Selection:** The player may pick any one of the 3 boxes freely to place next.
- **Queue Replenishment:** A selected slot is only replaced once the chosen box is successfully placed on the grid. A failed placement returns the box to its original slot; the queue does not advance.
- **Ownership:** Each conveyor belt is color-coded to its owning player and is visible but not accessible to others. This is explained more under *Controls*
 
##### Dev Context
- **Queue State:** Each conveyor belt maintains an array of 3 box slots. Each slot stores a shape type and tracks whether it is currently "in use" (picked up by the player but not yet placed).
- **Replenishment Trigger:** A new box is generated and inserted into a slot only on receipt of a successful placement confirmation from the grid. Failed placement emits no replenishment event.
- **RNG:** Replenished boxes are generated through a random pick of the pool of existing box shape variants. [Potential to add weighted replenishment based on the cumulative average of the last 5 boxes. E.g. The last five boxes have been larger boxes with a larger grid footprint. The next box is more likely to be a smaller box.]
 
##### Art Context
- **Conveyor Belt UI:** Only greyscale sprites needed, to be tinted in engine.
- **States Required per Slot**
  - Idle (Box on Belt)
  - Conveyor Belt Moving (Replenishing Box)
- **Dispatch Button:** The dispatch button is visually attached to the conveyor belt unit. Refer to §2.6 for button states.
 
### 2.4 Placement Flow
 
##### General Information
- The player selects a box from their conveyor belt, positions and rotates it over the grid, then confirms placement.
- A ghost/preview of the box is shown on the grid while the player is positioning.
- On confirmation: if the position is legal, the box locks into the grid and the conveyor slot replenishes. If illegal, the box is returned to its slot with no penalty.
 
##### Dev Context
- **Input Sequence:** Select box → move/rotate (grid-snapped) → confirm → trigger placement validation (see §2.1).
- **Ghost Rendering:** Each player’s ghost box is always displayed as a 25% opacity floater. When overlapped the colors simply layer over each other. [Insert Reference Overlap Photo]
- **Placement Result Events:** Emit `placement_success(player_id, cells[])` or `placement_fail(player_id)` to drive queue replenishment, scoring, and audio/visual feedback respectively.
 
### 2.5 Round Structure
 
##### General Information
- **Rounds per Match:** 3 rounds total, one truck per round.
- **Round Timer:** 60 seconds per round. The timer begins as soon as the truck arrives.
- **Round End:** The round ends when the timer reaches 0 (normal dispatch) or all players trigger early dispatch (see §2.6). The truck departs immediately on either trigger.
- **Inter-Round:** The next truck arrives immediately after departure. A brief per-round score recap is shown during the transition.
 
##### Dev Context
- **End Trigger State Machine:** Two valid end states — `TIMER_EXPIRED` and `EARLY_DISPATCH`. Both routes must produce identical downstream behavior (scoring, truck departure animation, inter-round transition).
- **Score Snapshot:** No further actions are allowed once dispatch is triggered. Score is calculated based on the exact state of the grid on dispatch trigger.
- **Inter-Round Transition:** Each player’s conveyor belt also has a counter displaying their scores. This score is updated during the transition.
 
##### Art Context
- **Timer Display:** Prominent, always visible. Should enter a visual urgency state (e.g., color change, pulse) at ≤ 10 seconds remaining.
- **Truck Departure:** An animated warning sign/scroller appears from the top of the frame to indicate clearly when the truck is departing. //Refer to §6.3. [To Remove §6.3 if not relevant]
 
### 2.6 Early Dispatch System
 
##### General Information
- Each player has a dedicated Dispatch button beside their conveyor belt.
- All 4 players must hold their button simultaneously to trigger an early dispatch.
- If any player releases before all 4 are holding, the dispatch is cancelled and the round continues.
- The dispatch bonus is calculated from the whole seconds remaining at the moment of dispatch (see §3.3).
 
##### Dev Context
- **Dispatch State:** Maintain a per-player boolean `isHolding`. Early dispatch fires when all 4 are `true` simultaneously.
- **Hold Threshold:** Allow of a minimum hold threshold, e.g. all buttons must be held for at least 500ms before triggering. To prevent accidental trigger. This value can be set to 0ms initially, but retain the buffer for correction if playtesting reveals consistent accidental triggers.
 
##### Art Context
- **Button States Required:** Idle, held (by this player), all-held (all 4 pressing — pre-dispatch confirmation), triggered.
- **Global Indicator:** Four dots below the timer, with a on-off state which will turn on based on the total number of buttons held at the point in time.
- **Audio:** 4 distinct tones in rising scale. Whenever a button is pressed, the max number of buttons held determines the tone which is played.
 
---
 
## 3. Scoring System
 
### 3.1 Scoring Overview
 
##### General Information
Scoring is resolved at the end of each round in the following order:
1. **Base Score** — fixed maximum per round (value TBD, e.g. 1000 pts)
2. **Emptiness Penalty** — reduces base score based on unfilled grid squares
3. **Early Dispatch Bonus** — increases score if players dispatched the truck before the timer expired
4. **Color Ratio Split** — divides the final adjusted score among players proportional to how many grid squares each filled
 
### 3.2 Emptiness Penalty
 
##### General Information
- A buffer of 10% of total grid squares is allowed before any penalty is applied.
- Beyond the buffer, the penalty scales linearly with the number of excess empty squares.
- **Formula:**
  ```
  buffer       = floor(0.10 × total_grid_squares)
  excess_empty = max(empty_squares − buffer, 0)
  penalty_%    = (excess_empty / total_grid_squares) × 100%
  ```
- **Example (6×4 grid, 24 squares):**
  - Buffer = floor(2.4) = 2 squares
  - If 5 squares are empty: excess = 3, penalty = 3/24 × 100% ≈ 12.5%
  - Score after penalty = base_score × (1 − penalty_%)
 
##### Dev Context
- Computed from the grid state snapshot taken at the moment of dispatch (see §2.5).
- Empty squares = cells where `cellFilled` is false at snapshot time.
 
### 3.3 Early Dispatch Bonus
 
##### General Information
- No bonus is applied if the truck departs at the 60-second mark.
- For every second saved, a bonus is applied to the post-penalty score.
- **Formula:**
  ```
  bonus_% = floor(seconds_remaining) / 60 × 50%
  ```
 
##### Dev Context
- The bonus is applied to the shared pool before the color ratio split — all players benefit equally from early dispatch.
 
### 3.4 Color Ratio Split
 
##### General Information
- Each player's share of the final adjusted score is proportional to the number of grid squares they filled.
- A player who fills 0 squares receives 0 points for that round.
- **Example (4 players, 20 filled squares — ratio 3:5:4:8):**
  - Player 1 → 3/20 = 15%
  - Player 2 → 5/20 = 25%
  - Player 3 → 4/20 = 20%
  - Player 4 → 8/20 = 40%
 
##### Dev Context
- Color ownership is tracked per cell: each `cellId` stores the `player_id` of the placer on successful placement.
 
### 3.5 Match Scoring
 
##### General Information
- Per-round scores accumulate across all 3 rounds into a match total.
- A final leaderboard is displayed at match end, alongside a per-round breakdown.
- [Tie-breaking rule TBD (e.g., most boxes placed, highest single-round score)]
 
##### Dev Context
- Maintain a per-player cumulative score ledger updated at the end of each round.
- Results payload must include: per-round base score, penalty applied, bonus applied, ratio split, and cumulative total — for results screen rendering.
 
## 4. Game Modes & Session Flow
 
### 4.1 Match Flow
 
##### General Information
- Lobby / player join (For Game Jam purposes, assume full 4 players with no potential to disconnect) → player fixed color assignment based on order of joining → optional tutorial/control reminder (skippable)
- Round 1 → inter-round recap → Round 2 → inter-round recap → Round 3 → final results screen
- Final results screen shows match totals and per-round breakdown
- Rematch and main menu options available from the results screen
 
##### Art Context
- **Color Assignment Screen:** Display all 4 player colors clearly; colors must be high-contrast and distinguishable.
- **Results Screen:** Requires layouts for per-round recap (shown mid-match) and full match summary (shown at end).
 
### 4.2 Planned Modes
 
##### General Information
- **Local Party:** Shared screen, split controls, all 4 players on one device.
 
### 4.3 Session Length
 
##### General Information
- **Target:** ~4-5 minutes per match (3 × 60 s rounds + transitions).
- Minimal friction from lobby to gameplay is a priority — players should be packing within 60 seconds of launching a session.
 
---
 
## 5. Controls & Input
 
### 5.1 Input Scheme
 
##### General Information
- **Primary:** Gamepad. **Fallback:** Keyboard
- Local multiplayer must support simultaneous independent inputs from all 4 players.
- **Required Actions:**
  - Browse / select a box from the conveyor belt
  - Move box over grid (grid-snapped)
  - Rotate box (clockwise)
  - Confirm placement
  - Hold dispatch button
 
##### Dev Context
- Each player's input must be isolated — no shared input state between players.
- [Input remapping should be supported for accessibility.]
 
### 5.2 Input Edge Cases
 
##### Dev Context
- **Simultaneous Placement Conflict:** Two players confirm placement on overlapping cells in the same frame. Resolved by the first-come-first-serve queue on the grid (see §2.1); the losing player's box is returned to their conveyor belt.
 
---
 
## 6. Art & Visual Design
 
### 6.1 Visual Style
 
##### General Information
- **Art Direction:** Bold, flat-ish cartoon aesthetic; warm and saturated palette.
- **Readability First:** Player colors must be distinguishable at small grid sizes and in peripheral vision. All 4 player colors must pass contrast checks against the grid background.
 
##### Art Context
- Define the 4 player colors early; all other color decisions (grid, UI, truck) should be made with these as fixed constraints.
 
### 6.2 Key Art Assets
 
##### Art Context
- Truck exterior (arrival / departure animations) and interior (the grid view)
- Grid background; cell states: empty, filled (per player color), ghost/preview overlay
- Box shape sprites — one greyscale sprite per shape variant and flip (see §2.2); tinted in engine per player
- Conveyor belt unit: queue slots, player color trim, dispatch button (all states)
- Round timer: standard and urgency (≤ 10 s) states
- Dispatch global indicator: idle, 1–3 players holding, all holding, triggered
- Per-round recap overlay and final results screen
 
### 6.3 Animations & Juice
 
##### Art Context
- **Box Placement (Success):** Drop and lock snap animation
- **Box Placement (Failure):** Shake and return to conveyor belt
- **Truck Departure (Normal & Early Dispatch):** Neutral drive-away
- **Conveyor Belt:** Scroll animation when a slot replenishes
- **Score Tally:** Animated count-up on the results screen
 
### 6.4 UI / HUD Layout
 
##### Art Context
- **Shared Truck Grid:** Centre of screen; primary visual focus
- **Player Conveyor Belts:** Screen edges or corners; 4-player layout TBD [Insert UI Design]
- **Round Timer:** Prominent, centre-top
- **Dispatch Global Indicator:** Always visible; positioned near the grid so all players can see it without looking away
- **Live Score Display:** Optional — evaluate during playtesting whether this adds useful information or visual noise
 
---
 
## 7. Audio Design
 
### 7.1 Sound Effects
 
##### Art Context
- Box: pick-up, move (grid snap), rotate, place success, place failure
- Conveyor belt: slot replenishment scroll
- Dispatch button: press, release, all-held confirm, dispatch triggered, dispatch cancelled
- Truck: departure (normal variant), departure (early dispatch variant)
- Round: start sting, end sting
- Scoring: score tally blips on results screen
 
### 7.2 Music
 
##### Art Context
- **Main Loop:** Upbeat, loopable background track that escalates in energy as the timer drops
- **Urgency Variant:** Kicks in at ≤ 10 seconds remaining
- **Results Jingle:** Short celebratory or neutral sting for the results screen
 
### 7.3 Voice / Personality
 
##### General Information
- Optional delivery-themed announcer barks (e.g., "Truck's here!", "Last 10 seconds!", "Dispatched early!").
 
---
 
## 8. Technical Systems Design
 
### 8.1 Grid State Management
 
##### Dev Context
- Cell data model: `cell[x][y] = { cellId: int, cellFilled: bool, playerId: int | null }`
- All placement actions queued in a list on a first-come-first-serve basis to prevent race conditions (see §2.1).
 
### 8.2 Shape & Placement Logic
 
##### Dev Context
- Shape definitions stored as offset arrays per rotation state (0°, 90°, 180°, 270°).
- Placement validation checks all target cells for `cellFilled` before committing.
 
### 8.3 Timer & Dispatch
 
##### Dev Context
- Dispatch state machine: `{ player_id: isHolding bool }` — fires when all 4 are `true`.
 
### 8.5 Scoring Engine
 
##### Dev Context
- All scoring formulas (§3.2–3.4) implemented as pure functions; unit-testable in isolation [We don’t test].
- Inputs: grid snapshot, `seconds_remaining` at dispatch, per-cell `playerId` ownership map.
- Outputs: per-player round score, cumulative match score, full breakdown for results screen.
 
---

## 9. Retention — Blocked Cells
 
### 9.1 Concept
 
##### General Information
- At the start of each round, a random number of grid cells are pre-filled and blocked out before any player places a box.
- Blocked cells are visually distinct from empty cells and from player-filled cells. They cannot be placed on and cannot be removed.
- The intention is to introduce a curveball each round — players cannot rely on a fully empty truck and must adapt their placement strategy to the current layout.
 
### 9.2 Scoring Interaction
 
##### General Information
- Blocked cells are excluded from the total available slots used in the emptiness penalty calculation.
- Effective grid = total cells − blocked cells. All penalty and ratio calculations operate on this adjusted total.
- Blocked cells do not contribute to any player's color ratio, as no player placed them.
 
##### Dev Context
- The scoring engine receives the blocked cell count as an input and subtracts it from `total_grid_squares` before applying §3.2 and §3.4 formulas.
- Blocked cells are flagged via `isBlocked: true` in the cell data model (see §8.1) and are excluded from the `playerId` ownership map.
 
### 9.3 Blocked Cell Generation
 
##### General Information
- Blocked cells are randomly chosen based off a set of pre-designed templates.
 
##### Dev Context
- Blocked at round initialization before players receive control. The blocked layout is fixed for the duration of that round.
 
##### Art Context
- Blocked cells require a distinct visual treatment — clearly not empty, and clearly not belonging to any player.
- Suggested treatment: a darker or hatched fill with a visual indicator (e.g. a crate or obstruction icon) to communicate that the cell is pre-occupied and immovable.
- Blocked cells do not animate on placement or dispatch.
 
---
 
## 10. Development Milestones
 
### 10.1 Milestone 0 — Prototype
- Single-player local grid with shape placement and collision validation
- Hardcoded 3-box queue with manual cycling (no RNG)
- Basic 60-second round timer
- Scoring formulas implemented and logged to console
 
### 10.2 Milestone 1 — Core Loop Playable
- 4-player local support with 4 independent conveyor belts (all visible)
- RNG box queue replenishment per player
- Full placement flow: select → position → rotate → confirm → success/fail feedback
- Early dispatch button mechanic with 4-dot global indicator and rising tone audio
- 3-round match structure with inter-round transition (no art yet)
- Scoring pipeline producing correct per-player output
 
### 10.3 Milestone 2 — Blocked Cells & Balance
- Blocked cell generation at round start with configurable range
- Scoring engine updated to exclude blocked cells from calculations
- Blocked cell visual treatment (placeholder art)
- First full playtesting pass: grid size, shape pool, blocked cell count, timer feel, dispatch threshold
 
### 10.4 Milestone 3 — Art & Audio Pass
- Full sprite set for all box shapes (greyscale, tinted in engine)
- Conveyor belt UI with idle and replenishing states
- Box placement success/fail animations
- Truck departure animation
- Dispatch button states and indicator
- Timer urgency state (≤ 10 s)
- SFX pass: all events in §7.1
- Music: main loop, urgency variant, results jingle
- Results screen and per-round recap layouts
 
### 10.5 Milestone 4 — Polish & Game Jam Submission
- HUD readability pass against all 4 player colors simultaneously
- Input mapping finalized for 4 local controllers + keyboard fallback
- Stretch goal: announcer barks
- Stretch goal: celebratory early dispatch truck departure
- Final playtesting and balance tuning (shape weights, blocked cell range, scoring constants)
- Build packaged and submitted
 
---
 
## 11. Open Design Questions
 
| # | Question | Priority | Notes |
|---|----------|----------|-------|
| 1 | Final grid size (6×4 baseline vs. alternatives) | High | Affects all balance and blocked cell calculations |
| 2 | Early dispatch bonus: additive or multiplicative? | High | Changes feel of the dispatch reward |
| 3 | Base score per round | High | Sets scale for all other values |
| 4 | Blocked cell count range per round | High | Too few = no impact; too many = frustrating |
| 5 | Blocked cell generation constraints (avoid unreachable regions?) | Medium | Evaluate during playtesting |
| 6 | Shape rotation: snap-to-90° only, or free analogue? | Medium | UX and time-pressure implications |
| 7 | Dispatch hold threshold duration | Medium | Start at 0 ms; tune upward if accidental triggers occur |
| 8 | Color-blind accessibility: patterns or icons on boxes? | Medium | Player color differentiation is core to scoring |
| 9 | Live score display in HUD: include or omit? | Low | May add useful info or visual noise — evaluate in playtesting |
| 10 | Tie-breaking rule on final leaderboard | Low | Needs definition before results screen is built |
| 11 | Announcer voice lines | Low | Stretch goal |
| 12 | Celebratory early dispatch truck animation | Low | Stretch goal |
 
---
 
*End of Outline — FedUp GDD v0.1*