using Android.App;
using Android.Content;
using Android.Content.PM;
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
using System.Threading.Tasks;
using static Xamarin.Essentials.Platform;

namespace GuessThePicBeta9
{
    [Activity(Label = "ScoreBoardActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ScoreBoardActivity : Activity
    {
        GameEngine gameEngine;
        TextView score;
        Android.OS.Handler handler;
        private ProgressBar progressBar;
        TextView roundCounter;

        private int loopLength = 1000;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.score_board);

            roundCounter = FindViewById<TextView>(Resource.Id.roundcount);
            score = FindViewById<TextView>(Resource.Id.score);
            this.progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            gameEngine = GameEngineSingleton.GetInstance();

            handler = new Android.OS.Handler();


            if (!gameEngine.IsLastImage())
                SetRoundCounter();
            else
            {
                progressBar.Max = 2000;
                loopLength = 2000;
                SetFinaleScreen();
            }

            await SetScoreBoard();

            var thread = new Thread(Timer);

            thread.Start();
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
        public void Timer()
        {
            for (int i = 0; i <= loopLength; i++)
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