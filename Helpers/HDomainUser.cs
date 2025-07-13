using Instadose.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace portal_instadose_com_v3.Helpers
{
    public class HDomainUser
    {
        public static bool IsCollectionUser(string userName)
        {
            InsDataContext idc = new InsDataContext();

            var collectionUserSetting = idc.AppSettings.FirstOrDefault(a => a.AppKey == "CollectionUsers");

            if (collectionUserSetting == null)
                return false;

            string collectionUser = collectionUserSetting.Value;

            if (string.IsNullOrEmpty(collectionUser))
                return false;

            string[] collectionUsers = collectionUser.Split(',');

            return collectionUsers.Contains(userName, StringComparer.OrdinalIgnoreCase);
        }
    }
}