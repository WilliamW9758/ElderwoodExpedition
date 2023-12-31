# ElderwoodExpedition
## Genre
2D Top-down action rogue-like
## Engine & Platform
Unity 2021.3.27f1 Apple Silicon LTS, currently built to Windows & MacOS, easily portable to mobile in the future
## Key Game Feature
The key feature of Elderwood Expedition is its customizable combo system. In most games, one button usually corresponds to one ability or a series of predetermined moves; however, in this game, abilities (called runes) are like items that can be placed into timeline-like slots, where each click of the button will activate one tune, going from top to bottom. There are two weapon timelines. The left one, which has more slots and faster reloads, corresponds to the left click, or primary attack. The right one has fewer slots, slower reload, and corresponds to the right click, or secondary attacks. This allows a balance of commonly used attacks, such as quick slash, vs. situational attacks, such as parry or large spells that cost energy to cast. 

![Customizable Combo Demo](https://github.com/WilliamW9758/ElderwoodExpedition/blob/main/DemoMedia/AttackDemo.gif)
This way, the game frontloads a lot of the strategy to the planning phase before the battle, requiring the player to adapt and plan out their series of actions. For example, should I put the parry rune at the first slot so I initiate with an almost certain success parry, or in the middle of the pack so I can adapt my pace during battle, or at the end so I can ensure a safe reload if I parry successfully.

There will also be relics similar to traditional rogue-likes that give you certain buffs as long as they are in your inventory. This will allow the player to dive into certain build paths more easily.
## Technical Features
### Scriptable-Object-based Inventory and Weapon System
To implement a flexible inventory system that allows moving abilities around and reflecting their effect immediately in combat while also keeping in consideration the ease of implementing new items in the future, I implemented an inventory system based on scriptable objects in Unity. This allows a couple of advantages. First, items are easily carried over scenes and sessions, because scriptable objects are independent from game sessions. Second, a scriptable-object-based inventory system blurs the line between item and ability. I was able to make my weapon “timeline” slots also an inventory with a few special tags, which allows items to be dragged smoothly from bag to weapon. Third, I was able to easily create  enemy logic. Since runes inherently have their cooldown and energy requirement, by giving each enemy their weapon slots and corresponding abilities, I can quickly implement a new enemy by tweaking a few numbers and switching out a few items in their weapon slots. 

![Enemy Weapon Demo](https://github.com/WilliamW9758/ElderwoodExpedition/blob/main/DemoMedia/WeaponDemo.png)
### Randomly Generated Map
I also implemented a randomly generated map using the marching cubes algorithm. It can reliably generate organic cave and forest systems. The game first generates a field of random noises, and then a smoothing algorithm similar to the game of life is run for a few iterations. The system then uses flood fill to identify independent rooms and make connections between them to ensure the whole map is explorable. Finally, the game generates the player spawn location, boss location, enemy groups, chests, and events. 
![Map Generation Demo](https://github.com/WilliamW9758/ElderwoodExpedition/blob/main/DemoMedia/MapGenDemo.gif)

## Future Game Plans
### More Content
I have more than 100 ideas of new relics and runes planned out, spread across 9 play styles including Crit, Flat Damage, Life steal, Reload, Tank, DOT, WindUp, Direct Damage Mage, and Buff Mage. I will gradually implement the ideas throughout the next few months.

There will also be more enemy types and boss types. Thanks to the modular structure of the weapon system, creating more runes automatically creates more combinations for enemies. I will just need to balance the stats and drag in a few runes to create a new enemy.

### Better Level System
The current exploration style may not be the best fit for the planning + executing game flow. I plan on redesigning the level system to have smaller maps and showing the enemy compositions of the level in the preparation stage. This way, you can fine-tune your loadout for the level, maximizing the effect of the planning aspect of the game.
