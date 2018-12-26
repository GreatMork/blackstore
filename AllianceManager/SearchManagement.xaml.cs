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
    /// SearchManagement.xaml 的交互逻辑
    /// </summary>
    public partial class SearchManagement : UserControl
    {
        private readonly ObservableCollection<UserInfo> filterUserList = new ObservableCollection<UserInfo>();
        public ObservableCollection<UserInfo> FilterUserList
        {
            get { return filterUserList; }
        }

        public SearchManagement()
        {
            InitializeComponent();
            InitializeComponentData();
            InitializeDataValue();
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshUserFilter();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeDataValue();
        }

        private void InitializeComponentData()
        {
            this.DataContext = this;
        }

        private void InitializeDataValue()
        {
            FilterUserList.Clear();
            var users = DBAccess.GetAllUser();
            users.ForEach(u => FilterUserList.Add(u));
            UserGroup.SelectedIndex = -1;
            SignRecord.Text = string.Empty;
        }

        private void RefreshUserFilter()
        {
            CollectionViewSource src = (CollectionViewSource)this.FindResource("UserCollectionViewSource");
            src.View.Refresh();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = FilterUserInfo(e.Item);
        }

        private bool FilterUserInfo(object item)
        {
            var user = item as UserInfo;
            if (user == null) return false;
            if (user.IsRemoved) return false;

            var filterName = FilterText.Text.ToLower();
            if (string.IsNullOrEmpty(filterName)) return true;

            return user.Name.ToLower().Contains(filterName)
                || user.Pinyin.ToLower().Contains(filterName)
                || user.ShortName.ToLower().Contains(filterName);
        }

        private void UserGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = UserGroup.SelectedItem as UserInfo;
            if (item != null)
            {
                ShowAttendInfo(item.Id);
            }
        }

        private void ShowAttendInfo(int userId)
        {
            var list = DBAccess.GetActivityAttendInfoByUserId(userId).OrderBy(t => t.Date);

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.AppendLine(string.Format("[{0}] 参加了 {1}.", item.Date.ToString("yyyy-MM-dd"), item.ActivityName));
            }

            SignRecord.Text = sb.ToString();
        }
    }
}
