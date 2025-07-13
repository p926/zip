using Instadose.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace portal_instadose_com_v3.Services
{
	/// <summary>
	/// Summary description for Renewal
	/// </summary>
	[WebService(Namespace = "http://portal.instadose.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class Renewal : System.Web.Services.WebService
	{
		[WebMethod]
		public string ReprocessRenewalByThread(int renewalThreadLogID, string userName)
		{
			RenewalByThread renewal = new RenewalByThread();
			renewal.UserName = userName;

			bool isPrcessed = true;
			string msg = string.Empty;

			bool isAllProcessed = false;
			try
			{
				isAllProcessed = renewal.IsAllProcessed(renewalThreadLogID);
			}
			catch (Exception ex)
			{
				isPrcessed = false;
				msg = ex.Message;
			}

			if (isAllProcessed)
			{
				isPrcessed = false;
				msg = "All accounts for the renewal were processed already.";
			}
			else
			{
				Thread trd = new Thread(() =>
				{
					string errMsg = string.Empty;

					try
					{
						renewal.ReProcessRenewalUnProcessed(renewalThreadLogID);
					}
					catch (ThreadAbortException taex)
					{
						errMsg = taex.Message;
					}
					catch (Exception ex)
					{
						errMsg = ex.Message;
					}

					renewal.UpdateCompleteRenewalThreadLog(renewalThreadLogID, errMsg);
				});

				trd.IsBackground = true;
				trd.Start();
			}

			var rtn = new { Processed = isPrcessed, Message = msg };

			JavaScriptSerializer js = new JavaScriptSerializer();

			return js.Serialize(rtn);
		}
	}
}
