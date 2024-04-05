using System;
using System.Collections.Generic;
using System.Linq;
using AnimalSitter.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using AnimalSitter.Common;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework.Graphics;
using AnimalSitter.Integrations.GenericModConfigMenu;
using Microsoft.Xna.Framework.Input;

namespace AnimalSitter
{
    public class AnimalSitter : Mod
    {
        /*********
        ** Properties
        *********/
        private SButton PetKey;

        // Whether to use dark magic to age the animals to maturity when visiting the animals.
        private bool GrowUpEnabled = true;

        // Whether to pet the animal until their maximum happiness level is reached.
        private bool MaxHappinessEnabled = true;

        // Whether to feed the animals to their max fullness when visiting.
        private bool MaxFullnessEnabled = true;

        // Whether to harvest animal drops while visiting.
        private bool HarvestEnabled = true;

        // Whether to pet animals as they are visited.
        private bool PettingEnabled = true;

        // Whether to max the animal's friendship toward the farmer while visiting, even though the farmer is completely ignoring them.
        private bool MaxFriendshipEnabled = true;

        // Whether to display the in game dialogue messages.
        private bool MessagesEnabled = true;

        // Who does the checking.
        private string Checker = "spouse";

        // How much to charge per animal.
        private int CostPerAnimal;

        // Whether to snatch hidden truffles from the snout of the pig.
        private bool TakeTrufflesFromPigs = true;

        // Coordinates of the default chest.
        private Vector2 ChestCoords = new Vector2(73f, 14f);

        // Whether to bypass the inventory, and first attempt to deposit the harvest into the chest.  Inventory is then used as fallback.
        private bool BypassInventory;

        // A string defining the locations of specific chests.
        private string ChestDefs = "";

        private ModConfig Config;

        private DialogueManager DialogueManager;

        private ChestManager ChestManager;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.DialogueManager = new DialogueManager(this.Config, helper.ModContent, this.Monitor);
            this.ChestManager = new ChestManager(this.Monitor);

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddKeybind(mod: this.ModManifest,
                name: () => "KeyBind",
                tooltip: () => "The key that you press to tell your animal helper to get to work. This defaults to the 'O' key.",
                getValue: () => SButtonExtensions.ToSButton((Keys)Enum.Parse(typeof(Keys), this.Config.KeyBind)),
                setValue: value => this.Config.KeyBind = value.ToString());

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "GrowUpEnabled",
                tooltip: () => "This open tells your animal helper that you'd like them to use dark magic to instantly bring young animals up to the age where they can become contributing members of your farm. Default is false.",
                getValue: () => this.Config.GrowUpEnabled,
                setValue: value => this.Config.GrowUpEnabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "MaxHappinessEnabled",
                tooltip: () => "This option tells your animal helper that you don't care how long (or where) they have to pet your animals, but their job is not done until each animal's happiness is maxed. Default is false.",
                getValue: () => this.Config.MaxHappinessEnabled,
                setValue: value => this.Config.MaxHappinessEnabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "MaxFullnessEnabled",
                tooltip: () => "This option tells your animal helper to feed your animals. A full animal is a producing animal. Default is false.",
                getValue: () => this.Config.MaxFullnessEnabled,
                setValue: value => this.Config.MaxFullnessEnabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "HarvestEnabled",
                tooltip: () => "This tells your animal worker to harvest the animal drops. If you like doing this yourself, then set this to false.",
                getValue: () => this.Config.HarvestEnabled,
                setValue: value => this.Config.HarvestEnabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "PettingEnabled",
                tooltip: () => "This tells your animal worker that you want your animals petted. This is the whole reason I made this mod, so it defaults to true. So if you set this to \"false\", please keep it to yourself. If you enjoy petting each of your animals (because you don't have a hundred of them) then set it to false.",
                getValue: () => this.Config.PettingEnabled,
                setValue: value => this.Config.PettingEnabled = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "MaxFriendshipEnabled",
                tooltip: () => "This tells your animal worker whether they have to wear your 'you' mask, so that the affection of the animals is directed toward you, and not the help. Defaults to false.",
                getValue: () => this.Config.MaxFriendshipEnabled,
                setValue: value => this.Config.MaxFriendshipEnabled = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "CostPerAction",
                tooltip: () => "This is how much to charge per action per animal. There have been some suggestions around this option, and I do have plans to introduce a more complex pricing structure in the future, but for now just make sure this includes all services that you want the animal checker to perform, and average it out over the number of actions. Defaults to 25.",
                getValue: () => this.Config.CostPerAction,
                setValue: value => this.Config.CostPerAction = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "EnableMessages",
                tooltip: () => "This is whether to enable in-game messages and dialogues revolving around your animal checker. It defaults to true.",
                getValue: () => this.Config.EnableMessages,
                setValue: value => this.Config.EnableMessages = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "TakeTrufflesFromPigs",
                tooltip: () => "Whoever trainedm these pigs did an awful job, as they have a tendency to hide truffles. Where? I'm not exactly sure, but they've got 'em, trust me. With this option enabled, your farm helper will perform a TSA-approved body cavity search and find them. If you love the person who is doing your checking, or if you want your pigs to remain unviolated, or if you consider this cheating, then set it to false.",
                getValue: () => this.Config.TakeTrufflesFromPigs,
                setValue: value => this.Config.TakeTrufflesFromPigs = value
            );
        }


        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.ImportConfiguration();

            //parseChestLocations();
            this.ChestManager.ParseChests(this.ChestDefs);
            this.ChestManager.SetDefault(this.ChestCoords);

            // Read in dialogue
            this.DialogueManager.ReadInMessages();

            this.Monitor.Log($"chestCoords:{this.ChestCoords.X},{this.ChestCoords.Y}", LogLevel.Trace);
        }

        private void ImportConfiguration()
        {
            if (!Enum.TryParse(this.Config.KeyBind, true, out this.PetKey))
            {
                this.PetKey = SButton.O;
                this.Monitor.Log($"Error parsing key binding; defaulted to {this.PetKey}.");
            }

            this.PettingEnabled = this.Config.PettingEnabled;
            this.GrowUpEnabled = this.Config.GrowUpEnabled;
            this.MaxHappinessEnabled = this.Config.MaxHappinessEnabled;
            this.MaxFriendshipEnabled = this.Config.MaxFriendshipEnabled;
            this.MaxFullnessEnabled = this.Config.MaxFullnessEnabled;
            this.HarvestEnabled = this.Config.HarvestEnabled;
            this.Checker = this.Config.WhoChecks;
            this.MessagesEnabled = this.Config.EnableMessages;
            this.TakeTrufflesFromPigs = this.Config.TakeTrufflesFromPigs;
            this.ChestCoords = this.Config.ChestCoords;

            this.BypassInventory = this.Config.BypassInventory;
            this.ChestDefs = this.Config.ChestDefs;

            if (this.Config.CostPerAction < 0)
            {
                this.Monitor.Log("I'll do it for free, but I'm not paying YOU to take care of YOUR stinking animals!", LogLevel.Trace);
                this.Monitor.Log("Setting costPerAction to 0.", LogLevel.Trace);
                this.CostPerAnimal = 0;
            }
            else
            {
                this.CostPerAnimal = this.Config.CostPerAction;
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button == this.PetKey)
            {
                try
                {
                    this.IterateOverAnimals();
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Exception onKeyReleased: {ex}", LogLevel.Error);
                }
            }
        }

        private void IterateOverAnimals()
        {
            Farmer farmer = Game1.player;
            AnimalTasks stats = new AnimalTasks();

            foreach (FarmAnimal animal in this.GetAnimals())
            {
                try
                {
                    if (!animal.wasPet.Value && this.PettingEnabled)
                    {
                        animal.pet(Game1.player);
                        stats.AnimalsPet++;

                        this.Monitor.Log($"Petting animal: {animal.Name}", LogLevel.Trace);
                    }


                    if (this.GrowUpEnabled && animal.isBaby())
                    {
                        this.Monitor.Log($"Aging animal to mature+1 days: {animal.Name}", LogLevel.Trace);

                        animal.age.Value = animal.age.Value + 1;
                        animal.reload(animal.home);
                        stats.Aged++;
                    }

                    if (this.MaxFullnessEnabled && animal.fullness.Value < byte.MaxValue)
                    {
                        this.Monitor.Log($"Feeding animal: {animal.Name}", LogLevel.Trace);

                        animal.fullness.Value = byte.MaxValue;
                        stats.Fed++;
                    }

                    if (this.MaxHappinessEnabled && animal.happiness.Value < byte.MaxValue)
                    {
                        this.Monitor.Log($"Maxing Happiness of animal {animal.Name}", LogLevel.Trace);

                        animal.happiness.Value = byte.MaxValue;
                        stats.MaxHappiness++;
                    }

                    if (this.MaxFriendshipEnabled && animal.friendshipTowardFarmer.Value < 1000)
                    {
                        this.Monitor.Log($"Maxing Friendship of animal {animal.Name}", LogLevel.Trace);

                        animal.friendshipTowardFarmer.Value = 1000;
                        stats.MaxFriendship++;
                    }

                    if (animal.currentProduce.Value != null && Convert.ToInt32(animal.currentProduce.Value) > 0 && this.HarvestEnabled)
                    {
                        this.Monitor.Log($"Has produce: {animal.Name} {animal.currentProduce}", LogLevel.Trace);

                        if (animal.type.Value == "Pig")
                        {
                            if (this.TakeTrufflesFromPigs)
                            {
                                Object toAdd = new Object(animal.currentProduce.Value, 1, false, -1, animal.produceQuality.Value);
                                this.AddItemToInventory(toAdd, farmer);

                                stats.TrufflesHarvested++;
                            }
                        }
                        else
                        {
                            Object toAdd = new Object(animal.currentProduce.Value, 1, false, -1, animal.produceQuality.Value);
                            this.AddItemToInventory(toAdd, farmer);

                            animal.currentProduce.Value = "0";
                            stats.ProductsHarvested++;
                        }


                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Exception onKeyReleased: {ex}", LogLevel.Error);
                }
            }

            this.HarvestTruffles(stats);
            this.HarvestCoops(stats);

            int actions = stats.GetTaskCount();
            bool gatheringOnly = stats.JustGathering();

            if (actions > 0 && this.CostPerAnimal > 0)
            {
                int totalCost = actions * this.CostPerAnimal;
                bool doesPlayerHaveEnoughCash = Game1.player.Money >= totalCost;
                Game1.player.Money = Math.Max(0, Game1.player.Money - totalCost);

                if (this.MessagesEnabled)
                    this.ShowMessage(actions, totalCost, doesPlayerHaveEnoughCash, gatheringOnly, stats);

                this.Monitor.Log($"Animal sitter performed {actions} actions. Total cost: {totalCost}g", LogLevel.Trace);

            }
            else if (actions == 0 && this.CostPerAnimal > 0)
            {
                if (this.MessagesEnabled)
                {
                    HUDMessage msg = new HUDMessage("There's nothing to do for the animals right now.");
                    Game1.addHUDMessage(msg);
                }

                this.Monitor.Log("There's nothing to do for the animals right now.", LogLevel.Trace);
            }
        }

        private void HarvestTruffles(AnimalTasks stats)
        {
            Farm farm = Game1.getFarm();
            Farmer farmer = Game1.player;
            Random random = new Random();

            List<Vector2> itemsToRemove = new List<Vector2>();

            // Iterate over the objects, and add them to inventory.
            foreach (KeyValuePair<Vector2, Object> keyvalue in farm.Objects.Pairs)
            {
                Object obj = keyvalue.Value;

                if (obj.Name == "Truffle")
                {
                    bool doubleHarvest = false;

                    if (Game1.player.professions.Contains(16))
                        obj.Quality = 4;

                    double randomNum = random.NextDouble();
                    bool doubleChance = (this.Checker.Equals("pet")) ? (randomNum < 0.4) : (randomNum < 0.2);

                    if (Game1.player.professions.Contains(13) && doubleChance)
                    {
                        obj.Stack = 2;
                        doubleHarvest = true;
                    }

                    if (this.AddItemToInventory(obj, farmer))
                    {
                        itemsToRemove.Add(keyvalue.Key);
                        farmer.gainExperience(2, 7);
                        stats.TrufflesHarvested++;

                        if (doubleHarvest)
                        {
                            stats.TrufflesHarvested++;
                            farmer.gainExperience(2, 7);
                        }

                    }
                    else
                    {
                        this.Monitor.Log("Inventory full, could not add animal product.", LogLevel.Trace);
                    }
                }

            }

            // Now remove the items
            foreach (Vector2 itemLocation in itemsToRemove)
            {
                farm.removeObject(itemLocation, false);
            }

        }

        private void HarvestCoops(AnimalTasks stats)
        {
            Farm farm = Game1.getFarm();
            Farmer farmer = Game1.player;

            foreach (Building building in farm.buildings)
            {
                if (!building.GetIndoorsType().Equals(IndoorsType.None) && building is Building && !building.GetIndoorsName().Equals("Greenhouse"))
                {
                    List<Vector2> itemsToRemove = new List<Vector2>();

                    foreach (KeyValuePair<Vector2, Object> keyvalue in building.GetIndoors().Objects.Pairs)
                    {
                        Object obj = keyvalue.Value;

                        this.Monitor.Log($"Found coop object: {obj.Name} / {obj.Category}/{obj.isAnimalProduct()}", LogLevel.Trace);

                        if (obj.isAnimalProduct() || obj.ParentSheetIndex == 107 && obj.Name != "Plush Bunny")
                        {
                            if (this.AddItemToInventory(obj, farmer))
                            {
                                itemsToRemove.Add(keyvalue.Key);
                                stats.ProductsHarvested++;
                                farmer.gainExperience(0, 5);
                            }
                            else
                            {
                                this.Monitor.Log("Inventory full, could not add animal product.", LogLevel.Trace);
                            }
                        }
                    }

                    // Remove the object that were picked up.
                    foreach (Vector2 itemLocation in itemsToRemove)
                    {
                        building.GetIndoors().removeObject(itemLocation, false);
                    }
                    
                }
            }
        }

        private bool AddItemToInventory(Object obj, Farmer farmer)
        {
            if (!this.BypassInventory)
            {
                if (farmer.couldInventoryAcceptThisItem(obj))
                {
                    farmer.addItemToInventory(obj);
                    return true;
                }
            }

            // Get the preferred chest (could be default)
            Object chestObj = this.ChestManager.GetChest(obj.ParentSheetIndex);

            if (chestObj is Chest chest)
            {
                Item i = chest.addItem(obj);
                if (i == null)
                    return true;
            }

            // We haven't returned, get the default chest.
            chestObj = this.ChestManager.GetDefaultChest();

            if (chestObj is Chest defaultChest)
            {
                Item i = defaultChest.addItem(obj);
                if (i == null)
                    return true;
            }

            // Haven't been able to add to a chest, try inventory one last time.
            if (farmer.couldInventoryAcceptThisItem(obj))
            {
                farmer.addItemToInventory(obj);
                return true;
            }

            return false;
        }

        private void ShowMessage(int numActions, int totalCost, bool doesPlayerHaveEnoughCash, bool gatheringOnly, AnimalTasks stats)
        {
            stats.NumActions = numActions;
            stats.TotalCost = totalCost;

            string message = "";

            if (this.Checker.ToLower() == "pet")
            {
                if (Game1.player.hasPet())
                {
                    if (Game1.player.catPerson)
                    {
                        message += "Meow..";
                    }
                    else
                    {
                        message += "Woof.";
                    }
                }
                else
                {
                    message += "Your imaginary pet has taken care of your animals.";
                }

                HUDMessage msg = new HUDMessage(message);
                Game1.addHUDMessage(msg);
            }
            else
            {
                if (this.Checker.ToLower() == "spouse")
                {
                    if (Game1.player.isMarriedOrRoommates())
                    {
                        message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetMessageAt(1, "Xdialog"), stats, this.Config);
                    }
                    else
                    {
                        message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetMessageAt(2, "Xdialog"), stats, this.Config);
                    }

                    if (totalCost > 0 && this.CostPerAnimal > 0)
                    {
                        message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetMessageAt(3, "Xdialog"), stats, this.Config);
                    }

                    HUDMessage msg = new HUDMessage(message);
                    Game1.addHUDMessage(msg);
                }
                else if (gatheringOnly)
                {
                    message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetMessageAt(4, "Xdialog"), stats, this.Config);

                    if (totalCost > 0 && this.CostPerAnimal > 0)
                    {
                        message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetMessageAt(3, "Xdialog"), stats, this.Config);
                    }

                    HUDMessage msg = new HUDMessage(message);
                    Game1.addHUDMessage(msg);
                }
                else
                {
                    NPC character = Game1.getCharacterFromName(this.Checker);
                    if (character != null)
                    {
                        //this.isCheckerCharacter = true;
                        string portrait = "";
                        if (character.Name.Equals("Shane"))
                        {
                            portrait = "$8";
                        }

                        message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetRandomMessage("greeting"), stats, this.Config);
                        message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetMessageAt(5, "Xdialog"), stats, this.Config);

                        if (this.CostPerAnimal > 0)
                        {
                            if (doesPlayerHaveEnoughCash)
                            {
                                message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetMessageAt(6, "Xdialog"), stats, this.Config);
                            }
                            else
                            {
                                message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetRandomMessage("unfinishedmoney"), stats, this.Config);
                            }
                        }
                        else
                        {

                            //message += portrait + "#$e#";
                        }

                        message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetRandomMessage("smalltalk"), stats, this.Config);
                        message += portrait + "#$e#";

                        character.CurrentDialogue.Push(new Dialogue(character, message));
                        Game1.drawDialogue(character);
                    }
                    else
                    {
                        //message += checker + " has performed " + numActions + " for your animals.";
                        message += this.DialogueManager.PerformReplacement(this.DialogueManager.GetMessageAt(7, "Xdialog"), stats, this.Config);
                        HUDMessage msg = new HUDMessage(message);
                        Game1.addHUDMessage(msg);
                    }
                }
            }

        }

        private List<FarmAnimal> GetAnimals()
        {
            Farm farm = Game1.getFarm();
            List<FarmAnimal> animals = farm.getAllFarmAnimals().ToList();

            foreach (Building building in farm.buildings)
                if (building.indoors.Value != null && building.indoors.Value.GetType() == typeof(AnimalHouse))
                    animals.AddRange(((AnimalHouse)building.indoors.Value).animals.Values.ToList());
            return animals;
        }

    }
}
