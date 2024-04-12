using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuessThePicBeta9
{
    static class MusicServiceSingleton
    {
        private static bool IsMusicOn = false;

        static public bool ShouldTurnOnMusic()
        {
            if(IsMusicOn == true)
            {
                IsMusicOn = false;
                return false;
            }
            else
            {
                IsMusicOn = true;
                return true;
            }
        }
    }
}