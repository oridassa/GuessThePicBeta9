using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuessThePicBeta9
{
    class GameEngineSingleton
    {
        private static GameEngine instance;
        private static object lockObject = new object();

        private GameEngineSingleton() { }
        public static GameEngine GetInstance()
        {
            lock (lockObject)
            {
                if (instance == null)
                    instance = new GameEngine();
                return instance;
            }
        }
        public static void DeleteInstance()
        {
            instance = null;
        }
        public static void SetInstance(GameEngine obj)
        {
            instance = obj;
        }

    }
}