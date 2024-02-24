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
    public class Player
    {
        public string name { get; private set; }
        public bool isAdmin { get; private set; }
        public int points { get; private set; }
        public bool? Ready { get; set; }

        public Player(string name, bool isAdmin)
        {
            this.name = name;
            this.isAdmin = isAdmin;
            this.points = 0;
            this.Ready = false;
        }

        public void AddPoints(int timePoints)
        {
            int addedPoints = 1000 - (timePoints / 2);
            this.points += addedPoints;
        }
        public override string ToString()
        {
            return $"{name}: {points}";
        }
    }
}