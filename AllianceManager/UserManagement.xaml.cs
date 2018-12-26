﻿using NPinyin;
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
    /// UserManagement.xaml 的交互逻辑
    /// </summary>
    public partial class UserManagement : UserControl
    {
        public static string[] sexList = new string[]
        {
            "男", "女", "未知"
        };

        public static string[] careerList = new string[]
        {
            "铁卫", "守护者", "角斗士",
            "狂战士", "武器大师", "执政官",
            "剑圣", "魔剑士", "影舞者",
            "烈焰咏者", "霜语者", "霜火法师",
            "战斗法师", "奥数学者", "水晶先知",
            "混沌术士", "死灵法师", "吸血鬼",
            "主教", "圣者", "光明使者",
            "圣堂武士", "审判者", "龙骑士",
            "吟游诗人", "神射手", "狩魔猎人"
        };

        public static string[] dutyList = new string[]
        {
            "会长", "副会长", "内政大臣", "国防大臣", "文化大臣", "公会之星", "精英", "会员", "学徒"
        };

        private EditModeEnum editMode = EditModeEnum.Add;

        private readonly ObservableCollection<UserInfo> filterUserList = new ObservableCollection<UserInfo>();
        public ObservableCollection<UserInfo> FilterUserList
        {
            get { return filterUserList; }
        }

        public UserManagement()
        {
            InitializeComponent();
            InitializeComponentData();
            InitializeDataValue();
        }

        private void InitializeComponentData()
        {
            this.DataContext = this;
            SexCombo.ItemsSource = sexList;
            CareerCombo.ItemsSource = careerList;
            DutyCombo.ItemsSource = dutyList;
            
        }

        private void InitializeDataValue()
        {
            FilterUserList.Clear();
            var users = DBAccess.GetAllUser();
            users.ForEach(u => FilterUserList.Add(u));
            NameTxt.Text = string.Empty;
            IDTxt.Text = string.Empty;
            SexCombo.SelectedIndex = 0;
            CareerCombo.SelectedIndex = 0;
            DutyCombo.SelectedIndex = 7;
            UserGroup.SelectedIndex = -1;
        }

        private void UserGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = UserGroup.SelectedItem as UserInfo;
            UserEditBtn.IsEnabled = (item != null);
            UserDeleteBtn.IsEnabled = (item != null);
            UserEditArea.IsEnabled = false;
            if (item != null)
            {
                NameTxt.Text = item.Name;
                IDTxt.Text = item.UserId;
                SexCombo.SelectedIndex = item.Sex;
                CareerCombo.SelectedIndex = item.Career;
                DutyCombo.SelectedIndex = item.Duty;
            }
        }

        private void UserAddBtn_Click(object sender, RoutedEventArgs e)
        {
            editMode = EditModeEnum.Add;
            InitializeDataValue();
            UserEditArea.IsEnabled = true;
        }

        private void UserEditBtn_Click(object sender, RoutedEventArgs e)
        {
            editMode = EditModeEnum.Edit;
            UserEditArea.IsEnabled = true;
        }

        private void UserDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = UserGroup.SelectedItem as UserInfo;
            if (item == null) return;
            var result = MessageBox.Show(string.Format("是否将<{0}>除名?\n（除名后可以在除名列表中将其加回来，数据依然保存）", item.Name), "询问", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                item.IsRemoved = true;
                DBAccess.UpdateUser(item);
                RefreshUserFilter();
            }
        }

        private void UserSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (editMode == EditModeEnum.Add)
            {
                AddUser();
            }
            else if (editMode == EditModeEnum.Edit)
            {
                EditUser();
            }
        }

        private void AddUser()
        {
            string name = NameTxt.Text;
            string id = IDTxt.Text;
            int sex = SexCombo.SelectedIndex;
            int career = CareerCombo.SelectedIndex;
            int duty = DutyCombo.SelectedIndex;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("昵称是必须的，不然加毛成员?");
                return;
            }

            var duplex = FilterUserList.FirstOrDefault(s => s.Name.Equals(name));
            if (duplex != null)
            {
                if (duplex.IsRemoved)
                {
                    // 恢复除名
                    var result = MessageBox.Show(string.Format("<{0}>已被除名，是否加回来?", duplex.Name), "询问", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        duplex.IsRemoved = false;
                        DBAccess.UpdateUser(duplex);
                        UserEditArea.IsEnabled = false;
                        RefreshUserFilter();
                    }
                }
                else
                {
                    MessageBox.Show("该成员已经添加，如需修改信息，出门右转！");
                }
                return;
            }

            var pinyin = Pinyin.GetPinyin(name).Split(' ');
            var user = new UserInfo
            {
                Name = name,
                UserId = id,
                Sex = sex,
                Career = career,
                Duty = duty,
                Pinyin = string.Join("", pinyin),
                ShortName = pinyin.Length <= 1 ? name : string.Join("", pinyin.Select(s => s[0]))
            };


            // 添加
            var dbuser = DBAccess.AddUser(user);
            if (dbuser == null)
            {
                MessageBox.Show("额.添加数据库出错...");
                return;
            }
            
            UserEditArea.IsEnabled = false;
            FilterUserList.Add(dbuser);
            RefreshUserFilter();
        }

        private void EditUser()
        {
            var item = UserGroup.SelectedItem as UserInfo;
            if (item == null)
            {
                UserEditArea.IsEnabled = false;
                return;
            }

            string name = NameTxt.Text;
            string id = IDTxt.Text;
            int sex = SexCombo.SelectedIndex;
            int career = CareerCombo.SelectedIndex;
            int duty = DutyCombo.SelectedIndex;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("咋地，你想销户? 销户出门右转！");
                return;
            }

            var duplex = FilterUserList.FirstOrDefault(s => s.Name.Equals(name));
            if (duplex != null && duplex.Id != item.Id)
            {
                MessageBox.Show("拒绝重名，与现有成员或除名成员重名！");
                return;
            }

            var pinyin = Pinyin.GetPinyin(name).Split(' ');
            item.Name = name;
            item.UserId = id;
            item.Sex = sex;
            item.Career = career;
            item.Duty = duty;
            item.Pinyin = string.Join("", pinyin);
            item.ShortName = pinyin.Length <= 1 ? name : string.Join("", pinyin.Select(s => s[0]));

            DBAccess.UpdateUser(item);
            UserEditArea.IsEnabled = false;
            RefreshUserFilter();
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
            if(string.IsNullOrEmpty(filterName)) return true;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeDataValue();
        }
    }

    public enum EditModeEnum
    {
        Add, Edit
    }
}