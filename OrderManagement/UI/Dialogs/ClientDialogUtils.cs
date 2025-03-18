using OrderManagement.Entities;
using OrderManagement.Services;
using Terminal.Gui;

namespace OrderManagement.UI.Dialogs;

public class ClientDialogUtils : DialogUtils
{
    public static void ShowAddClientDialog(IOrderService _orderService)
    {
        var dialog = new Dialog("Add new client", 60, 20);
        var nameField = new TextField("")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2
        };

        var addressField = new TextField("")
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill() - 2
        };

        var clientTypeOptions = Enum.GetValues<ClientType>().Select(ct => $"  {ct}").ToList();
        var selectedClientType = ClientType.Individual;
        var clientTypeList = new ListView(clientTypeOptions)
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill() - 2,
            Height = 4
        };

        clientTypeOptions[0] = $"X {Enum.GetValues<ClientType>().GetValue(0)}";
        clientTypeList.SetSource(clientTypeOptions);

        clientTypeList.OpenSelectedItem += (args) =>
        {
            for (int i = 0; i < clientTypeOptions.Count; i++)
            {
                clientTypeOptions[i] = $"  {Enum.GetValues<ClientType>().GetValue(i)}";
            }
            clientTypeOptions[args.Item] = $"X {Enum.GetValues<ClientType>().GetValue(args.Item)}";
            clientTypeList.SetSource(clientTypeOptions);
            selectedClientType = (ClientType)args.Item;
        };

        var createButton = new Button("Create client")
        {
            X = Pos.Center(),
            Y = 11
        };

        createButton.Clicked += async () =>
        {
            if (string.IsNullOrWhiteSpace(nameField.Text.ToString()))
            {
                ShowMessage("Error", "Client name cannot be empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(addressField.Text.ToString())){
                ShowMessage("Error", "Address cannot be empty");
                return;
            }

            var result = await _orderService.InsertClientAsync(
                nameField.Text.ToString(),
                selectedClientType,
                addressField.Text.ToString()
            );

            if (result.IsSuccess)
            {
                dialog.Running = false;
                ShowMessage("Success", "Client created successfully");
            }
            else
            {
                ShowMessage("Error", result.Message);
            }
        };

        dialog.Add(
            new Label("Client name:") { X = 1, Y = 0 },
            nameField,
            new Label("Address:") { X = 1, Y = 2 },
            addressField,
            new Label("Client type:") { X = 1, Y = 4 },
            clientTypeList,
            createButton
        );

        Application.Run(dialog);
    }
}