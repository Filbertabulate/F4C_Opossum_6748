# F4C Opossum
![Project Banner](https://github.com/user-attachments/assets/7784b5c5-aa60-4aae-8fdf-343f42d4addb)

### NUS Orbital Project for AY 25/26 — Apollo 11

## :video_game: [PLAY THE GAME](https://filbertabulate.github.io/F4C_Opossum_6748/)

F4C Opossum is a classic 2D side-scrolling strategy and base-defence game inspired by the mechanics of **Age of War**. Players are challenged to balance their resources, deploy tactical units, construct defensive turrets, evolve through different eras, and use special abilities to destroy the enemy base before their own falls.

Built using **Unity and C#**, this project was developed as part of **NUS Orbital 2026**, with a focus on both game development and applying software engineering practices such as object-oriented programming (OOP), code refactoring, automated testing, and version control.

### :bar_chart: Game Statistics

Want to compare units, enemies, turrets, and abilities in more detail?

[**View the Unit, Enemy & Turret Stats →**](Final_Submission_Resources/Units_Statistics_Guide/Game_Statistics.md)

---

# :book: Project Resources

For a more detailed breakdown of the project's development, design decisions, testing, and implementation, refer to the resources below:

- :notebook: [Full README Report](https://drive.google.com/file/d/1TYmiiiV_vizJqK89_bU-nDQw_7TRR9Bt/view?usp=sharing)
- :framed_picture: [Project Poster](https://drive.google.com/file/d/1AYxXTOcizSNwgdR0DNb5BQjg6fUXYryB/view?usp=sharing)
- :film_strip: [Gameplay Video — Milestone 2](https://drive.google.com/file/d/1BHaVYKhp4yezxNoT85e6jFuCWHc53cf7/view?usp=sharing)
- :date: [Project Log](https://docs.google.com/spreadsheets/d/1UeVmOn5maOyk0yjt6T7f1WFaAEKY951M/edit?usp=sharing&ouid=107132362133645130135&rtpof=true&sd=true)

> **Note:** The current poster and gameplay video were created primarily for Milestone 2. The playable WebGL build and Full README Report reflect the newer Milestone 3 implementation, with additions from the Milestone 3 feedback.

---

# :video_game: Gameplay

The objective is simple:

> **Destroy the enemy base before your own base is destroyed.**

To achieve this, players must manage their resources and decide when to train units, strengthen their base with turrets, evolve to a stronger era, or use powerful special abilities.

![Main Gameplay Screenshot](https://github.com/user-attachments/assets/e13b7ce2-cc31-469f-b837-45973dcdcec5)

## :moneybag: Manage Your Resources

The game uses two main resources: **Gold** and **Experience (EXP)**.

**Gold** is earned passively and from defeating enemies. It is primarily used to train units and construct defensive turrets.

**EXP** is gained through combat and can be spent to evolve to the next era or activate the Meteor Strike special ability.

This means players must find the balance between short-term survival and long-term progression.

![Gold and EXP tracking Interface](https://github.com/user-attachments/assets/33f52fb1-b134-4d31-8e72-a167878cf5ad)

---

## :crossed_swords::bow_and_arrow::wrench: Train Your Army

Players can spend Gold to train different units using the action bar at the bottom of the screen.

Instead of spawning immediately, trained units enter a **production queue** before being deployed onto the battlefield. Once deployed, units automatically advance towards the enemy base and engage enemies they encounter.

Different unit types provide different strengths, including melee and ranged units.

![Unit Combat](https://github.com/user-attachments/assets/6b4db566-3ff7-4051-92a5-9d09c476acc8)

---

## :european_castle: Build Your Defences

Players can construct turrets to protect their base from approaching enemies.

Different turret types provide different offensive capabilities, while additional turret slots can be purchased as the game progresses.

Existing turrets can also be sold for a partial Gold refund (50% of the buying price), allowing players to change their defensive setup during a battle.

![Turret System](https://github.com/user-attachments/assets/ea0f4019-1e52-4f33-b999-09bf935986da)

---

## :japanese_ogre::arrow_right::european_castle: Evolve Through the Eras

By accumulating enough EXP, players can evolve their civilisation into the next era.

Evolution introduces:
- New units
- Stronger turrets
- Updated unit artwork
- Increased combat capabilities

The game's interface dynamically updates when the player's era changes, allowing the same action bar to represent the units and turrets available in the current era.

#### Era 1 UI Bar
![Era 1 Evolution UI](https://github.com/user-attachments/assets/9ab81bc5-9b7f-4f6e-8af1-05b94341a494)
#### Era 2 UI Bar
![Era 2 Evolution UI](https://github.com/user-attachments/assets/30374ad7-dc99-4d3b-bc0f-00d24049b60a)

---

## :comet::collision: Meteor Strike

Players can spend EXP to activate **Meteor Strike**, a powerful special ability that rains meteors across the battlefield and damages enemy units.

Because the same EXP is required for era progression, players must decide whether using Meteor Strike to survive the current battle is worth delaying their next evolution.

![Meteor Strike](https://github.com/user-attachments/assets/99d39b51-0bb7-47b1-803a-befc049fe2d7)


---

# :joystick: Controls

The game is designed primarily for desktop browsers and is controlled using the mouse.

| Action | Control | Visual Reference |
|-----|-----|-----|
| **Train Unit** | Left-click a unit button | <img src="https://github.com/user-attachments/assets/e81f7192-ecf0-48ad-9029-915226515f68" width="400" alt="Training Units Icon"> |
| **Build Turret Holder** | Left-click the Turret Holder button to purchase an additional turret slot | <img src="https://github.com/user-attachments/assets/d7126d23-8fa5-48a7-b410-d0421c5de7ba" height="180" alt="Build Holder Icon"> |
| **Build Turret** | Select a turret to enter Build Mode, then click an available turret slot | <img src="https://github.com/user-attachments/assets/9754e493-915a-4801-a73f-83f719eecac1" width="400" alt="Build Mode Screen"> |
| **Sell Turret** | Enter Sell Mode and select the turret to sell | <img src="https://github.com/user-attachments/assets/1f23ea19-2220-448d-972e-60730ce3adf9" width="400" alt="Sell Mode Screen"> |
| **Evolve Era** | Left-click the Evolve button | <img src="https://github.com/user-attachments/assets/69c7b900-62e6-44ba-8578-bc76707d2298" height="160" alt="Evolve Era Icon"> |
| **Meteor Strike** | Left-click the Meteor Strike button | <img src="https://github.com/user-attachments/assets/e071b198-7987-4914-a54e-841aef02e824" height="160" alt="Meteor Strike Icon"> |
| **Pause Game** | Left-click the Pause button | <img src="https://github.com/user-attachments/assets/7592646b-1a65-4427-b54f-b198b7b5e976" height="160" alt="Pause Button Icon"> |
| **Zoom In / Out** | Mouse scroll wheel |:computer_mouse: Scroll Wheel |
| **Move Camera Left** | Move the mouse cursor towards the left edge of the screen |:computer_mouse: &larr; Left Edge of the screen |
| **Move Camera Right** | Move the mouse cursor towards the right edge of the screen |:computer_mouse: &rarr; Right Edge of the screen |

### Overall Gameplay UI Controls
![Gameplay Controls](https://github.com/user-attachments/assets/78fe3533-bd44-4240-a5a2-af5e03f25cc9)

---

# :sparkles: Main Features

- **Unit Production System:** Train multiple unit types through a queue-based production system.
- **Automated Combat:** Units automatically move, identify enemies, and engage in melee or ranged combat.
- **Resource Economy:** Manage Gold and EXP earned passively and through combat.
- **Era Progression:** Spend EXP to advance through eras and unlock stronger units and defences.
- **Turret Management:** Purchase turret slots, construct different turret types, and sell existing turrets for partial refunds.
- **Meteor Strike:** Spend EXP to unleash a powerful special ability against enemy units across the battlefield.
- **Dynamic Gameplay UI:** Unit, turret, cost, and era information dynamically changes according to the current game state.
- **Enemy Progression:** Enemy units become increasingly challenging as the battle progresses, culminating in the release of a powerful boss unit.
- **Camera Controls:** Navigate across and zoom into the battlefield using the mouse.

---

# :test_tube: Software Engineering

Beyond implementing gameplay features, the project was also used to explore software engineering practices within game development.

### Object-Oriented Design & Refactoring

Gameplay responsibilities were reorganised into dedicated classes instead of concentrating unrelated behaviour within large scripts. This improved code readability and maintainability while making individual systems easier to test.

### :robot: Automated Testing

Automated tests were introduced across multiple levels, including:

- Unit testing
- Integration testing

Testing was used to validate key systems, including health and damage calculations, the resource economy, unit spawning, cooldowns, and interactions between gameplay components.

System testing was performed manually alongside gameplay balancing and refinement, as many full-game interactions were more effectively evaluated through playtesting.

### :balance_scale::link: Version Control

Development was managed with **Git and GitHub**, including feature branches, commits, merges, and collaborative development among team members.

For the complete software engineering discussion and testing documentation, see the [Full README Report](https://drive.google.com/file/d/1Ps6pWv2cOZs-fLn4H9zeP-FZC-D_YGg0/view?usp=sharing).

---

# :hammer_and_wrench: Technical Stack

- **Game Engine:** Unity 6
- **Programming Language:** C#
- **UI:** Unity UI & TextMeshPro
- **Animation & Tweening:** DOTween
- **Version Control:** Git & GitHub
- **Deployment:** Unity WebGL & GitHub Pages
- **Art & Assets:** Aseprite, Unity Asset Store, and other credited asset sources

---

# :computer: Running the Game Locally

1. Clone this repository:

   ```bash
   git clone https://github.com/Filbertabulate/F4C_Opossum_6748.git
   ```

2. Open **Unity Hub** and select **Add project from disk**.

3. Navigate to the cloned repository and open the project using the appropriate **Unity 6 / 6000.x** version (version used: **6000.4.8f1**).

4. Open the main game scene from the Unity Project window.

5. Press the **Play** button at the top of the Unity Editor.

Alternatively, the latest WebGL version can be played directly through the [online game](https://filbertabulate.github.io/F4C_Opossum_6748/).

---

# :open_file_folder: Project Structure

```text
F4C_Opossum_6748/
│
├── Assets/                     # Unity game assets, scripts, scenes and tests
├── docs/                       # Unity WebGL build deployed through GitHub Pages
├── Milestone_1_Resources/      # Milestone 1 deliverables
├── Milestone_2_Resources/      # Milestone 2 deliverables
├── Milestone_3_Resources/      # Milestone 3 deliverables
├── Packages/                   # Unity package configuration
├── ProjectSettings/            # Unity project settings
└── README.md                   # Repository overview and gameplay guide
```

---

# :scroll: Credits

F4C Opossum was developed as part of the **NUS Orbital 2026** programme and is inspired by the gameplay mechanics of the classic **Age of War**.

External artwork, music, sound effects, and other assets used within the project are credited in detail within the [Full README Report](https://drive.google.com/file/d/1Ps6pWv2cOZs-fLn4H9zeP-FZC-D_YGg0/view?usp=sharing).

---

# :information_source::busts_in_silhouette: Project Information

**Team:** F4C Opossum  
**Programme:** NUS Orbital 2026  
**Level of Achievement:** Apollo 11  
**Engine:** Unity (Editor Version: 6000.4.8f1)  
**Language:** C#  

### Team Members
:bust_in_silhouette: **Wong Bo Xi Filbert** — [LinkedIn](https://www.linkedin.com/in/wong-bo-xi-filbert/)  
:bust_in_silhouette: **Li JianXi** — [LinkedIn](https://www.linkedin.com/in/li-jianxi/)

For the complete project documentation, design decisions, testing strategy, challenges, user testing, and development process, refer to the **[Full README Report](https://drive.google.com/file/d/1Ps6pWv2cOZs-fLn4H9zeP-FZC-D_YGg0/view?usp=sharing)**.
