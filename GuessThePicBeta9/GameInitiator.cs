using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuessThePicBeta9
{
    public class GameInitiator
    {
        public GameInitiator() {  }
        public string GetNewGameID()
        {
            Random random = new Random();

            int gameID = random.Next(100000, 1000000);
            return gameID.ToString();
        }
    }
}