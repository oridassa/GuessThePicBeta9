using Android.App.AppSearch;
using Android.Locations;
using Android.Media;
using Firebase.Database;
using Firebase.Database.Query;
using Java.Lang;
using Java.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
            await pointer
                .Child("UsersPictures")
                .Child($"{CurrentPlayer.playerPointer.name}")
                .PostAsync<Image>(img);
        }
        public static async Task<bool> KillLobby()
        {
            await pointer.DeleteAsync();
            pointer = null;
            return true;
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
            catch (System.Exception ex)
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
            await pointer
                .Child("GameEngine")
                .Child("Players")
                .Child($"{player.name}")
                .PutAsync<Player>(player);
        }
        public static async Task<Dictionary<string, Player>> GetPlayersList()
        {
            Dictionary<string, Player> lst = await pointer
                        .Child("GameEngine")
                        .Child("Players").OnceSingleAsync<Dictionary<string, Player>>();
            return lst;
        }
        //public static async Task<int> GetLastIndexOfPlayer() //returns the index of the last player who joined a game
        //{
        //    List<Player> lst = await GetPlayersList();
        //    return lst.Count() - 1;
        //}
        public static async Task<string[]> GetPlayerNamesArray()
        {
            Dictionary<string, Player> players = await GetPlayersList();
            
            return players.Keys.ToArray<string>();
        }
        public static async void ExitFromLobby(string name)
        {
            await pointer
                .Child("GameEngine")
                .Child("Players")
                .Child($"{name}")
                .DeleteAsync();
            DeletePicturesFromLobby(name);
        }
        public static void ExitFromLobby(Player player)
        {
            ExitFromLobby(player.name);
        }
        
        public static async void DeletePicturesFromLobby(string name)
        {
            await pointer
                .Child("UsersPictures")
                .Child($"{name}")
                .DeleteAsync();
        }
        public static void SubscribeTothePlayersList(Action<string[]> setPlayersList)
        {
            pointer
                .Child("GameEngine")
                .Child("Players")
                .AsObservable<Dictionary<string, Player>>()
                .Subscribe(players =>
                {
                    string[] arr = players.Object.Keys.ToArray();
                    setPlayersList(arr);
                });
        }
        public static async Task<bool> IsGameStarted()
        {
            return await pointer
                .Child("GameStarted")
                .OnceSingleAsync<bool>();
        }
    }
}