using Android.Content;
using Android.Graphics;
using Android.Widget;

namespace App1.Graphics
{
    class ToastFactory
    {
        private static ColorMatrixColorFilter MakeMatrix(Color col) =>
            new ColorMatrixColorFilter(new float[]
            {
                0,0,0,0,col.R,
                0,0,0,0,col.G,
                0,0,0,0,col.B,
                0,0,0,1,0
            });


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx">Caller context</param>
        /// <param name="colour">Text colour</param>
        /// <returns></returns>
        public static Toast MakeColoredToast(Context ctx, string message, 
            Color colour, ToastLength length = ToastLength.Short)
        {
            var cm = MakeMatrix(colour);

            var toast = Toast.MakeText(ctx, message, length);

            toast.View.Background.SetColorFilter(cm);

            return toast;
        }
    }
}