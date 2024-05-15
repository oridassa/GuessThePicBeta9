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
using System.Runtime.InteropServices.ComTypes;
using static Java.Util.Jar.Attributes;
using static Android.Graphics.ColorSpace;
using Java.Nio.FileNio;
using AndroidX.Interpolator.View.Animation;


namespace GuessThePicBeta9
{
    public static class FirebaseActions
    {
        private static FirebaseClient firebaseClient = new
            FirebaseClient("https://guessthepic-4d369-default-rtdb.europe-west1.firebasedatabase.app/");
        private static ChildQuery pointer; //current game json pointer 

        public static async Task<bool> UploadGamesEngine(GameEngine g) //uploads the gameEngine to firebase
        {
            await pointer.Child("GameEngine").PutAsync<GameEngine>(g);
            return true;
        }
        public static async Task<bool> GameSetup(string gameID, GameEngine gameEngine = null) //sets up all of the flags for the game
        {
            SetDatabasePointer(gameID);
            if (CurrentPlayer.playerPointer.isAdmin == true)
            {
                await pointer.Child("GameStarted").PutAsync(false);
                await pointer.Child("DownloadGameEngine").PutAsync(false);
                await pointer.Child("IsLobbyOn").PutAsync(true);
                await pointer.Child("MoveToNextRound").PutAsync<bool>(false);
                await UploadGamesEngine(gameEngine);
            }

            return true;
        }
        public static void SetDatabasePointer(string gameID) //sets the pointer to the current game Json
        {
            pointer = firebaseClient.Child("Games").Child(gameID);
        }
        public static async Task<bool> UploadImageUser(Image img) //adds an image to the database
        {
            var result = await pointer
                .Child("UsersPictures")
                .Child($"{CurrentPlayer.playerPointer.name}")
                .PostAsync<Image>(img);
            AddToImagePointerDictionary(result.Key);
            return true;
        }
        public static async void AddToImagePointerDictionary(string imageKey)
        {
            ImageDatabasePointer dbPointer = new ImageDatabasePointer(imageKey);
            
            await pointer
                .Child("ImagePointerDictionary")
                .Child(CurrentPlayer.name)
                .PostAsync(dbPointer);
        }
        public static async Task<bool> KillLobby() //ends the lobby
        {
            await pointer.Child("IsLobbyOn").PutAsync<bool>(false); 
            pointer = null;
            return true;
        }
        public static async Task<bool> IsGameOnline(string gameID) //checks in a game is on
        {
            List<string> listOfKeys = await GetActiveGames();
            return listOfKeys.Contains(gameID);

        }
        public static async Task<List<string>> GetActiveGames() //gets a list of the active games
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
        public static async Task<GameEngine> GetGameEngine() //returns the gameEngine
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

        public static async Task<string[]> GetPlayerNamesArray()
        {
            Dictionary<string, Player> players = null;
            if (pointer != null)
                players = await GetPlayersList();

            return players.Keys.ToArray<string>();
        }
        public static async void ExitFromLobby(string name) //exits from a lobby
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
        public static async Task<bool> IsGameStarted()
        {
            if(pointer != null) 
                return await pointer
                    .Child("GameStarted")
                    .OnceSingleAsync<bool>();
            return false;
        }
        public static async Task<bool> ShouldDownloadGameEngine() //check if a player should download the GameEngine
        {
            bool? flag;
            if (pointer != null)
            {
                flag = await pointer
                    .Child("DownloadGameEngine")
                    .OnceSingleAsync<bool>();
                if(flag == null) return false;
                if(flag == true) return true;
            }
            return false;
        }
        public static bool IsPointerNull()
        {
            return pointer == null;
        }

        public static async Task<List<ImageDatabasePointer>> GetImageDatabasePointerList() //gets a list of all of the DatabaseImagePointers
        {
            string jsonString = await pointer.
                Child("ImagePointerDictionary")
                .OnceAsJsonAsync();
            return ExtractImagesFromJson(jsonString);
        }
        public static List<ImageDatabasePointer> ExtractImagesFromJson(string jsonString) 
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

        public static async void StartGame() //changes the GameStarted flag to true
        {
            await pointer
                .Child("GameStarted")
                .PutAsync<bool>(true);
        }
        public static async void StartGameEngineDownload() //changes the DownloadGameEngine flag to true
        {
            await pointer
                .Child("DownloadGameEngine")
                .PutAsync<bool>(true);
        }
        public static async Task<Image> GetImageByPointer(ImageDatabasePointer imagePointer) //gets an image from the database with ImageDatabasePointer
        {
            return await pointer
                .Child(imagePointer.First())
                .Child(imagePointer.Second())
                .Child(imagePointer.Third())
                .OnceSingleAsync<Image>();
        }
        public static async void UploadNamePointsString() //for current user
        {
            await pointer
                .Child("PointString")
                .Child(CurrentPlayer.name)
                .PutAsync<string>($"{CurrentPlayer.name}: {CurrentPlayer.playerPointer.points}");
        }
        public static async Task<string> GetPointsString()
        {
            string jsonString = await pointer.
                Child("PointString")
                .OnceAsJsonAsync();
            List<string> lst = SortByPoints(ExtractListStringFromJson(jsonString)); //for the list of points to be sorted
            string s = "";
            foreach (string playerPoints in lst) 
            {
                s += playerPoints;
                s += "\n";
            }
            return s;
        }
        public static List<string> SortByPoints(List<string> data) //sorts the points string by points
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.OrderByDescending(x => int.Parse(x.Split(':')[1].Trim())).ToList(); //lambda
        }
        public static List<string> ExtractListStringFromJson(string jsonString)
        {
            List<string> strings = new List<string>();
            try
            {
                Dictionary<string, string> jsonObj =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                foreach (var str in jsonObj.Values)
                {
                    strings.Add(str);
                }
               
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
            }

            return strings;
        }
        public static async void SetPlayerReady() 
        {
            await pointer
                .Child("GameEngine")
                .Child("Players")
                .Child(CurrentPlayer.name)
                .Child("Ready")
                .PutAsync<bool>(true);
        }

        public static async Task<bool> IsPlayerReady(string name) //checks if a certain player is ready
        {
            return await pointer
                .Child("GameEngine")
                .Child("Players")
                .Child(name)
                .Child("Ready")
                .OnceSingleAsync<bool>();
        }

        public static async Task<bool> IsAllReady(IList<string> playersList)//checks if all of the players are ready
        {
            foreach(string player in playersList)
            {
                if(!await IsPlayerReady(player))
                {
                    return false;
                }
            }
            return true;
        }
        public static async void SetPlayerDownloadedGameEngine() //saying that the currentPlayer downloaded the gameEngine
        {
            await pointer
                .Child("GameEngine")
                .Child("Players")
                .Child(CurrentPlayer.name)
                .Child("GameEngineDownloaded")
                .PutAsync<bool>(true);
        }
        public static async Task<bool> IsPlayerDownloadedGameEngine(string name)
        {
            return await pointer
                .Child("GameEngine")
                .Child("Players")
                .Child(name)
                .Child("GameEngineDownloaded")
                .OnceSingleAsync<bool>();
        }
        public static async Task<bool> IsDownloadedGameEngine(IList<string> playersList)
        {
            foreach (string player in playersList)
            {
                if (!await IsPlayerDownloadedGameEngine(player))
                {
                    return false;
                }
            }
            return true;
        }
        public static async Task<bool> IsLobbyOn()
        {
            return await pointer
                .Child("IsLobbyOn")
                .OnceSingleAsync<bool>();
        }
        public static async void SetMoveToNextRoundFalse()
        {
            await pointer
                .Child("MoveToNextRound")
                .PutAsync<bool>(false);
        }
        public static async Task<bool> CanContinueToNextRound()
        {
            return await pointer
                .Child("MoveToNextRound")
                .OnceSingleAsync<bool>();
        }
        public static async void AllowPlayersToContinue()
        {
            await pointer
                .Child("MoveToNextRound")
                .PutAsync<bool>(true);
        }

        public static async Task<bool> CanConnectToLobby(string gameID) //checking if a player can connect to a lobby by GameID
        {
            ChildQuery tempPointer = firebaseClient.Child("Games").Child(gameID);
            if (await tempPointer.Child("IsLobbyOn").OnceSingleAsync<bool>() == false) return false;
            if (await tempPointer.Child("DownloadGameEngine").OnceSingleAsync<bool>() == true) return false;
            return true; //otherwise 
        }
    }
}