using Android.App;
using Android.App.AppSearch;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessThePicBeta9
{
    public class GameEngine
    {
        public IList<ImageDatabasePointer> ImageList { get; private set; } //list of all of the Images used in the game 
        public int CurrentImageIndex { get; private set; } 
        public Dictionary<string, Player> Players { get; private set; } //כל השחקנים
        public string ID { get; set; }
        public Image CurrentImage { get; set; }

        public GameEngine()
        {
            this.ImageList = new List<ImageDatabasePointer>();
            this.Players = new Dictionary<string, Player>();
            this.Players.Add($"{CurrentPlayer.playerPointer.name}", CurrentPlayer.playerPointer);
            this.CurrentImageIndex = 0;
        }

        public void NextImage()
        {
            this.CurrentImageIndex++;
        }
        public Image GetCurrentImage()
        {
            return CurrentImage;
        }
        public string GetCurrentImagePlayerName()
        {
            return this.GetCurrentImage().SourcePlayer;
        }
        public bool IsLastImage()
        {
            return ImageList.Count == (CurrentImageIndex + 1);
        }
        public int GetRoundNum()
        {
            return CurrentImageIndex + 1;
        }
        public bool HasImages()
        {
            return this.ImageList.Count > 0;
        }
        public string GetScoreString()
        {
            string s = "";
            foreach (KeyValuePair<string, Player> player in Players)
            {
                s += player.Value.ToString();
                s += "\n";
            }
            return s;
        }
        public void SetImageList(List<ImageDatabasePointer> ImageList)
        {
            this.ImageList = ImageList;
        }
        public void ScambleImageList() //מארבב את רשימת התמונות
        {
            System.Random rng = new System.Random();
            int n = ImageList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                // Swap elements at indices k and n
                ImageDatabasePointer temp = ImageList[k];
                ImageList[k] = ImageList[n];
                ImageList[n] = temp;
            }
        }
        public async Task<bool> SetCurrentImage()
        {
            CurrentImage = await FirebaseActions.GetImageByPointer(ImageList[CurrentImageIndex]);
            return true;
        }
        public async void SetNextImage()
        {
            CurrentImage = await FirebaseActions.GetImageByPointer(ImageList[CurrentImageIndex + 1]);
        }
    }
}