using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// AdvanceMode.xaml 的交互逻辑
    /// </summary>
    public partial class AdvanceMode : UserControl
    {
        public AdvanceMode()
        {
            InitializeComponent();
        }

        private void UserExportBtn_Click(object sender, RoutedEventArgs e)
        {
            var users = DBAccess.GetAllUser();
            var sb = new StringBuilder();
            sb.AppendLine("ID,Name,UserId,Sex,Career,Duty,Pinyin,ShortName,IsRemoved,Description");
            foreach (var u in users)
            {
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", u.Id, u.Name, u.UserId, u.Sex, u.Career, u.Duty, u.Pinyin, u.ShortName, u.IsRemoved, u.Description));
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, sb.ToString());
                MessageBox.Show("导出成功");
            }
        }
    }
}
