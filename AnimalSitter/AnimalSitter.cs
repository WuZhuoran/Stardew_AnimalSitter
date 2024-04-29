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
using AnimalSitter.Integrations.GenericModConfigMenu;
using Microsoft.Xna.Framework.Input;
using StardewValley.Characters;

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

        // Whether to pet animals (Not Pets) as they are visited.
        private bool PettingEnabled = true;

        // Whether to pet all pets as they are visited.
        private bool PettingPetEnabled = true;

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

        private bool isFirstTimeTruffle = true;

        private ModConfig Config;

        // private DialogueManager DialogueManager;

        private ChestManager ChestManager;

        private readonly Random RandomDialogue = new Random();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            // this.DialogueManager = new DialogueManager(this.Config, helper.ModContent, this.Monitor);
            this.ChestManager = new ChestManager(this.Monitor);
            I18n.Init(helper.Translation);

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.isFirstTimeTruffle = true;
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
                name: () => I18n.Config_KeyBind(),
                tooltip: () => I18n.Config_KeyBind_Description(),
                getValue: () => SButtonExtensions.ToSButton((Keys)Enum.Parse(typeof(Keys), this.Config.KeyBind)),
                setValue: value => 
                {
                    this.Config.KeyBind = value.ToString();
                    this.ImportConfiguration();
                });

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_GrowUpEnabled(),
                tooltip: () => I18n.Config_GrowUpEnabled_Description(),
                getValue: () => this.Config.GrowUpEnabled,
                setValue: value => 
                {
                    // this.GrowUpEnabled = value;
                    this.Config.GrowUpEnabled = value;
                    this.ImportConfiguration();
                }
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_MaxHappinessEnabled(),
                tooltip: () => I18n.Config_MaxHappinessEnabled_Description(),
                getValue: () => this.Config.MaxHappinessEnabled,
                setValue: value =>
                {
                    // this.MaxHappinessEnabled = value;
                    this.Config.MaxHappinessEnabled = value;
                    this.ImportConfiguration();
                }
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_MaxFullnessEnabled(),
                tooltip: () => I18n.Config_MaxFullnessEnabled_Description(),
                getValue: () => this.Config.MaxFullnessEnabled,
                setValue: value => 
                {
                    // this.MaxFullnessEnabled = value; 
                    this.Config.MaxFullnessEnabled = value;
                    this.ImportConfiguration();
                }
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_HarvestEnabled(),
                tooltip: () => I18n.Config_HarvestEnabled_Description(),
                getValue: () => this.Config.HarvestEnabled,
                setValue: value => 
                {
                    // this.HarvestEnabled = value; 
                    this.Config.HarvestEnabled = value;
                    this.ImportConfiguration();
                }
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_PettingEnabled(),
                tooltip: () => I18n.Config_PettingEnabled_Description(),
                getValue: () => this.Config.PettingEnabled,
                setValue: value => 
                {
                    // this.PettingEnabled = value; 
                    this.Config.PettingEnabled = value;
                    this.ImportConfiguration();
                }
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_PettingPetEnabled(),
                tooltip: () => I18n.Config_PettingPetEnabled_Description(),
                getValue: () => this.Config.PettingPetEnabled,
                setValue: value => 
                {
                    // this.PettingPetEnabled = value; 
                    this.Config.PettingPetEnabled = value;
                    this.ImportConfiguration();
                }
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_MaxFriendshipEnabled(),
                tooltip: () => I18n.Config_MaxFriendshipEnabled_Description(),
                getValue: () => this.Config.MaxFriendshipEnabled,
                setValue: value => 
                {
                    // this.MaxFriendshipEnabled = value; 
                    this.Config.MaxFriendshipEnabled = value;
                    this.ImportConfiguration();
                }
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => I18n.Config_CostPerAction(),
                tooltip: () => I18n.Config_CostPerAction_Description(),
                getValue: () => this.Config.CostPerAction,
                setValue: value => 
                {
                    // this.CostPerAnimal = value;
                    this.Config.CostPerAction = value;
                    this.ImportConfiguration();
                }
                );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => I18n.Config_WhoChecks(),
                tooltip: () => I18n.Config_WhoChecks_Description(),
                getValue: () => this.Config.WhoChecks,
                setValue: value =>
                {
                    // this.Checker = value;
                    this.Config.WhoChecks = value;
                    this.ImportConfiguration();
                },
                allowedValues: new string[] { "spouse", "pet", "Shane", "Haley", "Alex", "Leah", "Marnie" }
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_EnableMessages(),
                tooltip: () => I18n.Config_EnableMessages_Description(),
                getValue: () => this.Config.EnableMessages,
                setValue: value => 
                {
                    // this.MessagesEnabled = value;
                    this.Config.EnableMessages = value;
                    this.ImportConfiguration();
                }
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_TakeTrufflesFromPigs(),
                tooltip: () => I18n.Config_TakeTrufflesFromPigs_Description(),
                getValue: () => this.Config.TakeTrufflesFromPigs,
                setValue: value =>
                {
                    // this.TakeTrufflesFromPigs = value;
                    this.Config.TakeTrufflesFromPigs = value;
                    this.ImportConfiguration();
                }
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
            // this.DialogueManager.ReadInMessages();

            this.Monitor.Log(I18n.Log_ChestCoords(x: this.ChestCoords.X, y: this.ChestCoords.Y), LogLevel.Trace);
        }

        private void ImportConfiguration()
        {
            if (!Enum.TryParse(this.Config.KeyBind, true, out this.PetKey))
            {
                this.PetKey = SButton.O;
                this.Monitor.Log(I18n.Log_ErrorParsingKeyBingding(default_value: this.PetKey));
            }

            this.PettingEnabled = this.Config.PettingEnabled;
            this.PettingPetEnabled = this.Config.PettingPetEnabled;
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
                this.Monitor.Log(I18n.Log_DoingFree(), LogLevel.Trace);
                this.Monitor.Log(I18n.Log_SetCostTo0(), LogLevel.Trace);
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
                    this.Monitor.Log(I18n.Log_ExceptionOnkeyreleased(ex: ex), LogLevel.Error);
                }
            }
        }

        private void IterateOverAnimals()
        {
            Farmer farmer = Game1.player;
            Farm farm = Game1.getFarm();
            AnimalTasks stats = new AnimalTasks();

            if (farmer.hasPet() && this.PettingPetEnabled)
            {
                // Pet each Pet
                foreach (Pet pet in this.GetPets())
                {
                    try
                    {
                        pet.checkAction(farmer, farm);
                        this.Monitor.Log(I18n.Log_PettingAnimal(animal_name: pet.Name), LogLevel.Trace);
                    }
                    catch (Exception ex)
                    {
                        this.Monitor.Log(I18n.Log_ExceptionOnkeyreleased(ex: ex), LogLevel.Error);
                    }
                }

                // Water all the bowl
                this.WaterPetBowl();
            }

            foreach (FarmAnimal animal in this.GetAnimals())
            {
                Random random = Utility.CreateRandom(animal.myID.Value / 2.0, Game1.stats.DaysPlayed);
                try
                {
                    if (!animal.wasPet.Value && this.PettingEnabled)
                    {
                        animal.pet(Game1.player);
                        stats.AnimalsPet++;

                        this.Monitor.Log(I18n.Log_PettingAnimal(animal_name: animal.Name), LogLevel.Trace);
                    }


                    if (this.GrowUpEnabled && animal.isBaby())
                    {
                        this.Monitor.Log(I18n.Log_AgingAnimal(animal_name: animal.Name), LogLevel.Trace);

                        animal.age.Value = animal.age.Value + 1;
                        animal.reload(animal.home);
                        stats.Aged++;
                    }

                    if (this.MaxFullnessEnabled && animal.fullness.Value < byte.MaxValue)
                    {
                        this.Monitor.Log(I18n.Log_FeedingAnimal(animal_name: animal.Name), LogLevel.Trace);

                        animal.fullness.Value = byte.MaxValue;
                        stats.Fed++;
                    }

                    if (this.MaxHappinessEnabled && animal.happiness.Value < byte.MaxValue)
                    {
                        this.Monitor.Log(I18n.Log_MaxHappinessAnimal(animal_name: animal.Name), LogLevel.Trace);

                        animal.happiness.Value = byte.MaxValue;
                        stats.MaxHappiness++;
                    }

                    if (this.MaxFriendshipEnabled && animal.friendshipTowardFarmer.Value < 1000)
                    {
                        this.Monitor.Log(I18n.Log_MaxFriendshipAnimal(animal_name: animal.Name), LogLevel.Trace);

                        animal.friendshipTowardFarmer.Value = 1000;
                        stats.MaxFriendship++;
                    }

                    if (animal.currentProduce.Value != null && Convert.ToInt32(animal.currentProduce.Value) > 0 && this.HarvestEnabled)
                    {
                        this.Monitor.Log(I18n.Log_HasProduce(animal_name: animal.Name, animal_currentProduce: animal.currentProduce.Name), LogLevel.Trace);

                        if (animal.type.Value == "Pig")
                        {
                            if (this.TakeTrufflesFromPigs && this.isFirstTimeTruffle)
                            {
                                int stack = random.NextDouble() + Game1.player.team.AverageDailyLuck() * animal.GetAnimalData().DeluxeProduceLuckMultiplier < 0.7 ? 1 : 2;
                                Object toAdd = new Object(animal.currentProduce.Value, stack, false, -1, animal.produceQuality.Value);
                                this.AddItemToInventory(toAdd, farmer);

                                stats.TrufflesHarvested++;
                            }
                        }
                        else
                        {
                            int stack = random.NextDouble() + Game1.player.team.AverageDailyLuck() * animal.GetAnimalData().DeluxeProduceLuckMultiplier < 0.7 ? 1 : 2;
                            stack = animal.hasEatenAnimalCracker.Value ? stack * 2 : stack;
                            
                            Object toAdd = new Object(animal.currentProduce.Value, stack, false, -1, animal.produceQuality.Value);
                            this.AddItemToInventory(toAdd, farmer);
                            animal.currentProduce.Value = null;

                            stats.ProductsHarvested++;
                        }


                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log(I18n.Log_ExceptionOnkeyreleased(ex: ex), LogLevel.Error);
                }
            }

            this.isFirstTimeTruffle = false;
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

                this.Monitor.Log(I18n.Log_TotalCost(actions: actions, total_cost: totalCost), LogLevel.Trace);

            }
            else if (actions == 0 && this.CostPerAnimal > 0)
            {
                if (this.MessagesEnabled)
                {
                    HUDMessage msg = new HUDMessage(I18n.Log_NothingToDo());
                    Game1.addHUDMessage(msg);
                }

                this.Monitor.Log(I18n.Log_NothingToDo(), LogLevel.Trace);
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
                        this.Monitor.Log(I18n.Log_InventoryFull(), LogLevel.Trace);
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

                        this.Monitor.Log(I18n.Log_FoundCoopObject(obj_name: obj.Name, obj_category: obj.Category, obj_isAnimalProduct: obj.isAnimalProduct()), LogLevel.Trace);

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
                                this.Monitor.Log(I18n.Log_InventoryFull(), LogLevel.Trace);
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
                        message += I18n.Log_Meow();
                    }
                    else
                    {
                        message += I18n.Log_Woof();
                    }
                }
                else
                {
                    message += I18n.Log_ImaginaryPetTakeCare();
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
                        string spouse = Game1.player.isMarriedOrRoommates() ? Game1.player.getSpouse().getName() : this.Checker;
                        message += I18n.Dialog_Xdialog1(spouse: spouse);
                    }
                    else
                    {
                        message += I18n.Dialog_Xdialog2(num_actions: stats.NumActions);
                    }

                    if (totalCost > 0 && this.CostPerAnimal > 0)
                    {
                        message += I18n.Dialog_Xdialog3(total_cost: stats.TotalCost);
                    }

                    HUDMessage msg = new HUDMessage(message);
                    Game1.addHUDMessage(msg);
                }
                else if (gatheringOnly)
                {
                    message += I18n.Dialog_Xdialog4(checker: this.Checker);

                    if (totalCost > 0 && this.CostPerAnimal > 0)
                    {
                        message += I18n.Dialog_Xdialog3(total_cost: stats.TotalCost);
                    }

                    HUDMessage msg = new HUDMessage(message);
                    Game1.addHUDMessage(msg);
                }
                else
                {
                    NPC character = Game1.getCharacterFromName(this.Checker);
                    if (character != null)
                    {
                        // this.isCheckerCharacter = true;
                        // string portrait = "";
                        if (character.Name.Equals("Shane"))
                        {
                            // portrait = "$8";
                            message += this.GetRandomMessage(messageStoreName: "greeting", low: 1, high: 7);
                            message += this.GetRandomMessage(messageStoreName: "Shane", low: 1, high: 2);
                        }
                        else if (character.Name.Equals("Haley"))
                        {
                            // portrait = "$8";
                            message += this.GetRandomMessage(messageStoreName: "greeting", low: 1, high: 7);
                            message += this.GetRandomMessage(messageStoreName: "Haley", low: 1, high: 2);
                        }
                        else if (character.Name.Equals("Alex"))
                        {
                            // portrait = "$8";
                            message += this.GetRandomMessage(messageStoreName: "greeting", low: 1, high: 7);
                            message += this.GetRandomMessage(messageStoreName: "Alex", low: 1, high: 2);
                        }
                        else if (character.Name.Equals("Leah"))
                        {
                            // portrait = "$8";
                            message += this.GetRandomMessage(messageStoreName: "greeting", low: 1, high: 7);
                            message += this.GetRandomMessage(messageStoreName: "Leah", low: 1, high: 2);
                        }
                        else if (character.Name.Equals("Marnie"))
                        {
                            // portrait = "$8";
                            message += this.GetRandomMessage(messageStoreName: "greeting", low: 1, high: 7);
                            message += this.GetRandomMessage(messageStoreName: "Marnie", low: 1, high: 2);
                        }
                        else
                        {
                            message += this.GetRandomMessage(messageStoreName: "greeting", low: 1, high: 7);
                            message += I18n.Dialog_Xdialog5();
                        }


                        if (this.CostPerAnimal > 0)
                        {
                            if (doesPlayerHaveEnoughCash)
                            {
                                message += I18n.Dialog_Xdialog6(total_cost: stats.TotalCost);
                            }
                            else
                            {
                                message += this.GetRandomMessage(messageStoreName: "unfinishedmoney", low: 1, high: 8);
                            }
                        }
                        else
                        {

                            //message += portrait + "#$e#";
                        }

                        message += this.GetRandomMessage(messageStoreName: "smalltalk", low: 1, high: 14);
                        // message += portrait + "#$e#";

                        character.CurrentDialogue.Push(new Dialogue(character, "", message));
                        Game1.drawDialogue(character);
                    }
                    else
                    {
                        message += I18n.Dialog_Xdialog7(checker: this.Checker, num_actions: stats.NumActions);
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

        private List<Pet> GetPets()
        {
            List<Pet> pets = new List<Pet>();
            foreach (GameLocation location in Game1.locations)
            {
                foreach (Pet pet in location.characters.OfType<Pet>())
                {
                    pets.Add(pet);
                }
            }
            return pets;
        }
        private void WaterPetBowl()
        {
            foreach (GameLocation location in Game1.locations)
            {
                foreach (PetBowl item in location.buildings.OfType<PetBowl>())
                {
                    item.watered.Set(true);
                    this.Monitor.Log(I18n.Log_WateringBowl(X: item.tileX.Value, Y: item.tileY.Value), LogLevel.Trace);
                }
            }
        }
        private string GetRandomMessage(string messageStoreName, int low=1, int high=4)
        {
            var rand = RandomDialogue.Next(low, high + 1);

            if (messageStoreName == "greeting")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Greeting1(name: Game1.player.Name),
                    2 => I18n.Dialog_Greeting2(name: Game1.player.Name),
                    3 => I18n.Dialog_Greeting3(name: Game1.player.Name),
                    4 => I18n.Dialog_Greeting4(name: Game1.player.Name),
                    5 => I18n.Dialog_Greeting5(name: Game1.player.Name),
                    6 => I18n.Dialog_Greeting6(name: Game1.player.Name),
                    7 => I18n.Dialog_Greeting7(name: Game1.player.Name),
                    _ => I18n.Dialog_Greeting1(name: Game1.player.Name),
                };
            }
            else if (messageStoreName == "unfinishedmoney")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Unfinishedmoney1(),
                    2 => I18n.Dialog_Unfinishedmoney2(),
                    3 => I18n.Dialog_Unfinishedmoney3(),
                    4 => I18n.Dialog_Unfinishedmoney4(),
                    5 => I18n.Dialog_Unfinishedmoney5(),
                    6 => I18n.Dialog_Unfinishedmoney6(),
                    7 => I18n.Dialog_Unfinishedmoney7(),
                    8 => I18n.Dialog_Unfinishedmoney8(),
                    _ => I18n.Dialog_Unfinishedmoney1(),
                };
            }
            else if (messageStoreName == "unfinishedinventory")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Unfinishedinventory1(),
                    2 => I18n.Dialog_Unfinishedinventory2(),
                    3 => I18n.Dialog_Unfinishedinventory3(),
                    4 => I18n.Dialog_Unfinishedinventory4(),
                    _ => I18n.Dialog_Unfinishedinventory1(),
                };
            }
            else if (messageStoreName == "smalltalk")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Smalltalk1(),
                    2 => I18n.Dialog_Smalltalk2(),
                    3 => I18n.Dialog_Smalltalk3(),
                    4 => I18n.Dialog_Smalltalk4(),
                    5 => I18n.Dialog_Smalltalk5(),
                    6 => I18n.Dialog_Smalltalk6(),
                    7 => I18n.Dialog_Smalltalk7(),
                    8 => I18n.Dialog_Smalltalk8(),
                    9 => I18n.Dialog_Smalltalk9(),
                    10 => I18n.Dialog_Smalltalk10(),
                    11 => I18n.Dialog_Smalltalk11(),
                    12 => I18n.Dialog_Smalltalk12(),
                    13 => I18n.Dialog_Smalltalk13(),
                    14 => I18n.Dialog_Smalltalk14(),
                    _ => I18n.Dialog_Smalltalk1(),
                };
            }
            else if (messageStoreName == "Shane")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Shane1(),
                    2 => I18n.Dialog_Shane2(),
                    _ => I18n.Dialog_Shane1(),
                };
            }
            else if (messageStoreName == "Haley")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Haley1(),
                    2 => I18n.Dialog_Haley2(),
                    _ => I18n.Dialog_Haley1(),
                };
            }
            else if (messageStoreName == "Alex")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Alex1(),
                    _ => I18n.Dialog_Alex1(),
                };
            }
            else if (messageStoreName == "Leah")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Leah1(),
                    _ => I18n.Dialog_Leah1(),
                };
            }
            else if (messageStoreName == "Marnie")
            {
                return rand switch
                {
                    1 => I18n.Dialog_Marnie1(),
                    _ => I18n.Dialog_Marnie1(),
                };
            }
            else
            {
                return "...$h#$e#";
            }
            
        }

    }
}
