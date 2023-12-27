using System;
using Android.App;
using Android.Content;
using Android.Media.TV;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;


namespace GuessThePicBeta9
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, View.IOnClickListener
    {
        private EditText nameInput;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            Button btn = FindViewById<Button>(Resource.Id.check);
            btn.Click += RequestPrem;

            nameInput = FindViewById<EditText>(Resource.Id.name);
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
    }
}