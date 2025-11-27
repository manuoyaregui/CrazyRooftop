# Game Design Document: CrazyRooftop

## 1. Overview
**Title:** CrazyRooftop  
**Genre:** 3D Endless Runner / Parkour  
**Platform:** PC (Windows)  
**Engine:** Unity 6  
**Target Audience:** Fans of fast-paced arcade games, parkour enthusiasts, synthwave lovers.

### Elevator Pitch
*CrazyRooftop* is a high-octane 3D endless runner where players traverse a procedurally generated synthwave city. Using fluid parkour moves like sliding and jumping, players must navigate rooftops, launch off platforms, and maintain momentum to outrun a digital "Glitch" that consumes everything in its path.

---

## 2. Gameplay

### Core Loop
1.  **The Run:** Player runs automatically (or with forward input) across rooftops.
2.  **The Flow:** Perform parkour moves (Slide, Jump, Wallrun, Vault) to build "Flow".
3.  **The Threat:** Maintain speed to avoid "The Glitch" (a pursuing wall of static) and don't fall into the Void.
4.  **The Transition:** Reach "Data Gates" to warp to new Districts (Stages) with different visuals and difficulty.
5.  **The End:** Death resets the run. Score is tallied based on Distance x Flow.

### Controls
*   **Movement:** WASD / Left Stick (Strafing & Forward Speed Control)
*   **Jump:** Space / Button South
*   **Crouch / Slide:** Ctrl / Button East
*   **Camera:** Mouse / Right Stick

### Mechanics

#### Movement & Physics
*   **Kinematic Character Controller (KCC):** Precise, fluid movement.
*   **Momentum:** Speed is life. Hitting obstacles slows you down; parkour moves maintain or boost speed.
*   **Air Control:** Adjustable air acceleration and drag.

#### Parkour Actions
*   **Jumping:** Scalable forward speed. Coyote time for fairness.
*   **Sliding:**
    *   **Boost:** Speed boost upon initiating a slide.
    *   **Slope Physics:** Unlimited speed gain on slopes.
    *   **Kick:** Sliding into small objects (Mass <= 30) propels them away (Physics interaction).
*   **Launch Platforms:** Physics-based jump pads for verticality.

#### Progression: The "Flow & Combo" System
*   **Flow Meter:** A dynamic bar that acts as your score multiplier and speed buffer.
    *   **Fills:** By performing "Combos" and maintaining high speed.
    *   **Decays:** Slowly over time, or instantly upon hitting static obstacles.
*   **Micro-Combos (Simple & Contextual):**
    *   Instead of complex button combinations (like fighting games), combos are about **timing and chaining movement states**.
    *   **Slide-Jump:** Jumping *during* a slide gives extra forward velocity and height.
    *   **Perfect Landing:** Pressing *Slide* just before touching the ground after a long fall prevents speed loss (no "hard landing" animation) and boosts Flow.
    *   **Strike:** Sliding into a physics object (Kick) and knocking it off the roof grants a "Destruction Bonus".
    *   **Launch-Chain:** Hitting a Launch Platform and performing a Perfect Landing on the target roof.
*   **Infinite Scaling:**
    *   **Speed:** Base running speed increases slightly with every District cleared.
    *   **Complexity:** Gaps get wider, platforms get smaller, moving obstacles appear.

#### Death Conditions
1.  **The Void:** Falling off the building.
2.  **The Agent Swarm (Men in Black):**
    *   **Visuals:** Humans in black suits and sunglasses. Serious but clumsy.
    *   **Chaotic Interception:** To ensure you see the chaos in First Person, agents don't just chase from behind. They **drop from helicopters**, **burst through doors ahead of you**, or **try to cut you off**.
    *   **Visible Clumsiness:** You'll see them trip over vents *in front of you*, miss jumps *ahead*, or crash into each other while trying to block your path.
    *   **Audio:** You'll hear the chaos behind you (crashes, Wilhelm screams) even if you don't see it.
    *   **Capture:** Being surrounded or grabbed by the swarm slows you down until you are overwhelmed (Game Over).

#### Combat & Interaction
*   **Slide Kick:** The player's slide is a weapon.
    *   **Mechanic:** Sliding into an Agent (physics interaction) knocks them back.
    *   **Ragdoll:** Kicked agents turn into ragdolls, flying off the roof or tripping other agents behind them (bowling pin effect).
    *   **Strategy:** Use the kick to clear a path through a group of agents blocking your way.

#### Scoring
*   **Base Score:** Distance traveled.
*   **Multiplier:** Current Flow Level.
*   **Style Points:** Bonus for specific tricks (e.g., "Slide Kick" an object into the void).

---

## 3. World & Environment

### Setting
*   **Theme:** Synthwave / Cyberpunk Simulation.
*   **Atmosphere:** Neon lights, dark skies, retro-futuristic aesthetic.

### Level Generation (Districts)
The city is divided into "Districts" (Stages). Passing a Data Gate transitions to the next District.
1.  **Neon Downtown:** Standard rooftops, billboards, AC units.
2.  **Industrial Zone:** Pipes, steam, moving conveyor belts.
3.  **Cyber Slums:** Narrow paths, more verticality.
4.  **The Cloud:** Floating platforms, high risk of falling.

---

## 4. Characters
### Player
*   **Abilities:** Parkour expert.
*   **Visuals:** Cyber-athlete with emissive clothing that changes color with Flow level.

---

## 5. Art & Audio
### Art Style
*   **Visuals:** Low-poly or stylized high-fidelity.
*   **Colors:** Neon Pink, Purple, Cyan, Deep Blue.
*   **Effects:** Trails, motion blur, chromatic aberration.

### Audio
*   **Music:** Synthwave / Retrowave soundtrack. Dynamic mixing (music gets more intense with Flow).
*   **SFX:** Footsteps, slide scrapes, jump whooshes, neon hums, "Glitch" static noise.

---

## 6. Technical
*   **Physics:** Unity Physics + KCC.
*   **Input:** Unity Input System.
*   **Architecture:** State Machine for Player Controller (Default, Sliding).
