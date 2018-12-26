using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllianceManager
{
    public class DBAccess
    {
        private const string dbName = @"blackstore.db";

        private static LiteDatabase db = new LiteDatabase(dbName);

        public static void Dispose()
        {
            db.Dispose();
        }

        public static UserInfo AddUser(UserInfo user)
        {
            var users = db.GetCollection<UserInfo>("users");
            users.Insert(user);
            return users.Find(x => x.Name.Equals(user.Name)).First();
        }

        public static void UpdateUser(UserInfo user)
        {
            var users = db.GetCollection<UserInfo>("users");
            users.Update(user);
        }

        public static void RemoveUser(UserInfo user)
        {
            var users = db.GetCollection<UserInfo>("users");
            users.Delete(user.Id);
        }

        public static List<UserInfo> GetAllUser()
        {
            var users = db.GetCollection<UserInfo>("users");
            return users.FindAll().ToList();
        }

        public static List<RaidInfo> GetAllRaidInfo(DateTime date)
        {
            var raids = db.GetCollection<RaidInfo>("raids");
            return raids.Find(x => x.Date.Equals(date)).ToList();
        }

        public static void AddRaidInfo(RaidInfo info)
        {
            var raids = db.GetCollection<RaidInfo>("raids");
            raids.Insert(info);
        }

        public static void RemoveRaidInfo(RaidInfo info)
        {
            var raids = db.GetCollection<RaidInfo>("raids");
            raids.Delete(info.Id);
        }

        public static bool HasActivityByRaidInfo(RaidInfo info)
        {
            var activities = db.GetCollection<ActivityAttendInfo>("activities");
            var count = activities.Count(t => t.ActivityName.Equals(info.Name) && t.Date.Equals(info.Date));
            return count >= 1;
        }

        public static void ClearAllActivitysByRaidInfo(RaidInfo info)
        {
            var activities = db.GetCollection<ActivityAttendInfo>("activities");
            activities.Delete(t => t.ActivityName.Equals(info.Name) && t.Date.Equals(info.Date));
        }

        public static List<ActivityAttendInfo> GetAllActivityAttendInfo(DateTime date, string activityName)
        {
            var activities = db.GetCollection<ActivityAttendInfo>("activities");
            return activities.Find(x => x.Date.Equals(date) && x.ActivityName.Equals(activityName)).ToList();
        }

        public static List<ActivityAttendInfo> GetAllActivityAttendInfo(DateTime startDate, DateTime endDate, string likeName)
        {
            var activities = db.GetCollection<ActivityAttendInfo>("activities");
            return activities.Find(x => x.Date >= startDate && x.Date <= endDate && x.ActivityName.Contains(likeName)).ToList();
        }

        public static List<ActivityAttendInfo> GetActivityAttendInfoByUserId(int userId)
        {
            var activities = db.GetCollection<ActivityAttendInfo>("activities");
            return activities.Find(x=> x.UserId == userId).ToList();
        }

        public static void AddAttendInfo(ActivityAttendInfo info)
        {
            var activities = db.GetCollection<ActivityAttendInfo>("activities");
            activities.Insert(info);
        }

        public static void RemoveAttendInfo(ActivityAttendInfo info)
        {
            var activities = db.GetCollection<ActivityAttendInfo>("activities");
            activities.Delete(t => t.Date.Equals(info.Date) && t.ActivityName.Equals(info.ActivityName) && t.UserId.Equals(info.UserId));
        }
    }
}
