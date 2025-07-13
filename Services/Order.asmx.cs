using Mirion.DSD.GDS.API;
using Mirion.DSD.GDS.API.Contexts;
using Mirion.DSD.GDS.API.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace portal_instadose_com_v3.Services
{
	/// <summary>
	/// Summary description for Order
	/// </summary>
	[WebService(Namespace = "http://portal.instadose.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class Order : System.Web.Services.WebService
	{

		[WebMethod]
		public string UpdateInstadoseOrderStatus(int instaOrderID, string orderStatus)
		{
			bool isSuccess = true;
			string msg = string.Empty;

			if (instaOrderID <= 0)
			{
				isSuccess = false;
				msg = "Instadose order number is invalid.";
			}
			else if (string.IsNullOrEmpty(orderStatus) || orderStatus.Length != 1)
			{
				isSuccess = false;
				msg = "Instadose Order Status Code is not valid.";
			}
			else
			{
				InstadoseRequests req = new InstadoseRequests();

				int resopnseCode = req.UpdateInstadoseOrderStatus(instaOrderID, orderStatus);
			}

			JavaScriptSerializer js = new JavaScriptSerializer();

			return js.Serialize(new
			{
				Success = isSuccess,
				ErrorMessage = msg,
				InstaOrderID = instaOrderID
			});
		}

		[WebMethod]
		public string GetInstaOrderDetails(int instaOrderID)
		{
			InstadoseOrderRequests request = new InstadoseOrderRequests();

			var order = request.GetInstaOrdersDailyWorkOrderSqlAndPLOrderUnion(instaOrderID);
			List<vw_GDSInstaOrderDetail> orderDetails = new List<vw_GDSInstaOrderDetail>();

			if (order != null)
				orderDetails = request.GetInstaOrderBadges(instaOrderID);

			var badges = orderDetails.OrderBy(od => od.BadgeType).ThenBy(od => od.FirstName).Select(od => new { od.BadgeType, od.BadgeDesc, od.BodyRegion, od.Color, od.GDSWearer, od.FirstName, od.LastName, od.SerialNo }).ToList();

			JavaScriptSerializer js = new JavaScriptSerializer();

			return js.Serialize(new
			{
				InstaOrder = new
				{
					InstaOrderID = order.InstaOrderID,
					PLOrderID = order.PLOrderID,
					GDSAccount = order.GDSAccount,
					GDSLocation = order.GDSLocation,
					OrderTypeName = order.OrderTypeName,
					Status = order.ShipDate != null ? "S" : order.GDSInstaOrderStatus,
					StatusName = order.ShipDate != null ? "Shipped" : order.OrderStatusName,
					MfgRunCode = order.MfgRunCode,
					WearDate = order.WearDate,
					OrderDate = order.OrderDate
				},
				InstaOrderBadges = badges
			});
		}
	}
}
