using OrderManagement.Entities;
using OrderManagement.Services;
using Terminal.Gui;

namespace OrderManagement.UI.Dialogs;

public class DialogUtils
{
    public static void ShowMessage(string title, string message)
    {
        var dialog = new Dialog(title, 50, 10);
        dialog.Add(new Label(message) { X = 1, Y = 1 });
        var okButton = new Button("OK") { X = Pos.Center(), Y = 5 };
        okButton.Clicked += () => Application.RequestStop();
        dialog.Add(okButton);
        Application.Run(dialog);
    }
}