using OrderManagement.Entities;
using OrderManagement.Services;
using Terminal.Gui;

namespace OrderManagement.UI.Dialogs;

public class ItemDialogUtils : DialogUtils
{
    public static void ShowAddItemDialog(IOrderService _orderService)
    {
        var dialog = new Dialog("Add new item", 60, 15);
        var nameField = new TextField("")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2
        };

        var priceField = new TextField("0.00")
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill() - 2
        };

        var createButton = new Button("Create item")
        {
            X = Pos.Center(),
            Y = 6
        };

        createButton.Clicked += async () =>
        {
            if (string.IsNullOrWhiteSpace(nameField.Text.ToString()))
            {
                ShowMessage("Error", "Item name cannot be empty");
                return;
            }

            if (!double.TryParse(priceField.Text.ToString(), out double price) || price <= 0)
            {
                ShowMessage("Error", "Invalid price value");
                return;
            }

            var result = await _orderService.InsertItemAsync(
                nameField.Text.ToString(),
                price
            );

            if (result.IsSuccess)
            {
                dialog.Running = false;
                ShowMessage("Success", "Item created successfully");
            }
            else
            {
                ShowMessage("Error", result.Message);
            }
        };

        dialog.Add(
            new Label("Item name:") { X = 1, Y = 0 },
            nameField,
            new Label("Price:") { X = 1, Y = 2 },
            priceField,
            createButton
        );

        Application.Run(dialog);
    }

}