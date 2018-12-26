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
    /// ActivityManagement.xaml 的交互逻辑
    /// </summary>
    public partial class ActivityManagement : UserControl
    {
        private readonly List<ActivityAttendInfo> attendInfos = new List<ActivityAttendInfo>();

        private readonly ObservableCollection<string> activityList = new ObservableCollection<string>();
        public ObservableCollection<string> ActivityList
        {
            get { return activityList; }
        }

        private readonly ObservableCollection<UserInfo> filterUserList = new ObservableCollection<UserInfo>();
        public ObservableCollection<UserInfo> FilterUserList
        {
            get { return filterUserList; }
        }

        public ActivityManagement()
        {
            InitializeComponent();
            InitializeComponentData();
            InitializeDataValue();
        }

        private void InitializeComponentData()
        {
            this.DataContext = this;
            CurrentDate.SelectedDate = CurrentDate.DisplayDate;
        }

        private void InitializeDataValue()
        {
            FilterUserList.Clear();
            var users = DBAccess.GetAllUser();
            users.ForEach(u => FilterUserList.Add(u));
        }

        private void CurrentDate_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCurrentAttendInfo();
        }

        private void LoadCurrentAttendInfo()
        {
            var date = CurrentDate.SelectedDate;
            if (date == null) return;

            var list = DBAccess.GetAllRaidInfo(date.Value);
            ActivityList.Clear();
            ActivityList.Add("祭坛占领");
            ActivityList.Add("讨伐");
            list.ForEach(l => ActivityList.Add(l.Name));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentAttendInfo();
            InitializeDataValue();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAttendInfo();
        }

        private void RefreshAttendInfo()
        {
            var date = CurrentDate.SelectedDate;
            if (date == null) return;

            attendInfos.Clear();
            var item = ActivityCombo.SelectedItem as string;
            if (!string.IsNullOrEmpty(item))
            {
                var list = DBAccess.GetAllActivityAttendInfo(date.Value, item);
                attendInfos.AddRange(list);
            }

            RefreshAbsentUserFilter();
            RefreshAttendUserFilter();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = FilterAbsentUserInfo(e.Item);
        }

        private void CollectionViewSource_Filter_1(object sender, FilterEventArgs e)
        {
            e.Accepted = FilterAttendUserInfo(e.Item);
        }

        private bool FilterAbsentUserInfo(object item)
        {
            var user = item as UserInfo;
            if (user == null) return false;
            if (user.IsRemoved) return false;
            if (attendInfos.Any(a => a.UserId == user.Id)) return false;
            if (ActivityCombo.SelectedIndex < 0) return false;

            var filterName = FilterText1.Text.ToLower();
            if (string.IsNullOrEmpty(filterName)) return true;

            return user.Name.ToLower().Contains(filterName)
                || user.Pinyin.ToLower().Contains(filterName)
                || user.ShortName.ToLower().Contains(filterName);
        }

        private bool FilterAttendUserInfo(object item)
        {
            var user = item as UserInfo;
            if (user == null) return false;
            if (user.IsRemoved) return false;
            if (!attendInfos.Any(a => a.UserId == user.Id)) return false;
            if (ActivityCombo.SelectedIndex < 0) return false;

            var filterName = FilterText2.Text.ToLower();
            if (string.IsNullOrEmpty(filterName)) return true;

            return user.Name.ToLower().Contains(filterName)
                || user.Pinyin.ToLower().Contains(filterName)
                || user.ShortName.ToLower().Contains(filterName);
        }

        private void RefreshAbsentUserFilter()
        {
            CollectionViewSource src = (CollectionViewSource)this.FindResource("UserCollectionViewSource1");
            if (src.View == null) return;
            src.View.Refresh();
        }

        private void RefreshAttendUserFilter()
        {
            CollectionViewSource src = (CollectionViewSource)this.FindResource("UserCollectionViewSource2");
            if (src.View == null) return;
            src.View.Refresh();
        }

        private void FilterText1_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshAbsentUserFilter();
        }

        private void FilterText2_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshAttendUserFilter();
        }

        private void AttendBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = AbsentGroup.SelectedItem as UserInfo;
            if (item == null) return;

            var attend = new ActivityAttendInfo 
            {
                Date = CurrentDate.SelectedDate.Value,
                UserId = item.Id,
                ActivityName = ActivityCombo.Text
            };
            DBAccess.AddAttendInfo(attend);
            RefreshAttendInfo();
        }

        private void DeAttendBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = PresentGroup.SelectedItem as UserInfo;
            if (item == null) return;

            var attend = new ActivityAttendInfo
            {
                Date = CurrentDate.SelectedDate.Value,
                UserId = item.Id,
                ActivityName = ActivityCombo.Text
            };
            DBAccess.RemoveAttendInfo(attend);
            RefreshAttendInfo();
        }
    }
}
