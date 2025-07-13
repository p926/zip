using Instadose.API.DA;
using Instadose.Data;
using Instadose.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

namespace portal_instadose_com_v3.Services
{
    /// <summary>
    /// Summary description for User
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class User : System.Web.Services.WebService
    {
        public DAUsers _user;
        public InsDataContext _idc;
        public User()
        {
            _user = new DAUsers();
            _idc = new InsDataContext();
        }

        [WebMethod]
        public string SendWelcomeEmail(int[] users, bool access)
        {
            
            Dictionary<int, string> response = new Dictionary<int, string>();
            
            foreach (var user in users)
            {
                var authuser = (from au in _idc.AuthUsers
                                join u in _idc.Users on au.UserName equals u.UserName
                                where u.UserID == user
                                select au).FirstOrDefault();
                var passwordHash = authuser.PasswordHash;
                string token = Hash.GetHash(passwordHash.Substring(0, 10), Hash.HashType.SHA256);

                //lookup username
                var username = authuser.UserName;
                if (string.IsNullOrEmpty(username))
                    response.Add(user, "Username not found.");
                else
                {
                    try
                    {
                        var success = _user.SendWelcomeEmail(username, access, token);
                        if (success)
                            response.Add(user, "");
                        else
                            response.Add(user, "Email failed to be sent.");
                    }
                    catch (Exception ex)
                    {
                        response.Add(user, ex.Message);
                    }
                }
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        [WebMethod]
        public string ResetPassword(string username)
        {
            Dictionary<string, string> response = new Dictionary<string, string>();
            try
            {
                int responseCode = _user.ResetPassword(username, "System");

                if (responseCode != 0)
                {
                    throw new Exception(string.Format("{0}: {1}", responseCode, APIHandlers.GetErrorMessage(responseCode)));
                }
                else
                {
                    response.Add("Success", responseCode.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Add("Error", ex.Message);
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        [WebMethod]
        public string ResetPasswords(int[] users)
        {
            Dictionary<int, string> response = new Dictionary<int, string>();
            foreach (var user in users)
            {
                //lookup username
                var authuser = (from au in _idc.AuthUsers
                                join u in _idc.Users on au.UserName equals u.UserName
                                where u.UserID == user
                                select au).FirstOrDefault();
                var username = authuser.UserName;
                if (string.IsNullOrEmpty(username))
                    response.Add(user, "Username not found.");
                else
                {
                    int responseCode = _user.ResetPassword(username,"System");
                    var success = responseCode != 0;
                    if (success)
                        response.Add(user, "");
                    else
                        response.Add(user, APIHandlers.GetErrorMessage(responseCode));
                }
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        [WebMethod]
        public int ToggleStatus(int[] users, bool active)
        {
            var authusers = (from au in _idc.AuthUsers
                             join u in _idc.Users on au.UserName equals u.UserName
                             where users.Contains(u.UserID)
                             select au).ToList();
            foreach (var authuser in authusers)
            {
                authuser.Active = active;
            }
            _idc.SubmitChanges();

            return 0;
        }
    }
}
