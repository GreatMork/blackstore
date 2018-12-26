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
    /// UserBlackList.xaml 的交互逻辑
    /// </summary>
    public partial class UserBlackList : UserControl
    {
        private readonly ObservableCollection<UserInfo> filterUserList = new ObservableCollection<UserInfo>();
        public ObservableCollection<UserInfo> FilterUserList
        {
            get { return filterUserList; }
        }

        public UserBlackList()
        {
            InitializeComponent();
            InitializeComponentData();
            InitializeDataValue();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeDataValue();
        }

        private void InitializeComponentData()
        {
            this.DataContext = this;
            SexCombo.ItemsSource = UserManagement.sexList;
            CareerCombo.ItemsSource = UserManagement.careerList;
            DutyCombo.ItemsSource = UserManagement.dutyList;
        }

        private void InitializeDataValue()
        {
            FilterUserList.Clear();
            var users = DBAccess.GetAllUser();
            users.ForEach(u => FilterUserList.Add(u));
            NameTxt.Text = string.Empty;
            IDTxt.Text = string.Empty;
            DescTxt.Text = string.Empty;
            SexCombo.SelectedIndex = 0;
            CareerCombo.SelectedIndex = 0;
            DutyCombo.SelectedIndex = 7;
            UserGroup.SelectedIndex = -1;
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = FilterUserInfo(e.Item);
        }

        private bool FilterUserInfo(object item)
        {
            var user = item as UserInfo;
            if (user == null) return false;
            if (!user.IsRemoved) return false;

            var filterName = FilterText.Text.ToLower();
            if (string.IsNullOrEmpty(filterName)) return true;

            return user.Name.ToLower().Contains(filterName)
                || user.Pinyin.ToLower().Contains(filterName)
                || user.ShortName.ToLower().Contains(filterName);
        }

        private void RefreshUserFilter()
        {
            CollectionViewSource src = (CollectionViewSource)this.FindResource("UserCollectionViewSource");
            src.View.Refresh();
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshUserFilter();
        }

        private void UserGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = UserGroup.SelectedItem as UserInfo;
            UserIDUpdateBtn.IsEnabled = item != null;
            UserDescUpdateBtn.IsEnabled = item != null;
            MoreOptionGroup.IsEnabled = item != null;
            if (item != null)
            {
                NameTxt.Text = item.Name;
                IDTxt.Text = item.UserId;
                DescTxt.Text = item.Description;
                SexCombo.SelectedIndex = item.Sex;
                CareerCombo.SelectedIndex = item.Career;
                DutyCombo.SelectedIndex = item.Duty;
            }
        }

        private void UserDescUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = UserGroup.SelectedItem as UserInfo;
            if (item != null)
            {
                item.Description = DescTxt.Text;
                DBAccess.UpdateUser(item);
                RefreshUserFilter();
            }
        }

        private void UserIDUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = UserGroup.SelectedItem as UserInfo;
            if (item != null)
            {
                item.UserId = IDTxt.Text;
                DBAccess.UpdateUser(item);
                RefreshUserFilter();
            }
        }

        private void RestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = UserGroup.SelectedItem as UserInfo;
            if (item != null)
            {
                var result = MessageBox.Show(string.Format("是否恢复[{0}]的身份?", item.Name), "询问", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    item.IsRemoved = false;
                    item.Duty = 7;
                    DBAccess.UpdateUser(item);
                    RefreshUserFilter();
                }
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = UserGroup.SelectedItem as UserInfo;
            if (item != null)
            {
                var result = MessageBox.Show(string.Format("删除后,[{0}]的所有信息将被清除,是否继续?", item.Name), "警告", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    FilterUserList.Remove(item);
                    DBAccess.RemoveUser(item);
                    RefreshUserFilter();
                }
            }
        }
    }
}
