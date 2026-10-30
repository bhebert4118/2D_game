
## Description
Your rocket is missing two parts. Run, jump and wall-jump through the level to find the **rocket body** and the **engine**, then carry each one back to the rocket. Every part you return opens a sliding number puzzle, and solving it installs the part. Install both and the rocket blasts off.

## How to play

1. Explore the level and touch a rocket part to pick it up. You can carry one part at a time, and the "Carrying:" text shows what you're holding.
2. Bring the part to the rocket's return point. A shuffled 3x3 sliding puzzle opens.
3. Put the tiles in order from 1 to 8 to install the part. Installing the rocket body stands the rocket up on the ground.
4. After both parts are installed, you board the rocket, it launches with the camera following it, and **COMPLETE** appears.

Watch out for:

- **Death zones**, which send you back to the start.
- **Vanishing platforms**, which disappear and come back on a timer.

## Controls

| Action | Keys |
| --- | --- |
| Move left / right | A / D or Left / Right arrows |
| Jump | Space |
| Wall jump | Space while sliding on a wall |
| Keep wall-jump direction | Hold Space after a wall jump (the direction stays locked until you let go) |
| Slide a puzzle tile | Left-click a tile in the same row or column as the empty space |

Touching a wall in mid-air makes you slide down it slowly, so you can wall-jump back and forth up a gap.


## Scripts

| Script | What it does |
| --- | --- |
| `PlayerMovement.cs` | Running, jumping, wall-sliding and wall-jumping |
| `PlayerInventory.cs` | Holds the one part the player is carrying and updates the "Carrying:" text |
| `RocketPart.cs` | A rocket part the player can pick up |
| `PartReturn.cs` | The rocket's return point: takes the part, opens the puzzle, stands the rocket up, and launches it when both parts are installed |
| `NumberSlidingPuzzle.cs` | The 3x3 sliding number puzzle, always shuffled to a solvable layout |
| `CameraFollow.cs` | Keeps the camera on the player, then follows the rocket during launch |
| `Death.cs` | Death zone that sends the player back to the start |
| `Disappear.cs` | Platforms that vanish and reappear on a timer |
| `floor_wall.cs` | Floors or walls that randomly appear or not each time the level loads |
| `KeyPickup.cs` | A key that removes a wall when the player collects it |
