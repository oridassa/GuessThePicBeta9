using Android.App.AppSearch;
using Android.Locations;
using Firebase.Database;
using Firebase.Database.Query;
using Java.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuessThePicBeta9
{
    public static class FirebaseActions
    {
        private static FirebaseClient firebaseClient = new
            FirebaseClient("https://guessthepic-4d369-default-rtdb.europe-west1.firebasedatabase.app/");
        private static ChildQuery pointer;
        public static string GetPlayersArray()
        {
            throw new NotImplementedException();
        }
        public static void UploadGamesEngine(GameEngine g)
        {
            pointer.Child("GameEngine").PutAsync<GameEngine>(g);
        }
        public static async Task<bool> GameSetup(string gameID, GameEngine gameEngine = null)
        {
            SetDatabasePointer(gameID);
            if (CurrentPlayer.playerPointer.isAdmin == true)
            {
                await pointer.Child("GameStarted").PutAsync(false);
                UploadGamesEngine(gameEngine);
            }

            return true;
            //await pointer.Child(gameID).Child("Images").PutAsync();
            
        }
        public static void SetDatabasePointer(string gameID)
        {
            pointer = firebaseClient.Child("Games").Child(gameID);
        }
        public static async void UploadImageUser(Image img)
        {
            //TODO: Add img to a static location at the firebase database where the host could add the pic to the game engine
            await pointer.Child("UsersPictures").PostAsync<Image>(img);
        }
        public static async void KillLobby()
        {
            await pointer.DeleteAsync();
            pointer = null;
        }
        public static async Task<bool> IsGameOnline(string gameID)
        {
            List<string> listOfKeys = await GetActiveGames();
            return listOfKeys.Contains(gameID);

        }
        public static async Task<List<string>> GetActiveGames()
        {
            List<string> listOfKeys = ExtractKeysFromJson(await firebaseClient.Child("Games").OnceAsJsonAsync());
            return listOfKeys;
        }
        public static List<string> ExtractKeysFromJson(string jsonString)
        {
            List<string> keys = new List<string>();

            try
            {
                // Parse the JSON string into a JObject
                JObject json = JObject.Parse(jsonString);

                // Extract keys from the first layer only
                foreach (var property in json.Properties())
                {
                    keys.Add(property.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
            }

            return keys;
        }
        public static async Task<GameEngine> GetGameEngine()
        {
            GameEngine gameEngine = await pointer.Child("GameEngine").OnceSingleAsync<GameEngine>();
            return gameEngine;
        }
        public static async void AddPlayerToLobby(Player player)
        {
            int indexOfNewPlayer = await GetLastIndexOfPlayer();
            await pointer
                .Child("GameEngine")
                .Child("Players")
                .Child($"{indexOfNewPlayer + 1}")
                .PutAsync<Player>(player);
        }
        public static async Task<List<Player>> GetPlayersList()
        {
            List<Player> lst = await pointer
                        .Child("GameEngine")
                        .Child("Players").OnceSingleAsync<List<Player>>();
            return lst;
        }
        public static async Task<int> GetLastIndexOfPlayer() //returns the index of the last player who joined a game
        {
            List<Player> lst = await GetPlayersList();
            return lst.Count() - 1;
        }
        public static async Task<string[]> GetPlayerNamesArray()
        {
            List<Player> players = await GetPlayersList();
            List<string> playersNames = new List<string>();
            foreach (Player player in players)
            {
                playersNames.Add(player.name);
            }
            return playersNames.ToArray();
        }
    }
}