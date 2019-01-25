﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Visifire.Charts;

namespace AllianceManager
{
    /// <summary>
    /// SummaryManagement.xaml 的交互逻辑
    /// </summary>
    public partial class SummaryManagement : UserControl
    {
        private List<ActivityAttendInfo> _savedList;
        private readonly List<UserInfo> _userList = new List<UserInfo>();

        public SummaryManagement()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _userList.Clear();
            var users = DBAccess.GetAllUser();
            _userList.AddRange(users.Where(u => !u.IsRemoved));

            ExportBtn.IsEnabled = false;
            SectionArea.IsEnabled = false;
            ChartView.Child = null;
        }

        private void ThisWeekBtn_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Today;
            var startWeek = dt.AddDays(1 - Convert.ToInt32(dt.DayOfWeek.ToString("d")));
            StartDate.SelectedDate = startWeek;
            EndDate.SelectedDate = startWeek.AddDays(6);
        }

        private void LastWeekBtn_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Today;
            var startWeek = dt.AddDays(1 - Convert.ToInt32(dt.DayOfWeek.ToString("d")));
            var lastWeek = startWeek.AddDays(-7);
            StartDate.SelectedDate = lastWeek;
            EndDate.SelectedDate = lastWeek.AddDays(6);
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDate.SelectedDate.HasValue
                || !EndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("请先选择开始时间/截止时间！");
                return;
            }

            if (string.IsNullOrEmpty(ActivityCombo.Text))
            {
                MessageBox.Show("活动内容未选择！");
                return;
            }

            ChartView.Child = null;
            var startDate = StartDate.SelectedDate.Value;
            var endDate = EndDate.SelectedDate.Value;
            var actName = ActivityCombo.Text;
            _savedList = DBAccess.GetAllActivityAttendInfo(startDate, endDate, actName);
            ShowListChart();
        }

        private void SectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowListChart();
        }

        private void ShowListChart()
        {
            try 
            {
                if (!this.IsLoaded) return;

                var index = SectionCombo.SelectedIndex;
                switch (index)
                {
                    case 0:
                        ShowListChartByCareer();
                        break;
                    case 1:
                        ShowListChartByPosition();
                        break;
                    case 2:
                        ShowListChartByMostActive();
                        break;
                    case 3:
                        ShowListChartByMostDeath();
                        break;
                }
                ExportBtn.IsEnabled = true;
                SectionArea.IsEnabled = true;
            }
            catch (Exception ex) 
            {
                MessageBox.Show("ShowListChart:" + ex.Message);
            }

        }

        private void ShowListChartByCareer()
        {
            if (null == _savedList || _savedList.Count == 0) return;
            var selectlist = from s in _savedList
                            group s by s.UserId into g
                            from u in _userList
                            where g.Key == u.Id
                            select new
                            {
                                Name = u.Name,
                                Color = Brushes.Blue,
                                Count = g.Count(s=>s.UserId == u.Id)
                            };

            var orderList = selectlist.OrderByDescending(s => s.Count);

            Chart chart = new Chart();
            chart.Width = 500;
            chart.Height = 300;
            chart.AnimationEnabled = true;
            chart.View3D = true;

            Title title = new Title();
            title.Text = "出勤统计表(职业划分)";
            chart.Titles.Add(title);

            DataSeries dataSeries = new DataSeries();
            dataSeries.RenderAs = RenderAs.Bar;

            foreach (var item in orderList)
            {
                var dataPoint = new DataPoint();
                dataPoint.YValue = item.Count;
                dataPoint.AxisXLabel = item.Name;
                dataPoint.Background = item.Color;
                dataSeries.DataPoints.Add(dataPoint);
            }

            chart.Series.Add(dataSeries);
            ChartView.Child = chart;
        }

        private void ShowListChartByPosition()
        {
            if (null == _savedList || _savedList.Count == 0) return;
            var selectlist = from s in _savedList
                             group s by s.UserId into g
                             from u in _userList
                             where g.Key == u.Id
                             select new
                             {
                                 Name = u.Name,
                                 Position = GetPosition(u.Career),
                                 Color = Brushes.Blue,
                                 Count = g.Count(s => s.UserId == u.Id)
                             };
            var careerList = from c in selectlist
                             group c by c.Position into g
                             select new
                             {
                                 Name = g.Key == 0 ? "坦克" : g.Key == 2 ? "治疗" : "输出",
                                 Count = g.Count()
                             };
            var orderList = careerList.OrderByDescending(s => s.Count);

            Chart chart = new Chart();
            chart.Width = 500;
            chart.Height = 300;
            chart.AnimationEnabled = true;
            chart.View3D = true;

            Title title = new Title();
            title.Text = "出勤统计表(坦克/输出/治疗划分)";
            chart.Titles.Add(title);

            DataSeries dataSeries = new DataSeries();
            dataSeries.RenderAs = RenderAs.Pie;

            foreach (var item in orderList)
            {
                var dataPoint = new DataPoint();
                dataPoint.YValue = item.Count;
                dataPoint.AxisXLabel = item.Name;
                dataSeries.DataPoints.Add(dataPoint);
            }

            chart.Series.Add(dataSeries);
            ChartView.Child = chart;
        }

        private void ShowListChartByMostActive()
        {
            if (null == _savedList || _savedList.Count == 0) return;
            var selectlist = from s in _savedList
                             group s by s.UserId into g
                             from u in _userList
                             where g.Key == u.Id
                             select new
                             {
                                 Name = u.Name,
                                 Color = Brushes.Blue,
                                 Count = g.Count(s => s.UserId == u.Id)
                             };

            var orderList = selectlist.OrderByDescending(s => s.Count);
            var maxCount = orderList.Count() > 0 ? orderList.First().Count : 0;
            var activeList = orderList.Where(o => o.Count == maxCount);

            Chart chart = new Chart();
            chart.Width = 500;
            chart.Height = 300;
            chart.AnimationEnabled = true;
            chart.View3D = true;

            Title title = new Title();
            title.Text = "全勤人员";
            chart.Titles.Add(title);

            DataSeries dataSeries = new DataSeries();
            dataSeries.RenderAs = RenderAs.Bar;

            foreach (var item in activeList)
            {
                var dataPoint = new DataPoint();
                dataPoint.YValue = item.Count;
                dataPoint.AxisXLabel = item.Name;
                dataPoint.Background = item.Color;
                dataSeries.DataPoints.Add(dataPoint);
            }

            chart.Series.Add(dataSeries);
            ChartView.Child = chart;
        }

        private void ShowListChartByMostDeath()
        {
            if (null == _savedList || _savedList.Count == 0) return;
            var activeIds = _savedList.GroupBy(s => s.UserId).Select(i => i.First().UserId);
            var selectlist = _userList.Where(r => !activeIds.Contains(r.Id));

            Chart chart = new Chart();
            chart.Width = 500;
            chart.Height = 300;
            chart.AnimationEnabled = true;
            chart.View3D = true;

            Title title = new Title();
            title.Text = "僵尸人员";
            chart.Titles.Add(title);

            DataSeries dataSeries = new DataSeries();
            dataSeries.RenderAs = RenderAs.Bar;

            foreach (var item in selectlist)
            {
                var dataPoint = new DataPoint();
                dataPoint.YValue = 1;
                dataPoint.AxisXLabel = item.Name;
                dataSeries.DataPoints.Add(dataPoint);
            }

            chart.Series.Add(dataSeries);
            ChartView.Child = chart;
        }

        private int GetPosition(int career)
        {
            switch(career)
            {
                case 0:
                case 1:
                case 12:
                case 21:
                case 23:
                    return 0;
                case 5:
                case 17:
                case 18:
                case 19:
                case 20:
                case 24:
                    return 2;
                default:
                    return 1;
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {

            try 
            {
                var chart = ChartView.Child as Chart;
                if (chart == null) return;
                var ds = chart.Series[0];

                var sb = new StringBuilder();
                sb.AppendLine(chart.Titles[0].Text);
                foreach (var item in ds.DataPoints)
                {
                    sb.AppendLine(string.Format("{0}:{1}", item.AxisXLabel, (int)item.YValue));
                }

                var sfd = new SaveFileDialog();
                sfd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (sfd.ShowDialog() == true)
                {
                    File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("导出成功");
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show("ExportBtn_Click:" + ex.Message);
            }

        }


    }
}
