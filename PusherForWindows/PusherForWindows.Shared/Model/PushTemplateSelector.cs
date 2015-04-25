using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PusherForWindows.Model
{
    class PushTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PushNoteTemplate { get; set; }
        public DataTemplate PushLinkTemplate { get; set; }
        public DataTemplate PushFileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is PushNote)
                return PushNoteTemplate;

            if (item is PushLink)
                return PushLinkTemplate;

            if (item is PushFile)
                return PushFileTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
