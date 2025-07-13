using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using Instadose.API.DA;

namespace portal_instadose_com_v3.Services
{
    /// <summary>
    /// Summary description for AccountDevice
    /// </summary>
    [WebService(Namespace = "http://portal.instadose.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class AccountDevice : System.Web.Services.WebService
    {
        [WebMethod]
        public string AssignAccountDevices(string SerialNumbers, bool IsAssign)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (string.IsNullOrEmpty(SerialNumbers))
            {
                return js.Serialize(new
                {
                    Success = false,
                    Message = "Serial Numbers are missing.",
                    SuccessSerialNumbers = (IEnumerable)null,
                    ErrorSerialNumberMessages = (IEnumerable)null
                });
            }

            var deviceMgr = new DADevices();

            List<string> serialNum_Success = new List<string>();
            List<string> serialNum_Error = new List<string>();

            string[] arrSerialNums = SerialNumbers.Split(',');

            foreach (string serialNum in arrSerialNums)
            {
                string serialNo = serialNum.Trim();

                if (string.IsNullOrEmpty(serialNo))
                    continue;

                if (!deviceMgr.IsAccountDeviceExist(serialNo))
                {
                    serialNum_Error.Add(string.Format("Serial #{0} - Device not exist.", serialNo));
                    continue;
                }

                bool success = true;
                string errMsg = "";

                try
                {
                    success = deviceMgr.UpdateCurrentDeviceAssign(serialNum, IsAssign);
                }
                catch (Exception ex)
                {
                    success = false;
                    errMsg = ex.Message;
                }

                if (success)
                    serialNum_Success.Add(serialNo);
                else
                    serialNum_Error.Add(string.Format("Serial #{0} - {1}", serialNo, errMsg));
            }

            return js.Serialize(new
            {
                Success = true,
                Message = "",
                SuccessSerialNumbers = serialNum_Success,
                ErrorSerialNumberMessages = serialNum_Error
            });
        }
    }
}
