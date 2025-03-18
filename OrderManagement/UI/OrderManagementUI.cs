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
        private readonly IOrderService _orderService;
        private Window _mainWindow;
        private ListView _ordersList;

        public OrderManagementUI(IOrderService orderService)
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
            new MenuBarItem("New _Order", "", async () => await CreateNewOrder()),
            new MenuBarItem("New _Client", "", () => ClientDialogUtils.ShowAddClientDialog(_orderService)),
            new MenuBarItem("New _Item", "", () => ItemDialogUtils.ShowAddItemDialog(_orderService)),
            new MenuBarItem("_Refresh Orders", "", async () => {
                await ShowOrdersList(true);
                DialogUtils.ShowMessage("Information", "Order list refreshed.");
                }),
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
                $"{o.Id}| CLIENT: {o.ClientName} | TOTAL: {o.Total:C} | STATUS: {o.Status}").ToList());

            if (refresh)
            {
                _mainWindow.SetNeedsDisplay();
            }
        }

        private async Task ShowOrderActions(ListViewItemEventArgs args)
        {
            var orderId = int.Parse(args.Value.ToString().Split('|')[0].Trim());
            var order = await _orderService.GetOrderByIdIncludesAsync(orderId);

            OrderDialogUtils.ShowOrderActions(_orderService, order);
        }

        private async Task CreateNewOrder()
        {
            await OrderDialogUtils.ShowNewOrderDialog(_orderService);
            await ShowOrdersList(true);
        }
    }
}