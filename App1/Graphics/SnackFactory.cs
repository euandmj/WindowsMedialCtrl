using Android.Support.Design.Widget;
using Android.Views;

namespace App1.Graphics
{
    class SnackFactory
    {


        public static void ShowSnackbarMessage(View view, string message, int length)
        {
            Snackbar.Make(view, message, length)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }
    }
}