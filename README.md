# Stardew Valley Mod - Animal Sitter

Animal Sitter Mod for Stardew Valley with long-term support.

## Introduction

This [mod](https://www.nexusmods.com/stardewvalley/mods/20831) provide Long-Term Support for original [Animal Sitter](https://www.nexusmods.com/stardewvalley/mods/581) Mod. It includes:

* Support Stardew Valley v1.6+.
* Support SMAPI v4.0.0+.
* Add i18n Support.
* Add Mod Config Support.
* Will Continue support future version of game and SMAPI.

## Getting Started

* Install the latest version of [SMAPI](https://www.nexusmods.com/stardewvalley/mods/2400).
* Unzip the mod folder into `Stardew Valley/Mods`.
* Run the game using SMAPI.
* Simply press `O` (Letter O).

## Advanced

### Configuration

Here's the default configuration:

```json
"keybind": "O",
"growUpEnabled": false,
"maxHappinessEnabled": false,
"maxFullnessEnabled": false,
"harvestEnabled": true,
"pettingEnabled": true,
"PettingPetEnabled": true,
"maxFriendshipEnabled": false,
"verboseLogging": false,
"costPerAction": 0,
"whoChecks": "spouse",
"enableMessages": true,
"takeTrufflesFromPigs": true,
"bypassInventory": false,
"chestCoords": "87, 18",
"chestDefs": ""
```

**keybind**: The key that you press to tell your animal helper to get to work. This defaults to the "O" key.

**growUpEnabled**: This open tells your animal helper that you'd like them to use dark magic to instantly bring young animals up to the age where they can become contributing members of your farm. Default is false.

**maxHappinessEnabled**: This option tells your animal helper that you don't care how long (or where) they have to pet your animals, but their job is not done until each animal's happiness is maxed. Default is false.

**maxFullnessEnabled**: This option tells your animal helper to feed your animals. A full animal is a producing animal. Default is false.

**harvestEnabled**: This tells your animal worker to harvest the animal drops. If you like doing this yourself, then set this to false.

**pettingEnabled**: This tells your animal worker that you want your animals petted. This is the whole reason I made this mod, so it defaults to true. So if you set this to "false", please keep it to yourself. If you enjoy petting each of your animals (because you don't have a hundred of them) then set it to false.

**PettingPetEnabled**: This will give love to your pet and water all the PetBowls.

**maxFriendshipEnabled**: This tells your animal worker whether they have to wear your "you" mask, so that the affection of the animals is directed toward you, and not the help. Defaults to false.

**verboseLogging**: This enables or disabled debug logging. Useful for troubleshooting.

**costPerAction**: This is how much to charge per action per animal. There have been some suggestions around this option, and I do have plans to introduce a more complex pricing structure in the future, but for now just make sure this includes all services that you want the animal checker to perform, and average it out over the number of actions. Defaults to 0.

**takeTrufflesFromPigs**: Whoever trainedm these pigs did an awful job, as they have a tendency to hide truffles. Where? I'm not exactly sure, but they've got 'em, trust me. With this option enabled, your farm helper will perform a TSA-approved body cavity search and find them. If you love the person who is doing your checking, or if you want your pigs to remain unviolated, or if you consider this cheating, then set it to false.

**whoChecks**: This is the name of the person (or animal) who checks your animals. Defaults to "spouse". You can also set it to "pet"-- they do a really good job, but they don't provide much feedback. You can also set this to any character in the game, which will enable dialogues with that character. If it's set to anything else, that name will be used in messages.

**enableMessages**: Thisis whether to enable in-game messages and dialogues revolving around your animal checker. It defaults to true.

**bypassInventory**: Whether to bypass your inventory when depositing animal drops, and go straight to the chests you have defined. Defaults to false.

**chestCoords**: These are the coordinates of the default chest where overflow items are placed. It defaults to (73,14) which is the tile location, when you are looking at your screen, is to the immediate right of the sell-box.

**chestDefs**: This is a bar-separated list of chest locations that you have defined for specific items. It defaults to blank, but here is an example: "430,70,14|340,7,4,FarmHouse" You can see there are 2 options.

The first specifies the item ID and the chest coordinates, this knows to check for a chest on the farm at that location. The second specifies an item id, a chest location, and a building. This will look for a chest at (7,4) in the FarmHouse (which in the starting house is the spot next to the fireplace).

### Customization on Dialogs

Now our mod support [SMAPI-ModTranslationClassBuilder](https://github.com/Pathoschild/SMAPI-ModTranslationClassBuilder) Framework. So all the strings used in mod will stored in [i18n/default.json](AnimalSitter/i18n/default.json). If you want to add more Dialog or Strings, you need to modify strings inside `default.json` or yoru specific `language.json` file.

The dialogue elements are arranged in a name_index format. If the name starts with a capital 'X', that means those messages need to stay roughly in that order and in the same format for the dialog to make sense.  The number is important.

All other elements can be added to, and modified because they are used randomly at the appropriate place in the dialogue.  You have to use the same "name" from the "name_id" that are already in the file, but you can add to and remove so long as all the numbers in a particular group are unique.

Also you'll see that I added a few names down at the bottom,"Shane_1", "Shane_2", "Leah_1", for example.  You can add any other Stardew characters dialog to the file in that same format.  Those dialogues will be merged in with the "smalltalk" group if your "whoChecks" is set to one of the characters.

Most of the notation that existing SDV dialogs use will work(for example @ is replaced by the name of the farmer).  There's also notation added to use values from this mod in the dialog, they are (along with a description)

**{{animalsPet}}**  -  The number of animals that were petted.
**{{trufflesHarvested}}**  -  The number of truffles that were harvested.
**{{productsHarvested}}**  -  The number of other animal products that were harvested (I promise to allow more granular tracking in the future).
**{{aged}}**  -  The number of animals that were aged to maturity.
**{{fed}}**  -  The number of animals that were fed.
**{{maxHappiness}}**  -  The number of animals who had their happiness maxed.
**{{maxFriendship}}**  -  The number animals that had their friendship toward the farmer maxed out.
**{{numActions}}**  -  The total number of actions performed.
**{{totalCost}}**  -  The total costs for all animals.
**{{spouse}}**  -  The farmer's spouse's name (if married), the value of whoChecks otherwise.

The dialog groups in the file, and a quick explanation of when each are used:

**Xdialog** - Some specific dialogs used when the spouse or non-character check the pots.
**greeting** - Greetings the checker will use when addressing the farmer.
**unfinishedmoney** - Comments the character checker will make when they weren't able to finish on account of no money.
**unfinishedinventory** - Comments the character checker will make when they weren't able to finish on account of not having anywhere to deliver the goods. (Not really used in this mod yet)
**smalltalk & character names** - Comments that are thrown in at the end of most conversations.

Note: If you add more strings, the mod need to be updated to show those new messages. If you want to contribute, feel free to edit and submit Pull Request to [i18n/default.json](https://github.com/WuZhuoran/Stardew_AnimalSitter/blob/main/AnimalSitter/i18n/default.json). I will update the mod ASAP.

## Credit

* Original [Mod](https://www.nexusmods.com/stardewvalley/mods/581) and [Code](https://github.com/jdusbabek/stardewvalley) from [John Dusbabek](https://github.com/jdusbabek)
* Unofficial Fix [Mod](http://forums.stardewvalley.net/threads/unofficial-mod-updates.2096/post-22271)
* Chinese Translation: [codeyz](https://www.nexusmods.com/stardewvalley/users/51596836) with its original translation [mod](https://www.nexusmods.com/stardewvalley/mods/22210)

## Contribution

This project will maintain Open Source [here](https://github.com/WuZhuoran/Stardew_AnimalSitter).

For i18n and translation support. Just Add your languages in `i18n` folders or Add more strings to `default.json` file.

We appreciate all contributions. Feel Free to raise any issues or pull requests.

## License

NOTE: Original Mod Use Apache License. In this repo, MIT License is used. Will update when necessary.

MIT @ [Oliver](https://github.com/WuZhuoran)
