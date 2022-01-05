using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;

namespace PotionHints
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }

        public string roomName;


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;
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
                            // Get the name of Potion Quest
                            var questDescription = Managers.Dialogue.GetQuestDescriptionKey();
                            string quest = questDescription.Substring(11);

                            // Get the plugin location
                            string pluginLoc = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                            // Grab the hint from the JSON file
                            JObject questObj = JObject.Parse(File.ReadAllText(pluginLoc + "/hints.json"));
                            string questHint = (string)questObj[quest]["hint"];

                            Log.LogInfo($"Quest Name: {quest} hint is: {questHint}");
                            Log.LogInfo($"Quest Hint is: {questHint}");

                            // Show the hint to the player
                            Notification.ShowText("Potion Hint", questHint, Notification.TextType.EventText);

                            // Have the player make a payment for the hint
                            Managers.Player.AddGold(-30);
                        }else
                        {
                            Notification.ShowText("Potion Hint", "You do not have enough gold for a hint", Notification.TextType.EventText);
                        }
                    }
                }
            }
        }
    }
}
