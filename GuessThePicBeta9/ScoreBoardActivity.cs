using Android.App;
using Android.Content;
using Android.Media.Metrics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static Xamarin.Essentials.Platform;

namespace GuessThePicBeta9
{
    [Activity(Label = "ScoreBoardActivity")]
    public class ScoreBoardActivity : Activity
    {
        GameEngine gameEngine;
        TextView score;
        Android.OS.Handler handler;
        private ProgressBar progressBar;
        TextView roundCounter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.score_board);

            roundCounter = FindViewById<TextView>(Resource.Id.roundcount);
            score = FindViewById<TextView>(Resource.Id.score);
            this.progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            gameEngine = GameEngineSingleton.GetInstance();

            handler = new Android.OS.Handler();

            SetRoundCounter();

            SetScoreBoard();

            var thread = new Thread(Timer);

            thread.Start();
        }
        protected override void OnStart()
        {
            base.OnStart();

        }
        public void Timer()
        {
            for (int i = 0; i <= 1000; i++)
            {
                // Update progress on UI thread
                handler.Post(() => progressBar.Progress = i);

                // Sleep for a short duration to simulate progress
                Thread.Sleep(5);
            }

            // After 5 seconds, post the function to be executed on the UI thread
            handler.Post(() => Logic());
        }
        public void WriteScore()
        {
            this.score.Text = $"{CurrentPlayer.playerPointer.name}: {CurrentPlayer.playerPointer.points}";
        }
        public void Logic()
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
        public async void FinishGame()
        {
            if (CurrentPlayer.playerPointer.isAdmin) await FirebaseActions.KillLobby();
            Android.Content.Intent intent = new Android.Content.Intent(this, typeof(MainActivity));
            base.StartActivity(intent);
        }
        public void SetRoundCounter()
        {
            roundCounter.Text = $"Score : Round {gameEngine.GetRoundNum()}";
        }
        public void SetScoreBoard()
        {
            string s = "";
            //***TODO: create a List<Player> of all of the players and do the same for all of them
            s += CurrentPlayer.playerPointer.ToString();
            score.Text = s;
        }
    }
}