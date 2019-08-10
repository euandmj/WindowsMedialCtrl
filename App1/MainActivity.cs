﻿using System.Linq;
using System.Text;
using System.Net.Sockets;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using Android.Content;
using Android.Preferences;
using System.Threading.Tasks;

namespace App1
{
    [Activity(Label = "Shutup Netflix", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        const string DEFAULT_HOSTNAME = "192.168.1.11";
        const int DEFAULT_PORT = 54001;
        string hostname;
        int port;
        TcpClient client;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            //textMessage = FindViewById<TextView>(Resource.Id.message);
            //BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            //navigation.SetOnNavigationItemSelectedListener(this);

            //swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.rootLayout);
            //swipeRefreshLayout.SetColorSchemeColors(new int[] { Android.Resource.Color.BackgroundLight });
            //swipeRefreshLayout.Refresh ;

            // First time initialisation
            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            bool b = prefs.GetBoolean(key: "firststart", defValue: true);

            if (b)
            {
                ShowConfigureDialog(prefs);
            }
            hostname = prefs.GetString(key: "hostname", defValue: DEFAULT_HOSTNAME);
            port = prefs.GetInt(key: "port", defValue: DEFAULT_PORT);


            // events             
            ((Button)FindViewById(Resource.Id.buttonReduceVol)).Click += this.ReduceVol_Click;
            ((Button)FindViewById(Resource.Id.buttonMute)).Click += this.Mute_Click;
            ((Button)FindViewById(Resource.Id.buttonIncrVol)).Click += this.IncrVol_Click;
            ((Button)FindViewById(Resource.Id.buttonBack)).Click += this.Back_Click;
            ((Button)FindViewById(Resource.Id.buttonForwards)).Click += this.Forwards_Click;
            ((Button)FindViewById(Resource.Id.buttonPause)).Click += this.Pause_Click;

            if(Connect() == 0)
                Toast.MakeText(this, "Connected!", ToastLength.Short).Show();
        }

        private void ShowConfigureDialog(ISharedPreferences prefs)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            View view = layoutInflater.Inflate(Resource.Layout.hostname_input_box, null);
            Android.Support.V7.App.AlertDialog.Builder alertBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertBuilder.SetView(view);

            var hostinput = view.FindViewById<EditText>(Resource.Id.hostnameText);
            var portinput = view.FindViewById<EditText>(Resource.Id.portText);

            hostinput.Text = prefs.GetString(key: "hostname", defValue: DEFAULT_HOSTNAME);
            portinput.Text = prefs.GetInt(key: "port", defValue: DEFAULT_PORT).ToString();

            alertBuilder.SetCancelable(false).SetPositiveButton("Submit", delegate
            {
                hostname = hostinput.Text;
                port = int.Parse(portinput.Text);

            }).SetNegativeButton("Cancel", delegate
            {
                hostname = DEFAULT_HOSTNAME;
                port = DEFAULT_PORT;
                alertBuilder.Dispose();
            });

            Android.Support.V7.App.AlertDialog dialog = alertBuilder.Create();
            dialog.Show();


            var editor = prefs.Edit();
            // add the hostname into the preferences
            editor.PutString("hostname", hostname);
            editor.PutInt("port", port);
            // add a boolean tag
            editor.PutBoolean("firststart", false);
            editor.Apply();
            
        }

        private int Connect()
        {
            try
            {

                client = new TcpClient();
                client.ReceiveTimeout = 1000;
                client.SendTimeout = 1000;
                client.Connect(hostname, 54001);

                if (!client.Connected)
                    throw new SocketException(-1);


                return 0;

            }
            catch (System.IO.IOException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), $"IOException: {ex.Message}", Snackbar.LengthLong)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
            }
            catch (SocketException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), $"SocketException: {ex.Message}", Snackbar.LengthLong)
                       .SetAction("Action", (View.IOnClickListener)null).Show();
            }
            catch (TimeoutException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), $"TimeoutException: {ex.Message}", Snackbar.LengthLong)
                       .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
            return -1;
        }

        private void SendDWORD(string dword)
        {
            if (client.Connected == false)
                if (Connect() == -1)
                    return;

            try
            {
                var ns = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(dword);

                ns.Write(data, 0, data.Length);

                ns.Close();

                //Toast.MakeText(this, "Sent", ToastLength.Short).Show();
            }
            catch(System.IO.IOException ex)
            {
                Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), $"Error Sending Req:\n{ex.Message}", Snackbar.LengthLong)
                       .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
        }

        private void Pause_Click(object sender, EventArgs e)
        {
            SendDWORD(Resources.GetString(Resource.String.MEDIA_PLAY_PAUSE));
        }

        private void Forwards_Click(object sender, EventArgs e)
        {
            SendDWORD(Resources.GetString(Resource.String.MEDIA_NEXT_TRACK));
        }

        private void Back_Click(object sender, EventArgs e)
        {
            SendDWORD(Resources.GetString(Resource.String.MEDIA_PREV_TRACK));
        }

        private void IncrVol_Click(object sender, EventArgs e)
        {
            SendDWORD(Resources.GetString(Resource.String.VOLUME_UP));
        }

        private void Mute_Click(object sender, EventArgs e)
        {
            SendDWORD(Resources.GetString(Resource.String.VOLUME_MUTE));
        }

        private void ReduceVol_Click(object sender, EventArgs e)
        {
            SendDWORD(Resources.GetString(Resource.String.VOLUME_DOWN));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.menu_main, menu);

            var item = menu.FindItem(Resource.Id.action_configure);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.action_configure)
            {
                ShowConfigureDialog(PreferenceManager.GetDefaultSharedPreferences(this));
            }

            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            //switch (item.ItemId)
            //{
            //    case Resource.Id.navigation_home:
            //        textMessage.SetText(Resource.String.title_home);
            //        return true;
            //    case Resource.Id.navigation_dashboard:
            //        textMessage.SetText(Resource.String.title_dashboard);
            //        return true;
            //    case Resource.Id.navigation_notifications:
            //        textMessage.SetText(Resource.String.title_notifications);
            //        return true;
            //}
            return false;
        }
    }
}

