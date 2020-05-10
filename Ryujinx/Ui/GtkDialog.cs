using Gtk;
using System.Reflection;

namespace Ryujinx.Ui
{
    internal class GtkDialog : MessageDialog
    {
        internal static bool _isChoiceDialogOpen = false;

        private GtkDialog(string title, string mainText, string secondaryText,
            MessageType messageType = MessageType.Other, ButtonsType buttonsType = ButtonsType.Ok) : base(null, DialogFlags.Modal, messageType, buttonsType, null)
        {
            Title          = title;
            Icon           = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");
            Text           = mainText;
            SecondaryText  = secondaryText;
            WindowPosition = WindowPosition.Center;
            Response      += GtkDialog_Response;

            SetSizeRequest(100, 20);
        }

        private void GtkDialog_Response(object sender, ResponseArgs args)
        {
            Dispose();
        }

        internal static void CreateInfoDialog(string title, string mainText, string secondaryText)
        {
            new GtkDialog(title, mainText, secondaryText, MessageType.Info).Run();
        }

        internal static void CreateWarningDialog(string mainText, string secondaryText)
        {
            new GtkDialog("Ryujinx - Warning", mainText, secondaryText, MessageType.Warning).Run();
        }

        internal static void CreateErrorDialog(string errorMessage)
        {
            new GtkDialog("Ryujinx - Error", "Ryujinx has encountered an error", errorMessage, MessageType.Error).Run();
        }

        internal static MessageDialog CreateConfirmationDialog(string mainText, string secondaryText = "")
        {
            return new GtkDialog("Ryujinx - Confirmation", mainText, secondaryText, MessageType.Question, ButtonsType.YesNo);
        }

        internal static bool CreateChoiceDialog(string title, string text, string secondaryText)
        {
            if (_isChoiceDialogOpen)
            {
                return false;
            }

            _isChoiceDialogOpen = true;

            MessageDialog messageDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, null)
            {
                Title = title,
                Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png"),
                Text = text,
                SecondaryText = secondaryText,
                WindowPosition = WindowPosition.Center
            };

            messageDialog.SetSizeRequest(100, 20);
            ResponseType res = (ResponseType)messageDialog.Run();
            messageDialog.Dispose();
            _isChoiceDialogOpen = false;

            if (res == ResponseType.Yes)
            {
                return true;
            }

            return false;
        }
    }
}