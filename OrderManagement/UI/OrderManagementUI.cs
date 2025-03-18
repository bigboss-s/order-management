using Terminal.Gui;
using OrderManagement.Services;
using OrderManagement.Entities;
using System.Linq;
using System;
using System.Threading.Tasks;
using OrderManagement.UI.Dialogs;

namespace OrderManagement.UI
{
    public class OrderManagementUI
    {
        private readonly OrderService _orderService;
        private Window _mainWindow;
        private ListView _ordersList;

        public OrderManagementUI(OrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task Run()
        {
            Application.Init();

            _mainWindow = new Window("Order Management System")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 1,
                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.Gray)
                }
            };

            var menu = new MenuBar(new MenuBarItem[]
            {
            new MenuBarItem("_New order", "", async () => await CreateNewOrder()),
            new MenuBarItem("_New client", "", () => DialogUtils.ShowAddClientDialog(_orderService)),
            new MenuBarItem("_New item", "", () => DialogUtils.ShowAddItemDialog(_orderService)),
            new MenuBarItem("_Refresh orders", "", async () => await ShowOrdersList(true)),
            new MenuBarItem("_Quit", "", () => Application.RequestStop())
            });

            await CreateOrdersListView();

            Application.Top.Add(_mainWindow, menu);
            Application.Run();
        }

        private async Task CreateOrdersListView()
        {
            var frame = new FrameView("Orders")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.White, Color.DarkGray)
                }
            };

            _ordersList = new ListView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _ordersList.OpenSelectedItem += async (args) => await ShowOrderActions(args);
            frame.Add(_ordersList);
            _mainWindow.Add(frame);
            await ShowOrdersList();
        }

        private async Task ShowOrdersList(bool refresh = false)
        {
            var orders = await _orderService.GetOrdersAsync();
            _ordersList.SetSource(orders.Select(o =>
                $"{o.Id} | {o.ClientName} | {o.Total:C} | {o.Status}").ToList());

            if (refresh)
            {
                _mainWindow.SetNeedsDisplay();
                DialogUtils.ShowMessage("Information", "Order list refreshed.");
            }
        }

        private async Task ShowOrderActions(ListViewItemEventArgs args)
        {
            var orderId = int.Parse(args.Value.ToString().Split('|')[0].Trim());
            var order = await _orderService.GetOrderByIdIncludesAsync(orderId);

            var dialog = new Dialog($"Order {orderId} Actions");

            var itemList = new ListView()
            {
                Width = Dim.Fill(),
                Height = 10,
                X = 1,
                Y = 5,
                CanFocus = false
            };

            itemList.SetSource(order.OrderItems.Select(o => 
                $"{o.Item.Name} ( {o.Item.Price:C} ) x {o.ItemQuantity}"
            ).ToList());

            var moveButton = new Button("Move to Warehouse")
            {
                X = 1,
                Y = 12
            };
            moveButton.Clicked += async () => await MoveToWarehouse(orderId);

            var shipButton = new Button("Ship Order")
            {
                X = 1,
                Y = 13
            };
            shipButton.Clicked += async () => await ShipOrder(orderId);

            dialog.Add(
                new Label($"Status: {order.Status}") { X = 1, Y = 1 }, 
                new Label($"Client: {order.Client.Name}") { X = 1, Y = 2 }, 
                new Label($"Shipping address: {order.Address}") { X = 1, Y = 3 }, 
                new Label($"Items (item total: {order.Total:C}):") { X = 1, Y = 4},
                itemList,
                moveButton, 
                shipButton);
            Application.Run(dialog);
        }

        private async Task CreateNewOrder()
        {
            await DialogUtils.ShowNewOrderDialog(_orderService);
            await ShowOrdersList(true);
        }

        private async Task MoveToWarehouse(int orderId)
        {
            var result = await _orderService.MoveOrderToWarehouseAsync(orderId);
            DialogUtils.ShowMessage(result.IsSuccess ? "Success!" : "Error", result.Message);
            await ShowOrdersList(true);
        }

        private async Task ShipOrder(int orderId)
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

            await ShowOrdersList(true);
        }
    }
}