using System;
using System.Linq;
using Xamarin.Forms;

/**
 *  Código de clase sacado de esta página web:
 *  https://greenfinch.ie/restrict-certain-characters-in-entry-control/
 */

namespace AkuaApp_v3.CustomElements
{
    public class LimitedEntry : Behavior<Entry>
    {
        private const string limitedChars = ",";

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        private static void OnEntryTextChanged (object sender, TextChangedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                bool ValidEntry = e.NewTextValue.ToCharArray().All(x => !limitedChars.Contains(x));

                ((Entry)sender).Text = ValidEntry ? e.NewTextValue : e.NewTextValue.Remove(e.NewTextValue.Length - 1);
            }
        }

    }
}
