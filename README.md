# F4C_Opossum_6748
NUS Orbital Project for AY 25/26
Milestone 1
<img width="1919" height="1140" alt="image" src="https://github.com/user-attachments/assets/7e4a0241-355d-45d4-ad0f-4d8ded555110" />

## [DEMO](https://filbertabulate.github.io/F4C_Opossum_6748/)

F4C Opossum is a classic 2D side-scrolling strategy game inspired by the mechanics of Age of War. Players are challenged to balance resource economies, spawn tactical units, and manage base defenses to conquer a dynamic enemy AI. 

## Milestone 1: Proof of Concept
Our first milestone establishes the fundamental physics, rendering, and combat engines required for the core game loop. 

**Current Features Implemented:**
* **Basic 2D Engine Setup:** Configured the Unity Universal Render Pipeline (URP) and initialized the 2D workspace with layered backgrounds and environmental rendering.
* **Entity Spawning System:** Implemented dynamic instantiation for player units, allowing units to be deployed onto the battlefield.
* **Base Defense & HP System:** Built a robust health tracking system for both the Player and Enemy towers. 
* **Combat & Collision Logic:** Units successfully march, detect opposing structures using Raycasting/Colliders, and execute attack loops that dynamically reduce the target's HP.
* **UI Integration:** Decoupled UI elements displaying real-time visual health bars that update dynamically as bases take damage.

## Technical Stack
* **Engine:** Unity (C#) - Chosen for high-performance 2D game loops and physics.
* **Art & Animation:** Custom assets and sprite implementations. 
* **Version Control:** Git & GitHub with feature-branching.

## How to Run Locally
1. Clone this repository: `git clone https://github.com/Filbertabulate/F4C_Opossum_6748.git`
2. Open **Unity Hub** and select *Add project from disk*.
3. Navigate to the cloned folder and open it (requires Unity 6 / 6000.x or newer).
4. In the Project window, search for `t:Scene` and open `SampleScene`.
5. Press the **Play** button at the top of the editor.
