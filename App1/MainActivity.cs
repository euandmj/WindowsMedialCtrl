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
        private const int DEFAULT_PORT = 54001;
        private const string DEFAULT_HOSTNAME = "192.168.1.219";
        private const string FIRST_START = "firststart";
        private readonly RemoteClient _Client;

        public MainActivity()
        {
            _Client = new RemoteClient();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // First time initialisation
            InitFirstStart();

            // events             
            _Client.SnackEvent += this._Client_SnackEvent;
            ((Button)FindViewById(Resource.Id.buttonReduceVol)).Click += this.ReduceVol_Click;
            ((Button)FindViewById(Resource.Id.buttonMute)).Click += this.Mute_Click;
            ((Button)FindViewById(Resource.Id.buttonIncrVol)).Click += this.IncrVol_Click;
            ((Button)FindViewById(Resource.Id.buttonBack)).Click += this.Back_Click;
            ((Button)FindViewById(Resource.Id.buttonForwards)).Click += this.Forwards_Click;
            ((Button)FindViewById(Resource.Id.buttonPause)).Click += this.Pause_Click;
        }

        private void InitFirstStart()
        {
            using var prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            bool isFirstStart = prefs.GetBoolean(key: FIRST_START, defValue: true);

            if (isFirstStart)
            {
                ShowConfigureDialog();
            }
            _Client.Hostname = prefs.GetString(key: "hostname", defValue: DEFAULT_HOSTNAME);
            _Client.Port = prefs.GetInt(key: "port", defValue: DEFAULT_PORT);
        }

        private void ShowConfigureDialog()
        {
            var layoutInflater = LayoutInflater.From(this);
            var view = layoutInflater.Inflate(Resource.Layout.hostname_input_box, null);
            var alertBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertBuilder.SetView(view);

            var hostinput = view.FindViewById<EditText>(Resource.Id.hostnameText);
            var portinput = view.FindViewById<EditText>(Resource.Id.portText);

            hostinput.Text = _Client.Hostname;
            portinput.Text = _Client.Port.ToString();

            alertBuilder.SetCancelable(false)
                .SetPositiveButton("Submit", delegate
            {
                _Client.Hostname = hostinput.Text;
                _Client.Port = int.Parse(portinput.Text);
                PutPreferences(new (string key, object arg)[] { ("hostname", _Client.Hostname), ("port", _Client.Port) });

            }).SetNegativeButton("Cancel", delegate
            {
                _Client.Hostname = DEFAULT_HOSTNAME;
                _Client.Port = DEFAULT_PORT;
                PutPreferences(new (string key, object arg)[] { ("hostname", _Client.Hostname), ("port", _Client.Port) });
                alertBuilder.Dispose();
            });

            var dialog = alertBuilder.Create();
            dialog.Show();
            PutPreferences(new (string key, object arg)[] { (FIRST_START, false) });
        }

        private void ShowSnackbarMessage(string message, int length = Snackbar.LengthLong) =>
            Snackbar.Make(FindViewById<View>(Resource.Id.rootLayout), message, length)
                .SetAction("Action", (View.IOnClickListener)null).Show();

        private void Pause_Click(object sender, EventArgs e) =>
            _Client.SendDWORD(Resources.GetString(Resource.String.MEDIA_PLAY_PAUSE));

        private void Forwards_Click(object sender, EventArgs e) =>
            _Client.SendDWORD(Resources.GetString(Resource.String.MEDIA_NEXT_TRACK));

        private void Back_Click(object sender, EventArgs e) =>
            _Client.SendDWORD(Resources.GetString(Resource.String.MEDIA_PREV_TRACK));

        private void IncrVol_Click(object sender, EventArgs e) =>
            _Client.SendDWORD(Resources.GetString(Resource.String.VOLUME_UP));

        private void Mute_Click(object sender, EventArgs e) =>
            _Client.SendDWORD(Resources.GetString(Resource.String.VOLUME_MUTE));

        private void ReduceVol_Click(object sender, EventArgs e) =>
            _Client.SendDWORD(Resources.GetString(Resource.String.VOLUME_DOWN));


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
                _Client.SendDWORD(Resources.GetString(Resource.String.SHUTDOWN));
            else if (id == Resource.Id.action_restart)
                _Client.SendDWORD(Resources.GetString(Resource.String.RESTART));
            else if (id == Resource.Id.action_lock)
                _Client.SendDWORD(Resources.GetString(Resource.String.LOCK));

            return base.OnOptionsItemSelected(item);
        }

        private void _Client_SnackEvent(object sender, Events.SnackbarEventArgs e)
        {
            ShowSnackbarMessage(e.Message);
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

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // events
            _Client.SnackEvent -= _Client_SnackEvent;
            ((Button)FindViewById(Resource.Id.buttonReduceVol)).Click -= this.ReduceVol_Click;
            ((Button)FindViewById(Resource.Id.buttonMute)).Click -= this.Mute_Click;
            ((Button)FindViewById(Resource.Id.buttonIncrVol)).Click -= this.IncrVol_Click;
            ((Button)FindViewById(Resource.Id.buttonBack)).Click -= this.Back_Click;
            ((Button)FindViewById(Resource.Id.buttonForwards)).Click -= this.Forwards_Click;
            ((Button)FindViewById(Resource.Id.buttonPause)).Click -= this.Pause_Click;
        }
    }
}

