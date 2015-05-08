using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PusherForWindows.Model
{
    public class PushTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PushNoteTemplate { get; set; }

        public DataTemplate PushLinkTemplate { get; set; }
        
        public DataTemplate PushImageTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is PushNote)
                return this.PushNoteTemplate;

            if (item is PushLink)
                return this.PushLinkTemplate;

            if (item is PushFile)
                if ((new Regex(@"(^image\/)(.*)")).Match((string)((PushFile)item).MimeType).Success)
                    return this.PushImageTemplate;
                else
                    return this.PushLinkTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
