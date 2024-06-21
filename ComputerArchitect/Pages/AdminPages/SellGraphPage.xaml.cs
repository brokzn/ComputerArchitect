using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using ComputerArchitect.Database;
using System.Windows;

namespace ComputerArchitect.Pages.AdminPages
{
    public partial class SellGraphPage : Page
    {
        private List<Orders> _orders;
        private List<OrderCartItems> _orderCartItems;

        public SellGraphPage()
        {
            InitializeComponent();
            LoadData();
            LoadDataAndPlot("day");
        }

        private void LoadData()
        {
            var context = App.Database;
            _orders = context.Orders.ToList();
            _orderCartItems = context.OrderCartItems.ToList();
        }

        private void LoadDataAndPlot(string period)
        {
            var filteredOrders = _orders.Where(o => o.OrderStatusId == 3).ToList();
            var filteredOrderCartItems = _orderCartItems
                .Where(ci => filteredOrders.Any(o => o.OrderId == ci.OrderId))
                .ToList();

            var salesData = TransformOrdersToSalesData(filteredOrders, filteredOrderCartItems, period);

            var plotModel = new PlotModel
            {
                Title = "Количество товаров по датам",
                TextColor = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                PlotAreaBorderColor = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                PlotAreaBorderThickness = new OxyThickness(1)
            };

            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM/yyyy",
                AxislineColor = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                TicklineColor = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                TextColor = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                MinorGridlineStyle = LineStyle.Solid,
                MinorGridlineColor = OxyColor.FromRgb(0x42, 0x42, 0x4B),
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(0x42, 0x42, 0x4B),
            };
            plotModel.Axes.Add(dateAxis);

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Количество товаров",
                AxislineColor = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                TicklineColor = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                TextColor = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                MinorGridlineStyle = LineStyle.Solid,
                MinorGridlineColor = OxyColor.FromRgb(0x42, 0x42, 0x4B),
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(0x42, 0x42, 0x4B),
            };
            plotModel.Axes.Add(valueAxis);

            var series = new LineSeries
            {
                Title = "Количество товаров",
                MarkerType = MarkerType.Circle,
                Color = OxyColor.FromRgb(0xD6, 0xD6, 0xD6),
                MarkerFill = OxyColor.FromRgb(0x6D, 0xB2, 0xE3),
                StrokeThickness = 2  // Set the thickness of the line
            };

            foreach (var data in salesData)
            {
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(data.Date), data.Quantity));
            }

            plotModel.Series.Add(series);
            PlotView.Model = plotModel;

            var (popularItem, unpopularItem) = GetMostAndLeastPopularItems(filteredOrderCartItems);
            PopularItemTextBlock.Text = $"Самый популярный товар: {popularItem.ItemName} (Количество продаж: {popularItem.Quantity})";
            UnpopularItemTextBlock.Text = $"Самый непопулярный товар: {unpopularItem.ItemName} (Количество продаж: {unpopularItem.Quantity})";

            var totalEarnings = filteredOrders.Sum(o => o.TotalCost ?? 0);
            TotalEarningsTextBlock.Text = $"Общая заработанная сумма: {totalEarnings:N0} ₽";
        }



        private List<SalesData> TransformOrdersToSalesData(List<Orders> orders, List<OrderCartItems> orderCartItems, string period)
        {
            IEnumerable<IGrouping<object, Orders>> groupedOrders;

            if (period == "week")
            {
                groupedOrders = orders.Where(o => o.OrderCreateDate.HasValue)
                                      .GroupBy(o => new { Year = o.OrderCreateDate.Value.Year, Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(o.OrderCreateDate.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
            }
            else if (period == "month")
            {
                groupedOrders = orders.Where(o => o.OrderCreateDate.HasValue)
                                      .GroupBy(o => new { Year = o.OrderCreateDate.Value.Year, Month = o.OrderCreateDate.Value.Month });
            }
            else if (period == "year")
            {
                groupedOrders = orders.Where(o => o.OrderCreateDate.HasValue)
                                      .GroupBy(o => new { Year = o.OrderCreateDate.Value.Year });
            }
            else
            {
                groupedOrders = orders.Where(o => o.OrderCreateDate.HasValue)
                                      .GroupBy(o => new { Date = o.OrderCreateDate.Value.Date });
            }

            var salesData = groupedOrders
        .Select(g =>
        {
            DateTime date;
            if (period == "week")
            {
                date = FirstDateOfWeekISO8601(g.First().OrderCreateDate.Value.Year, g.First().OrderCreateDate.Value.Day);
            }
            else if (period == "month")
            {
                date = new DateTime(g.First().OrderCreateDate.Value.Year, g.First().OrderCreateDate.Value.Month, 1);
            }
            else if (period == "year")
            {
                date = new DateTime((int)g.First().OrderCreateDate.Value.Year, 1, 1);
            }
            else
            {
                date = g.First().OrderCreateDate.Value.Date;
            }

                    var quantity = g.Sum(order =>
                        orderCartItems.Where(i => i.OrderId == order.OrderId)
                                      .Sum(i => (i.CpuCount ?? 0) + (i.MotherboardCount ?? 0) + (i.CaseCount ?? 0) +
                                                (i.GPUCount ?? 0) + (i.FanCount ?? 0) + (i.RAMCount ?? 0) +
                                                (i.MemoryCount ?? 0) + (i.PowerSuppliesCount ?? 0)));

                    return new SalesData { Date = date, Quantity = quantity };
                })
                .ToList();

            return salesData;
        }




        private (PopularItem popularItem, PopularItem unpopularItem) GetMostAndLeastPopularItems(List<OrderCartItems> orderCartItems)
        {
            var itemCounts = new Dictionary<string, int>();

            foreach (var item in orderCartItems)
            {
                // Примеры использования доступных атрибутов вместо Name
                IncrementItemCount(itemCounts, item.Cases?.Model, item.CaseCount);
                IncrementItemCount(itemCounts, item.Coolers?.Model, item.FanCount);
                IncrementItemCount(itemCounts, item.CPUS?.Model, item.CpuCount);
                IncrementItemCount(itemCounts, item.GPUS?.GPU_Model, item.GPUCount);
                IncrementItemCount(itemCounts, item.HDDs?.Model, item.MemoryCount);
                IncrementItemCount(itemCounts, item.Motherboards?.Motherboard_Model, item.MotherboardCount);
                IncrementItemCount(itemCounts, item.PowerSupplies?.Model, item.PowerSuppliesCount);
                IncrementItemCount(itemCounts, item.RAMS?.RAM_Model, item.RAMCount);
            }

            var mostPopularItem = itemCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
            var leastPopularItem = itemCounts.OrderBy(kvp => kvp.Value).FirstOrDefault();

            return (new PopularItem { ItemName = mostPopularItem.Key, Quantity = mostPopularItem.Value },
                    new PopularItem { ItemName = leastPopularItem.Key, Quantity = leastPopularItem.Value });
        }

        private void IncrementItemCount(Dictionary<string, int> itemCounts, string itemName, int? count)
        {
            if (!string.IsNullOrEmpty(itemName) && count.HasValue)
            {
                if (itemCounts.ContainsKey(itemName))
                {
                    itemCounts[itemName] += count.Value;
                }
                else
                {
                    itemCounts[itemName] = count.Value;
                }
            }
        }


        private void SortByDay_Click(object sender, RoutedEventArgs e)
        {
            LoadDataAndPlot("day");
        }

        private void SortByWeek_Click(object sender, RoutedEventArgs e)
        {
            LoadDataAndPlot("week");
        }

        private void SortByMonth_Click(object sender, RoutedEventArgs e)
        {
            LoadDataAndPlot("month");
        }

        private void SortByYear_Click(object sender, RoutedEventArgs e)
        {
            LoadDataAndPlot("year");
        }

        private static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
    }

    public class SalesData
    {
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }

    public class PopularItem
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
    }
}
