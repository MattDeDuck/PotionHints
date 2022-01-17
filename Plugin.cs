using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;
using ObjectBased.UIElements;
using ObjectBased.UIElements.ConfirmationWindow;
using LocalizationSystem;

namespace PotionHints
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }

        public string roomName;


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            LocalizationSystem.LocalizationSystem.textData[LocalizationSystem.LocalizationSystem.Locale.en].AddText("ph_hintboxTitle", "Potion Hints");
            LocalizationSystem.LocalizationSystem.textData[LocalizationSystem.LocalizationSystem.Locale.en].AddText("ph_hintboxText", "Would you like a hint? It would cost 30 Gold.");
            LocalizationSystem.LocalizationSystem.textData[LocalizationSystem.LocalizationSystem.Locale.en].AddText("ph_nofundsText", "You do not have enough gold for a hint!");
        }

        public void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.H))
            {
                // Get the name of the current room
                roomName = Managers.Room.settings.rooms[(int)Managers.Room.currentRoom].name;
                // Get the state of the dialogue
                string dialogueState = Managers.Dialogue.State.ToString();

                if (roomName == "MeetingRoom") // If we're in the shop
                {
                    if(dialogueState == "PotionRequest") // If it's a potion request
                    {
                        // Get the players current gold amount
                        int currentPlayerGold = Managers.Player.Gold;

                        if(currentPlayerGold >= 30)
                        {
                            ConfirmationWindow.Show(DarkScreen.Layer.Upper, new Key("ph_hintboxTitle", null), new Key("ph_hintboxText", null), Managers.Game.settings.confirmationWindowPosition, delegate ()
                            {
                                // Get the name of Potion Quest
                                var questDescription = Managers.Dialogue.GetQuestDescriptionKey();
                                string quest = questDescription.Substring(11);

                                // Get the plugin location
                                string pluginLoc = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                                // Grab the hint from the JSON file
                                JObject questObj = JObject.Parse(File.ReadAllText(pluginLoc + "/hints.json"));
                                string questHint = (string)questObj[quest]["hint"];

                                Log.LogInfo($"Quest Name: {quest}");
                                Log.LogInfo($"Quest Hint: {questHint}");

                                // Have the player make a payment for the hint
                                Managers.Player.AddGold(-30);
                                Log.LogInfo($"30 Gold payed");

                                // Show the hint to the player
                                Notification.ShowText("Potion Hint", questHint, Notification.TextType.EventText);
                            }, null);
                        }else
                        {
                            ConfirmationWindow.Show(DarkScreen.Layer.Upper, new Key("ph_hintboxTitle", null), new Key("ph_nofundsText", null), Managers.Game.settings.confirmationWindowPosition, null);
                        }
                    }
                }
            }
        }
    }
}