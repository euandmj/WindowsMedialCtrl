using Android.App;
using Android.OS;
using Android.Preferences;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Net.Sockets;
using System.Text;

namespace App1
{
    [Activity(Label = "Shutup Netflix", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        const string DEFAULT_HOSTNAME = "192.168.1.219";
        const int DEFAULT_PORT = 54001;
        string hostname;
        int port;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // First time initialisation
            var prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            bool b = prefs.GetBoolean(key: "firststart", defValue: true);

            if (b)
            {
                ShowConfigureDialog();
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
        }

        private void ShowConfigureDialog()
        {
            var layoutInflater = LayoutInflater.From(this);
            var view = layoutInflater.Inflate(Resource.Layout.hostname_input_box, null);
            var alertBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertBuilder.SetView(view);

            var hostinput = view.FindViewById<EditText>(Resource.Id.hostnameText);
            var portinput = view.FindViewById<EditText>(Resource.Id.portText);

            hostinput.Text = hostname;
            portinput.Text = port.ToString();

            alertBuilder.SetCancelable(false)
                .SetPositiveButton("Submit", delegate
            {
                hostname = hostinput.Text;
                port = int.Parse(portinput.Text);
                PutPreferences(new (string key, object arg)[] { ("hostname", hostname), ("port", port) });

            }).SetNegativeButton("Cancel", delegate
            {
                hostname = DEFAULT_HOSTNAME;
                port = DEFAULT_PORT;
                PutPreferences(new (string key, object arg)[] { ("hostname", hostname), ("port", port) });
                alertBuilder.Dispose();
            });

            var dialog = alertBuilder.Create();
            dialog.Show();
            PutPreferences(new (string key, object arg)[] { ("firsstart", false) });
        }

        private async void SendDWORD(string dword)
        {
            try
            {
                using var client = new TcpClient()
                {
                    ReceiveTimeout = 1000,
                    SendTimeout = 1000
                };

                if (!client.Connected)
                    await client.ConnectAsync(hostname, port);

                using var ns = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(dword);
                await ns.WriteAsync(data, 0, data.Length);
            }
            catch (InvalidOperationException)
            {
                ShowSnackbarMessage("Unable to send request.");
            }
            catch (TimeoutException)
            {
                ShowSnackbarMessage("Request Timed Out");
            }
            catch (Exception ex)
            {
                ShowSnackbarMessage(ex);
            }
        }

        private void ShowSnackbarMessage(Exception ex, int length = Snackbar.LengthLong)
        {
            ShowSnackbarMessage($"{ex.GetType()}\n{ex.Message}", length);
        }

        private void ShowSnackbarMessage(string message, int length = Snackbar.LengthLong) =>
            Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), message, length)
                .SetAction("Action", (View.IOnClickListener)null).Show();

        private void Pause_Click(object sender, EventArgs e) =>
            SendDWORD(Resources.GetString(Resource.String.MEDIA_PLAY_PAUSE));

        private void Forwards_Click(object sender, EventArgs e) =>
            SendDWORD(Resources.GetString(Resource.String.MEDIA_NEXT_TRACK));

        private void Back_Click(object sender, EventArgs e) =>
            SendDWORD(Resources.GetString(Resource.String.MEDIA_PREV_TRACK));

        private void IncrVol_Click(object sender, EventArgs e) =>
            SendDWORD(Resources.GetString(Resource.String.VOLUME_UP));

        private void Mute_Click(object sender, EventArgs e) =>
            SendDWORD(Resources.GetString(Resource.String.VOLUME_MUTE));

        private void ReduceVol_Click(object sender, EventArgs e) =>
            SendDWORD(Resources.GetString(Resource.String.VOLUME_DOWN));


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.action_configure)
                ShowConfigureDialog();
            else if (id == Resource.Id.action_shutdown)
                SendDWORD(Resources.GetString(Resource.String.SHUTDOWN));
            else if (id == Resource.Id.action_restart)
                SendDWORD(Resources.GetString(Resource.String.RESTART));
            else if (id == Resource.Id.action_lock)
                SendDWORD(Resources.GetString(Resource.String.LOCK));

            return base.OnOptionsItemSelected(item);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.VolumeUp:
                    IncrVol_Click(null, null);
                    break;
                case Keycode.VolumeDown:
                    ReduceVol_Click(null, null);
                    break;
                case Keycode.VolumeMute:
                    Mute_Click(null, null);
                    break;
            }
            return true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();


            // events             
            ((Button)FindViewById(Resource.Id.buttonReduceVol)).Click -= this.ReduceVol_Click;
            ((Button)FindViewById(Resource.Id.buttonMute)).Click -= this.Mute_Click;
            ((Button)FindViewById(Resource.Id.buttonIncrVol)).Click -= this.IncrVol_Click;
            ((Button)FindViewById(Resource.Id.buttonBack)).Click -= this.Back_Click;
            ((Button)FindViewById(Resource.Id.buttonForwards)).Click -= this.Forwards_Click;
            ((Button)FindViewById(Resource.Id.buttonPause)).Click -= this.Pause_Click;
        }

        private void PutPreferences((string key, object arg)[] args)
        {
            using var prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            var editor = prefs.Edit();

            foreach (var (key, arg) in args)
            {
                var t = arg.GetType();

                if (arg is string @string)
                    editor.PutString(key, @string);
                else if (arg is int @int)
                    editor.PutInt(key, @int);
                else if (arg is bool boolean)
                    editor.PutBoolean(key, boolean);
                else if (arg is float single)
                    editor.PutFloat(key, single);
                else if (arg is long int1)
                    editor.PutLong(key, int1);
                else
                    throw new ArgumentException($"{nameof(arg)} is not a supported type.");
            }
            editor.Apply();
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

