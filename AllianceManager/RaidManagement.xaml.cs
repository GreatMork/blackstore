using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AllianceManager
{
    /// <summary>
    /// RaidManagement.xaml 的交互逻辑
    /// </summary>
    public partial class RaidManagement : UserControl
    {
        private readonly ObservableCollection<RaidInfo> raidList = new ObservableCollection<RaidInfo>();
        public ObservableCollection<RaidInfo> RaidList
        {
            get { return raidList; }
        }

        public RaidManagement()
        {
            InitializeComponent();
            InitializeComponentData();
        }

        private void InitializeComponentData()
        {
            this.DataContext = this;
            CurrentDate.SelectedDate = CurrentDate.DisplayDate;
        }

        private void LoadCurrentRaidInfo()
        {
            var date = CurrentDate.SelectedDate;
            if (date == null) return;

            var list = DBAccess.GetAllRaidInfo(date.Value);
            RaidList.Clear();
            list.ForEach(l => RaidList.Add(l));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentRaidInfo();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var raidInfo = new RaidInfo
            {
                Date = CurrentDate.SelectedDate.Value,
                Name = string.Format("{0}_{1}_{2}", RaidName.Text, RaidTime.Text, RaidNo.Text)
            };

            if (RaidList.Any(r => r.Name.Equals(raidInfo.Name)))
            {
                MessageBox.Show("已经有一个相同的副本添加了！");
                return;
            }

            DBAccess.AddRaidInfo(raidInfo);
            LoadCurrentRaidInfo();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var item = RaidListBox.SelectedItem as RaidInfo;
            if (item == null) return;

            if (DBAccess.HasActivityByRaidInfo(item)) 
            {
                var result = MessageBox.Show("该副本已经有签到记录，删除将清除所有签到记录，是否继续？", "询问", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;
            }

            DBAccess.ClearAllActivitysByRaidInfo(item);
            DBAccess.RemoveRaidInfo(item);
            LoadCurrentRaidInfo();
        }

        private void CurrentDate_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCurrentRaidInfo();
        }
    }
}
