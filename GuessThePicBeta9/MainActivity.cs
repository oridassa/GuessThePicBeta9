using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Media.TV;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;


namespace GuessThePicBeta9
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, View.IOnClickListener
    {
        private EditText nameInput;
        private ImageButton imageb;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);
                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.activity_main);

                //Button btn = FindViewById<Button>(Resource.Id.check);
                //btn.Click += RequestPrem;

                nameInput = FindViewById<EditText>(Resource.Id.name);
                imageb = FindViewById<ImageButton>(Resource.Id.MusicStatus);
            }
            catch (Exception e)
            {
                Toast.MakeText(this, e.Message, ToastLength.Short).Show();
            }
            
            
            if (MusicServiceSingleton.DidMusicTurnOn == false)
            {
                StartMusic();
                MusicServiceSingleton.DidMusicTurnOn = true;
            }
            else if(MusicServiceSingleton.IsMusicOn == false)
            {
                imageb.SetImageResource(Resource.Drawable.soff);
            }
        }

        private async void RequestPrem(object sender, EventArgs e) //checks if the user gives the app premission to use his pictures. 
        {
            var premission = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            Toast.MakeText(this, premission.ToString(), ToastLength.Short).Show();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public void OnClick(View v)
        {
            if (v.Id == Resource.Id.MusicStatus)
            {
                ImageButton ib = (ImageButton)v;
                ChangeMusicStatus(ib);
                return;
            }
            string name = nameInput.Text.ToString();
            Intent intent;
            Button b = (Button)v;
            if (b.Text == "Create a game")
            {
                if (name == "")
                {
                    Toast.MakeText(this, "please enter a name", ToastLength.Short).Show();
                }
                else
                {
                    CurrentPlayer.InitiatePlayer(name, true);
                    intent = new Intent(this, typeof(GameLobbyHost));
                    base.StartActivity(intent);
                }
            }
            else if (b.Text == "Join a game")
            {
                if (name == "")
                {
                    Toast.MakeText(this, "please enter a name", ToastLength.Short).Show();
                }
                else
                {
                    CurrentPlayer.InitiatePlayer(name, false);
                    intent = new Intent(this, typeof(LoginActivity));
                    base.StartActivity(intent);
                }
            }
        }

        private void ChangeMusicStatus(ImageButton ib)
        {
            if (MusicServiceSingleton.ShouldTurnOnMusic())
            {
                StartMusic();
                ib.SetImageResource(Resource.Drawable.son);
            }
            else
            {
                StopMusic();
                ib.SetImageResource(Resource.Drawable.soff);
            }
        }
        private void StopMusic()
        {
            StopService(new Intent(this, typeof(BackgroundMusicService)));
        }

        private void StartMusic()
        {
            StartService(new Intent(this, typeof(BackgroundMusicService)));
        }
    }
}