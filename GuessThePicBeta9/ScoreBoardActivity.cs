﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media.Metrics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Nio.Channels;
using Java.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Xamarin.Essentials.Platform;

namespace GuessThePicBeta9
{
    [Activity(Label = "ScoreBoardActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ScoreBoardActivity : Activity
    {
        GameEngine gameEngine;
        TextView score; //score of all of the players
        Android.OS.Handler handler;
        TextView roundCounter; //current rount

        private int loopLength = 1000; //length of the loop used to time this screen
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.score_board);

            

            roundCounter = FindViewById<TextView>(Resource.Id.roundcount);
            score = FindViewById<TextView>(Resource.Id.score);

            gameEngine = GameEngineSingleton.GetInstance();

            handler = new Android.OS.Handler();


            if (!gameEngine.IsLastImage())
                SetRoundCounter();
            else
            {
                loopLength = 2000;
                SetFinaleScreen();
            }

            await SetScoreBoard();

            var thread = new Thread(HostTimerControl);

            thread.Start();

            WaitForNextRound();

        }
        public override void OnBackPressed() //Disables the back button
        {
        }
        private void SetFinaleScreen()
        {
            roundCounter.Text = $"Game Ended\nFinal Scoreboard";
        }

        protected override void OnStart()
        {
            base.OnStart();
        }
        public async void WaitForNextRound() //waits for the flag in the database to change
        {
            while (await FirebaseActions.CanContinueToNextRound() == false)
            {
                Thread.Sleep(100);
            }
            Logic();
        }
        public void HostTimerControl() //keeps the time to change the flag as the host
        {
            for (int i = 0; i <= loopLength; i++)
                Thread.Sleep(5);
            FirebaseActions.AllowPlayersToContinue();
        }
        public void WriteScore() 
        {
            this.score.Text = $"{CurrentPlayer.playerPointer.name}: {CurrentPlayer.playerPointer.points}";
        }
        public void Logic() //whats happen when the scoreboard screen ends
        {
            if (gameEngine.IsLastImage())
            {
                FinishGame();
            }
            else
            {
                gameEngine.NextImage();
                Android.Content.Intent intent = new Android.Content.Intent(this, typeof(GameplayScreen));
                base.StartActivity(intent);
            }
        }
        public async void FinishGame() //if its the last scoreboard
        {
            if (CurrentPlayer.playerPointer.isAdmin) await FirebaseActions.KillLobby();
            CurrentPlayer.DeletePlayerInstance();
            Android.Content.Intent intent = new Android.Content.Intent(this, typeof(MainActivity));
            base.StartActivity(intent);
        }
        public void SetRoundCounter()
        {
            roundCounter.Text = $"Score : Round {gameEngine.GetRoundNum()}";
        }
        public async Task<bool> SetScoreBoard()
        {
            string s = await FirebaseActions.GetPointsString();
            score.Text = s;
            return true;
        }
    }
}