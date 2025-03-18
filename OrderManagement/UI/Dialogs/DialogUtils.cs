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

    public static async Task ShowNewOrderDialog(IOrderService _orderService)
    {
        var createDialog = new Dialog("Create new order", 73, 26); 
        var selectedItems = new Dictionary<int, int>();
        var address = string.Empty;

        var leftPanel = new FrameView("Order details")
        {
            X = 0,
            Y = 0,
            Width = 45,
            Height = 25
        };

        var rightPanel = new FrameView("Payment/Submit")
        {
            X = 46,
            Y = 0,
            Width = 25,
            Height = 25
        };

        var clients = await _orderService.GetClientsAsync();
        var clientList = new ListView(clients.Select(c => c.Name).ToList())
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(),
            Height = 5
        };

        var addressField = new TextField("")
        {
            X = 1,
            Y = 7,
            Width = Dim.Fill() - 2,
            Height = 1,
        };

        var items = await _orderService.GetItemsAsync();
        var itemList = new ListView(items.Select(i => i.Name).ToList())
        {
            X = 1,
            Y = 9,
            Width = Dim.Fill(),
            Height = 5
        };

        var quantityField = new TextField("1")
        {
            X = 1,
            Y = 15,
            Width = 10
        };

        var selectedItemsList = new ListView()
        {
            X = 1,
            Y = 17,
            Width = Dim.Fill(),
            Height = 5,

        };

        var addItemButton = new Button("Add item")
        {
            X = Pos.Right(quantityField) + 2,
            Y = 15
        };

        var paymentOptions = Enum.GetValues<PaymentMethod>().Select(p => $"  {p}").ToList();
        var paymentList = new ListView(paymentOptions)
        {
            X = 1,
            Y = 1,
            Width = 25,
            Height = paymentOptions.Count,
            AllowsMarking = false
        };

        PaymentMethod selectedPayment = PaymentMethod.Card;

        paymentList.OpenSelectedItem += (args) =>
        {
            for (int i = 0; i < paymentOptions.Count; i++)
            {
                paymentOptions[i] = $"  {Enum.GetValues<PaymentMethod>().GetValue(i)}";
            }

            paymentOptions[args.Item] = $"X {Enum.GetValues<PaymentMethod>().GetValue(args.Item)}";
            paymentList.SetSource(paymentOptions);
            selectedPayment = (PaymentMethod)args.Item;
        };

        paymentOptions[0] = $"X {paymentOptions[0].Substring(2)}";
        paymentList.SetSource(paymentOptions);

        var createButton = new Button("Create order")
        {
            X = Pos.Center(),
            Y = Pos.AnchorEnd(1)
        };

        addItemButton.Clicked += () =>
        {
            if (itemList.SelectedItem >= 0 && int.TryParse(quantityField.Text.ToString(), out int quantity) && quantity > 0)
            {
                var selectedItem = items[itemList.SelectedItem];
                selectedItems[selectedItem.Id] = quantity;
                selectedItemsList.SetSource(selectedItems.Select(kvp =>
                    $"{items.First(i => i.Id == kvp.Key).Name} x {kvp.Value}").ToList());
            }
        };

        createButton.Clicked += async () =>
        {
            if (clientList.SelectedItem == -1)
            {
                ShowMessage("Error", "Select a client");
                return;
            }

            if (!selectedItems.Any())
            {
                ShowMessage("Error", "Add at least one item");
                return;
            }

            try
            {
                var result = await _orderService.InsertOrderAsync(
                    clients[clientList.SelectedItem].Id,
                    selectedItems,
                    selectedPayment,
                    addressField.Text.ToString()
                );

                if (result.IsSuccess)
                {
                    createDialog.Running = false;
                }
                else
                {
                    ShowMessage("Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error", ex.Message);
            }
        };

        leftPanel.Add(
            new Label("Select client:") { X = 1, Y = 0 },
            clientList,
            new Label("Shipping address:") { X = 1, Y = 6 },
            addressField,
            new Label("Available items:") { X = 1, Y = 8 },
            itemList,
            new Label("Quantity:") { X = 1, Y = 14 },
            quantityField,
            addItemButton,
            new Label("Selected items:") { X = 1, Y = 16 },
            selectedItemsList
        );

        rightPanel.Add(
            new Label("Payment Method:") { X = 1, Y = 0 },
            paymentList,
            createButton
        );

        createDialog.Add(leftPanel, rightPanel);
        Application.Run(createDialog);
    }

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