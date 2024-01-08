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
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Android.Security.Identity;
using System.Linq.Expressions;
using AndroidX.AppCompat.View.Menu;


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
        public static async Task<bool> UploadGamesEngine(GameEngine g)
        {
            await pointer.Child("GameEngine").PutAsync<GameEngine>(g);
            return true;
        }
        public static async Task<bool> GameSetup(string gameID, GameEngine gameEngine = null)
        {
            SetDatabasePointer(gameID);
            if (CurrentPlayer.playerPointer.isAdmin == true)
            {
                await pointer.Child("GameStarted").PutAsync(false);
                await UploadGamesEngine(gameEngine);
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
            var result = await pointer
                .Child("UsersPictures")
                .Child($"{CurrentPlayer.playerPointer.name}")
                .PostAsync<Image>(img);
            AddToImagePointerDictionaryt(result.Key);
        }
        public static async void AddToImagePointerDictionaryt(string imageKey)
        {
            ImageDatabasePointer dbPointer = new ImageDatabasePointer(imageKey);
            
            await pointer
                .Child("ImagePointerDictionary")
                .Child(CurrentPlayer.name)
                .PostAsync(dbPointer);
        }
        public static async Task<bool> KillLobby()
        {
            await pointer.DeleteAsync();
            //pointer = null;
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
            DeleteImagePointerFromLobby(name);
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
        public static async void DeleteImagePointerFromLobby(string name)
        {
            await pointer
                .Child("ImagePointerDictionary")
                .Child(name)
                .DeleteAsync();
        }
        public static IDisposable SubscribeTothePlayersList(Action<string[]> setPlayersList)
        {
            return pointer
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
        public static bool IsPointerNull()
        {
            return pointer == null;
        }
        //public static async Task<List<Image>> GetUsersImages()
        //{
        //    string jsonString = await pointer.
        //        Child("UsersPictures")
        //        .OnceAsJsonAsync();
        //    return ExtractImagesFromJson<Image>(jsonString);
        //}
        public static async Task<List<ImageDatabasePointer>> GetImageDatabasePointerList()
        {
            string jsonString = await pointer.
                Child("ImagePointerDictionary")
                .OnceAsJsonAsync();
            return ExtractImagesFromJson(jsonString);
        }
        public static List<ImageDatabasePointer> ExtractImagesFromJson(string jsonString) //this function finally works!!!!
        {
            List<ImageDatabasePointer> images = new List<ImageDatabasePointer>();
            try
            {
                Dictionary<string, Dictionary<string, ImageDatabasePointer>> jsonObj = 
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ImageDatabasePointer>>>(jsonString);
                
                foreach (var userImages in jsonObj.Values)
                {
                    foreach (var image in userImages.Values)
                    {
                        images.Add(image);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
            }

            return images;
        }

        public static async void StartGame()
        {
            await pointer
                .Child("GameStarted")
                .PutAsync<bool>(true);
        }
        public static async Task<Image> GetImageByPointer(ImageDatabasePointer imagePointer)
        {
            return await pointer
                .Child(imagePointer.First())
                .Child(imagePointer.Second())
                .Child(imagePointer.Third())
                .OnceSingleAsync<Image>();
        }
    }
}