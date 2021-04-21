Inventory Editor
---


This is a bare-bones inventory editor. You can't add/remove items, **you can only change the quantity**, but it has item names for just about everything.

### The game must be running for this to work.
This is basically a structured view of the inventory as it exists in ram. If the game isn't running, this program will do nothing and immediately terminate.

### Don't leave open when you play. Pause the game, run this, change quantities, close, return to game.
The game moves the inventory around in ram as it adds/removes items. Meaning if that happens when this is open, the shown address will point to the wrong location in mem. Editing it at that point may, at worst, corrupt saves.

![](Pics/1.png?raw=true)

Credits
---

Thanks to DrummerIX for posting a list of item ids. I only had to add/change about 2 entries.