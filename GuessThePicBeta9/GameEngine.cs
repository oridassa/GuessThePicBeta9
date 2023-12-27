using Android.App;
using Android.App.AppSearch;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuessThePicBeta9
{
    public class GameEngine
    {
        public IList<Image> ImageList { get; private set; }
        public int CurrentImageIndex {  get; private set; }
        public IList<Player> Players {  get; private set; } 
        public string ID { get; set; }
        public GameEngine() 
        {
            this.ImageList = new List<Image>();
            this.Players = new List<Player>() { CurrentPlayer.playerPointer};
            this.CurrentImageIndex = 0;
        }
        public GameEngine(IList<Image> imageList)
        {
            this.ImageList = imageList;
            this.CurrentImageIndex = 0;
        }
        public void AddImage(Image image)
        {
            this.ImageList.Add(image);
        }
        public void NextImage()
        {
            this.CurrentImageIndex++;
        }
        public Image GetCurrentImage()
        {
            return this.ImageList[this.CurrentImageIndex];
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
            return ImageList.Count > 0;
        }
        public string GetScoreString()
        {
            string s = "";
            foreach (Player player in Players)
            {
                s += player.ToString();
                s += "\n";
            }
            return s;
        }
    }
}