using System;
using Android.Support.V7.Widget;

namespace CardsAndroid.NativeClasses
{
    public class DragDetector : RecyclerView.OnScrollListener
    {
        public Action DragEnd;
        public Action DragStart;
        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            base.OnScrollStateChanged(recyclerView, newState);

            if (newState == RecyclerView.ScrollStateDragging)
            { //The user starts scrolling
              //readyForAction = true;
                try { DragStart(); } catch { }
            }
            // DragEnded event.
            else
            {
                try { DragEnd(); } catch { }
            }
        }
    }
}
