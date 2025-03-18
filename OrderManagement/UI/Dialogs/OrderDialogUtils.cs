using OrderManagement.Entities;
using OrderManagement.Services;
using Terminal.Gui;

namespace OrderManagement.UI.Dialogs;

public class OrderDialogUtils : DialogUtils
{
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
        var itemList = new ListView(items.Select(i => $"{i.Name} | {i.Price:c}").ToList())
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
            Width = 10,
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
            else
            {
                ShowMessage("Error", "Invalid quantity");
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

    public static void ShowOrderActions(IOrderService _orderService, Order order)
    {
        var dialog = new Dialog($"Order {order.Id} Actions", 60, 18);

        var itemList = new ListView()
        {
            Width = Dim.Fill(),
            Height = 5,
            X = 1,
            Y = 5,
            CanFocus = false,
            ColorScheme = new ColorScheme
            {
                Focus = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
                Normal = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
                HotFocus = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
                HotNormal = Application.Driver.MakeAttribute(Color.Black, Color.Gray)
            }
        };

        itemList.SetSource(order.OrderItems.Select(o =>
            $"{o.Item.Name} ( {o.Item.Price:C} ) x {o.ItemQuantity}"
        ).ToList());

        var moveButton = new Button("Move to Warehouse")
        {
            X = 1,
            Y = 12
        };
        moveButton.Clicked += async () => {
            await MoveToWarehouse(_orderService, order.Id);
            dialog.SetNeedsDisplay();
        };

        var shipButton = new Button("Ship Order")
        {
            X = 1,
            Y = 13
        };
        shipButton.Clicked += async () => {
            await ShipOrder(_orderService, order.Id);
            dialog.SetNeedsDisplay();
        };

        dialog.Add(
            new Label($"Status: {order.Status}") { X = 1, Y = 1 },
            new Label($"Client: {order.Client.Name}") { X = 1, Y = 2 },
            new Label($"Shipping address: {order.Address}") { X = 1, Y = 3 },
            new Label($"Items (item total: {order.Total:C}):") { X = 1, Y = 4 },
            itemList,
            moveButton,
            shipButton);
        Application.Run(dialog);
    }

    private static async Task MoveToWarehouse(IOrderService _orderService, int orderId)
    {
        var result = await _orderService.MoveOrderToWarehouseAsync(orderId);
        ShowMessage(result.IsSuccess ? "Success!" : "Error", result.Message);
    }

    private static async Task ShipOrder(IOrderService _orderService, int orderId)
    {
        var dialog = new Dialog("Processing Shipping", 50, 10);
        var messageLabel = new Label("Initializing...") { X = 1, Y = 1 };
        var spinnerLabel = new Label("") { X = 1, Y = 3 };
        var okButton = new Button("OK") { X = Pos.Center(), Y = 5 };

        var cts = new CancellationTokenSource();
        var spinnerFrames = new[] { "-", "\\", "|", "/" };
        var spinnerIndex = 0;
        var spinnerTimer = new System.Timers.Timer(100);

        spinnerTimer.Elapsed += (s, e) =>
        {
            Application.MainLoop.Invoke(() =>
            {
                spinnerIndex = (spinnerIndex + 1) % spinnerFrames.Length;
                spinnerLabel.Text = spinnerFrames[spinnerIndex];
                dialog.SetNeedsDisplay();
            });
        };

        dialog.Add(messageLabel, spinnerLabel, okButton);
        okButton.Clicked += () => dialog.Running = false;

        var shippingTask = Task.Run(async () =>
        {
            try
            {
                Application.MainLoop.Invoke(() =>
                {
                    messageLabel.Text = "Starting shipping process...";
                    spinnerTimer.Start();
                    okButton.Visible = false;
                });

                var result = await _orderService.ShipOrderAsync(orderId);

                Application.MainLoop.Invoke(() =>
                {
                    spinnerTimer.Stop();
                    okButton.Visible = true;
                    messageLabel.Text = result.Message;
                    okButton.Visible = true;
                    dialog.LayoutSubviews();
                });
            }
            catch (Exception ex)
            {
                Application.MainLoop.Invoke(() =>
                {
                    spinnerTimer.Stop();
                    messageLabel.Text = $"Error: {ex.Message}";
                    okButton.Visible = true;
                    dialog.LayoutSubviews();
                });
            }
        }, cts.Token);

        Application.Run(dialog);

        if (!shippingTask.IsCompleted)
        {
            cts.Cancel();
            await shippingTask;
        }
    }
}