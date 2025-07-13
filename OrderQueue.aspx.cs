using Mirion.DSD.GDS.API.Contexts;
using Mirion.DSD.GDS.API.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace portal_instadose_com_v3.TechOps
{
	public partial class OrderQueue : System.Web.UI.Page
	{
        #region Events
        protected void Page_Load(object sender, EventArgs e)
		{
            _badgeSummaryOrderStatuses = new Dictionary<string, string>
            {
                { "G", "Fulfillment" },
                { "R", "Released" },
                { "P", "Packaged" },
                { "S", "Shipped" }
            };

            if (!IsPostBack)
			{
				LoadOrderStatusDropDown();

                //txtOrderFilterDateRangeFrom.Text = DateTime.Now.AddDays(-7).ToString("d");
                txtOrderFilterDateRangeFrom.Text = DateTime.Now.AddYears(-2).ToString("d");
                txtOrderFilterDateRangeTo.Text = DateTime.Now.ToString("d");

                List<GDSInstaOrderQueueList> orders = GetOrders();

                if (orders != null && orders.Count > 0)
                {
                    orders = orders.OrderBy(o => o.InstaOrderID).ToList();
                }

				//Session["sortDirection"] = SortDirection.Ascending;
				Session["InstaOrders"] = orders;

                RenderOrderListInfos(0, true);
			}
		}

		protected void btnOrderSearch_Click(object sender, EventArgs e)
		{
            List<GDSInstaOrderQueueList> orders = GetOrders();

            if (orders != null && orders.Count > 0)
            {
                orders = orders.OrderBy(o => o.InstaOrderID).ToList();
            }

            Session["InstaOrders"] = orders;

            RenderOrderListInfos(0, true);
        }

		protected void gvInstaOrders_Sorting(object sender, GridViewSortEventArgs e)
		{
			if ((SortDirection)Session["sortDirection"] == SortDirection.Ascending)
			{
				e.SortDirection = SortDirection.Ascending;
				Session["sortDirection"] = SortDirection.Descending;
			}
			else
			{
				e.SortDirection = SortDirection.Descending;
				Session["sortDirection"] = SortDirection.Ascending;
			}

            List<GDSInstaOrderQueueList> orders = Session["InstaOrders"] as List<GDSInstaOrderQueueList>; //GetOrders();

            if (orders != null && orders.Count > 0)
			{
                var pInfo = typeof(GDSInstaOrderQueueList).GetProperty(e.SortExpression);

                if (e.SortDirection == SortDirection.Ascending)
				{
					orders = orders.OrderBy(r => pInfo.GetValue(r, null)).ToList();
					/*
					if (e.SortExpression == "InitScheduleDate")
						orders = orders.OrderByDescending(r => r.InitScheduleDate.HasValue).ThenBy(r => r.InitScheduleDate).ToList();
					else
						orders = orders.OrderBy(r => pInfo.GetValue(r, null)).ToList();
					*/
				}
				else
				{
					orders = orders.OrderByDescending(r => pInfo.GetValue(r, null)).ToList();
					/*
					if (e.SortExpression == "InitScheduleDate")
						orders = orders.OrderBy(r => r.InitScheduleDate.HasValue).ThenByDescending(r => r.InitScheduleDate).ToList();
					else
						orders = orders.OrderByDescending(r => pInfo.GetValue(r, null)).ToList();
					*/
				}

                Session["InstaOrders"] = orders;

            }

            RenderOrderListInfos();
        }

		protected void gvInstaOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
            RenderOrderListInfos(e.NewPageIndex);
        }

		protected void gvInstaOrders_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				GridViewRow row = e.Row;

                GDSInstaOrderQueueList order = row.DataItem as GDSInstaOrderQueueList;

                if (order != null)
				{
					HyperLink hlInstaOrderID = row.FindControl("hlInstaOrderID") as HyperLink;
					Literal ltShippingMethod = row.FindControl("ltShippingMethod") as Literal;
					Image imgExpress = row.FindControl("imgExpress") as Image;
					//HyperLink hlReleaseOrder = row.FindControl("hlReleaseOrder") as HyperLink;

					hlInstaOrderID.Text = order.InstaOrderID.ToString();
					hlInstaOrderID.NavigateUrl = string.Format("javascript:openInstaOrderDetailDialog({0});", order.InstaOrderID);

                    ltShippingMethod.Text = order.PLOrderID != null ? GetShippingMethodName(order.ShippingMethodID) : string.Empty;
					imgExpress.Visible = order.ExpressShipment == true;

                    if (order.CreditHold == "H")
                    {
                        e.Row.Style.Add("background-color", "yellow");
                    }

					//hlReleaseOrder.Visible = false;

					//if (order.DailyStatus == "G" && order.ShipDate == null)
					//{
					//	hlReleaseOrder.Visible = true;
					//	hlReleaseOrder.NavigateUrl = string.Format("javascript:releaseInstadoseOrder({0});", order.InstaOrderID);
					//}
					//else
					//{
					//	hlReleaseOrder.Visible = false;
					//}
				}
			}
		}

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            List<GDSInstaOrderQueueList> orders = Session["InstaOrders"] as List<GDSInstaOrderQueueList>;

            if (orders != null && orders.Count > 0)
            {
                // Create an data table used to export
                DataTable exportTable = new DataTable();
                exportTable.Columns.Add("Temp #");
                exportTable.Columns.Add("Order #");
                exportTable.Columns.Add("Company");
                exportTable.Columns.Add("Account");
                exportTable.Columns.Add("Location");
                exportTable.Columns.Add("Type");
                exportTable.Columns.Add("Wear Date");
                exportTable.Columns.Add("ID");
                exportTable.Columns.Add("ID+");
                exportTable.Columns.Add("ID2");
                exportTable.Columns.Add("IDVue");
                exportTable.Columns.Add("IDVue-B");
                exportTable.Columns.Add("USB");
                exportTable.Columns.Add("Instalink");
                exportTable.Columns.Add("Instalink3");
                exportTable.Columns.Add("Status");
                exportTable.Columns.Add("Order Date");
                exportTable.Columns.Add("Ship");
                exportTable.Columns.Add("Express");
                exportTable.Columns.Add("CreditHold");

                // Add the rows from the order list
                foreach (GDSInstaOrderQueueList order in orders)
                {
                    // Create a new table row
                    DataRow dr = exportTable.NewRow();

                    dr[0] = order.InstaOrderID.ToString();
                    dr[1] = order.PLOrderID.ToString();
                    dr[2] = order.CompanyName;
                    dr[3] = order.GDSAccount;
                    dr[4] = order.GDSLocation;
                    dr[5] = order.OrderTypeName;
                    dr[6] = order.WearDate.HasValue ? ((DateTime)order.WearDate).ToString("MM/dd/yyyy") : "";
                    dr[7] = order.InstadoseCount.ToString();
                    dr[8] = order.InstadosePlusCount.ToString();
                    dr[9] = order.Instadose2Count.ToString();
                    dr[10] = order.Instadose3.ToString();
                    dr[11] = order.InstadoseVueBeta.ToString();
                    dr[12] = order.InstaLinkUSB.ToString();
                    dr[13] = order.InstaLink.ToString();
                    dr[14] = order.InstaLink3.ToString();
                    dr[15] = order.OrderStatusName;
                    dr[16] = order.OrderDate.HasValue ? ((DateTime)order.OrderDate).ToString("MM/dd/yyyy") : "";
                    dr[17] = GetShippingMethodName(order.ShippingMethodID);
                    dr[18] = order.ExpressShipment == true ? "Y" : "N";
                    dr[19] = order.CreditHold;

                    // Add the row to the table
                    exportTable.Rows.Add(dr);
                }

                // Build the export table
                TableExport tableExport = new TableExport(exportTable);

                //try
                //{
                //    // Read the CSS template from file
                //    tableExport.Stylesheet =
                //        System.IO.File.ReadAllText(Server.MapPath("~/_templates/export/grids.css"));
                //}
                //catch { }
                
                try
                {
                    tableExport.Export("InstadosePlusOrderQueue", "XLS");

                    ExportFile file = tableExport.File;

                    // Clear everything out.
                    Response.Clear();
                    Response.ClearHeaders();

                    // Set the response headers.
                    Response.ContentType = file.ContentType;
                    Response.AddHeader("Content-Disposition", file.ContentDisposition);

                    // Write to Excel file.
                    if (file.Content.GetType() == typeof(byte[]))
                        Response.BinaryWrite((byte[])file.Content);
                    else
                    {
                        Response.Write(file.Content.ToString());
                    }
                }
                catch (Exception ex)
                {
                    return;
                }

                Response.Flush();
                Response.End();

            }
        }
        #endregion
        
        private void LoadOrderStatusDropDown()
		{
			ddlOrderFilterStatus.Items.Clear();

            // load 'All' status
			ddlOrderFilterStatus.Items.Add(new ListItem("ALL", ""));

            // load 'Other than Shipped' status
            ddlOrderFilterStatus.Items.Add(new ListItem("Not Shipped", "NS"));
            
            if (_instaOrderStatuses != null && _instaOrderStatuses.Count >= 0)
			{
				foreach (var status in _instaOrderStatuses)
				{
					ddlOrderFilterStatus.Items.Add(new ListItem(status.OrderStatusName, status.OrderStatus.ToString()));
				}
			}

            // Default page load looks for Fulfillment order status. Tdo, 01/13/2025
            // ddlOrderFilterStatus.SelectedValue = "NS";
            ddlOrderFilterStatus.SelectedValue = "G";
        }

        private void RenderOrderListInfos(int pageIndex = 0, bool resetSorting = false)
        {
            BindOrderListGrid(pageIndex, resetSorting);
            BindBadgeCountSummary();
        }

        private void BindOrderListGrid(int pageIndex = 0, bool resetSorting = false)
        {
            List<GDSInstaOrderQueueList> orders = Session["InstaOrders"] as List<GDSInstaOrderQueueList>;
            
            gvInstaOrders.PageIndex = pageIndex;
            gvInstaOrders.DataSource = orders;

            if (resetSorting)
            {
                Session["sortDirection"] = SortDirection.Ascending;

                gvInstaOrders.Sort("InstaOrderID", SortDirection.Ascending);
            }

            gvInstaOrders.DataBind();
        }

        private void BindBadgeCountSummary()
        {
            List<GDSInstaOrderQueueList> orders = Session["InstaOrders"] as List<GDSInstaOrderQueueList>;
            List<BadgeCountSummary> badgeCountSummaries = new List<BadgeCountSummary>();
            
            if (orders == null || orders.Count <= 0)
            {
                foreach (var orderStatus in _badgeSummaryOrderStatuses)
                {
                    badgeCountSummaries.Add(new BadgeCountSummary(orderStatus.Key, orderStatus.Value, null));
                }
            }
            else
            {
                foreach (var orderStatus in _badgeSummaryOrderStatuses)
                {
                    badgeCountSummaries.Add(new BadgeCountSummary(orderStatus.Key, orderStatus.Value, orders.Where(o => o.GDSInstaOrderStatus == orderStatus.Key)));
                }
            }

            rptBadgeSummaries.DataSource = badgeCountSummaries;
            rptBadgeSummaries.DataBind();
        }

        /// <summary>
        /// Pull data from vw_GDSInstaOrder view and convert to child class GDSInstaOrderList
        /// </summary>
        /// <returns></returns>
        private List<GDSInstaOrderQueueList> GetOrders()
        {
			InstadoseOrderRequests request = new InstadoseOrderRequests();
            List<vw_GDSInstaOrdersQueue> orders = new List<vw_GDSInstaOrdersQueue>();
           
            string orderNumber = txtOrderFilterOrderNumber.Text.Trim();
            
            string account = txtOrderFilterAccount.Text.Trim();
			string location = txtOrderFilterLocation.Text.Trim();
			string stat = ddlOrderFilterStatus.SelectedValue.Trim();

			if (!string.IsNullOrEmpty(orderNumber))
            {
                // search order by PLOrderID
                if (int.TryParse(orderNumber, out int tmpOrderNumber))
                {
                    orders = request.GetInsatOrdersByPLOrderQueue(tmpOrderNumber);
                }
            }
            else
            {
                // search orders by search criteria
                List<string> statuses = new List<string>();

                // display pending and released order, if status fileter is selected as ALL
                if (string.IsNullOrEmpty(stat))
                {
                    if (_instaOrderStatuses != null && _instaOrderStatuses.Count > 0)
                    {
                        foreach (var availStat in _instaOrderStatuses)
                            statuses.Add(availStat.OrderStatus.ToString());
                    }
                }
                else if (stat.Equals("NS", StringComparison.CurrentCulture))
                {
                    if (_instaOrderStatuses != null && _instaOrderStatuses.Count > 0)
                    {
                        foreach (var availStat in _instaOrderStatuses)
                        {
                            if (availStat.OrderStatus.ToString() != "S")
                            {
                                statuses.Add(availStat.OrderStatus.ToString());
                            }
                        }
                    }
                }
                else
                {
                    statuses.Add(stat);
                }

                // create order type list by selected search filter
                List<int> orderTypes = new List<int>();
                if (!string.IsNullOrEmpty(ddlOrderFilterType.SelectedValue.Trim()))
                    orderTypes.Add(int.Parse(ddlOrderFilterType.SelectedValue.Trim()));
                else
                    orderTypes = null;

                // Order Date search filter values
                DateTime tmpDt;
                DateTime? fromDt = null;
                DateTime? toDt = null;

                if (!string.IsNullOrEmpty(txtOrderFilterDateRangeFrom.Text))
                {
                    if (DateTime.TryParse(txtOrderFilterDateRangeFrom.Text, out tmpDt))
                        fromDt = tmpDt;
                }

                if (!string.IsNullOrEmpty(txtOrderFilterDateRangeTo.Text))
                {
                    if (DateTime.TryParse(txtOrderFilterDateRangeTo.Text, out tmpDt))
                        toDt = tmpDt;
                }

                orders = request.GetInstaOrdersQueue(account, location, statuses, fromDt, toDt, null, orderTypes);
            }

            // create GDSInstaOrderList list by vw_GDSInstaOrder records
            List<GDSInstaOrderQueueList> orderList = new List<GDSInstaOrderQueueList>();

            if (orders != null && orders.Count > 0)
            {
                orderList.AddRange(from order in orders
                                   select new GDSInstaOrderQueueList(order));
            }

            return orderList;
		}

        /// <summary>
        /// Get Shipping Carrier name by ShippingMethodID
        /// </summary>
        /// <param name="shippingMethodID"></param>
        /// <returns></returns>
		private string GetShippingMethodName(int? shippingMethodID)
		{
			string methodName = "Malvern";

			switch (shippingMethodID)
			{
				case 1:
					methodName = "FedEx";
					break;
				case 2:
					methodName = "USPS";
					break;
				case 3:
					methodName = "Global";
					break;
				case 4:
					methodName = "DHL";
					break;
				case 5:
					methodName = "UPS";
					break;
			}

			return methodName;
		}

        /// <summary>
        /// Get OrderStatus records from Session["InstaOrderStatuses"]. If Session is null, it sets session by pulling data from Database
        /// </summary>
        /// <returns></returns>
        private List<GDSInstaOrderStatus> GetOrderStatuses()
        {
            if (Session["InstaOrderStatuses"] == null)
            {
                InstadoseOrderRequests request = new InstadoseOrderRequests();

                Session["InstaOrderStatuses"] = request.GetOrderStatuses(true);
            }

            return Session["InstaOrderStatuses"] as List<GDSInstaOrderStatus>;
        }

        private List<GDSInstaOrderStatus> _instaOrderStatuses
        {
            get
            {
                return GetOrderStatuses();
            }
        }

        private Dictionary<string, string> _badgeSummaryOrderStatuses;

        /// <summary>
        /// Class for badge count summary on the page
        /// </summary>
        private class BadgeCountSummary
        {
            public string GDSInstaOrderStatus { get; set; }
            public string OrderStatusName { get; set; }
            
            public int IDCount { get; set; }
            public int IDPlusCount { get; set; }
            public int ID2Count { get; set; }
            public int IDVueCount { get; set; }
            public int IDVueBetaCount { get; set; }
            public int InstaLinkUSBCount { get; set; }
            public int InstaLinkCount { get; set; }
            public int InstaLink3 { get; set; }

            public BadgeCountSummary(string gdsInstaOrderStatus, string statusName, int idBadgeCount, int idPlusBadgeCount, int id2BadgeCount, int idVueBadgeCount, int idVueBetaBadgeCount, int instaLinkUSBCount, int instaLinkCount, int instaLink3)
            {
                GDSInstaOrderStatus = gdsInstaOrderStatus;
                OrderStatusName = statusName;

                IDCount = idBadgeCount;
                IDPlusCount = idPlusBadgeCount;
                ID2Count = id2BadgeCount;
                IDVueCount = idVueBadgeCount;
                IDVueBetaCount = idVueBetaBadgeCount;
                InstaLinkUSBCount = instaLinkUSBCount;
                InstaLinkCount = instaLinkCount;
                InstaLink3 = instaLink3;
            }

            public BadgeCountSummary(string gdsInstaOrderStatus, string statusName, IEnumerable<GDSInstaOrderQueueList> orders)
            {
                GDSInstaOrderStatus = gdsInstaOrderStatus;
                OrderStatusName = statusName;

                if (orders == null)
                {
                    IDCount = 0;
                    IDPlusCount = 0;
                    ID2Count = 0;
                    IDVueCount = 0;
                    IDVueBetaCount = 0;
                    InstaLinkUSBCount = 0;
                    InstaLinkCount = 0;
                    InstaLink3 = 0;
                }
                else
                {
                    IDCount = (int)orders.Sum(o => o.InstadoseCount);
                    IDPlusCount = (int)orders.Sum(o => o.InstadosePlusCount);
                    ID2Count = (int)orders.Sum(o => o.Instadose2Count);
                    IDVueCount = (int)orders.Sum(o => o.Instadose3); ;
                    IDVueBetaCount = (int)orders.Sum(o => o.InstadoseVueBeta); ;
                    InstaLinkUSBCount = (int)orders.Sum(o => o.InstaLinkUSB);
                    InstaLinkCount = (int)orders.Sum(o => o.InstaLink);
                    InstaLink3 = (int)orders.Sum(o => o.InstaLink3);
                }
            }
        }
    }

    public class GDSInstaOrderList : vw_GDSInstaOrder
    {
        public int InstadoseCount
        {
            get
            {
                return (int)InstadoseOne + (int)Instadose2Elite; 
            }
        }

        public int InstadosePlusCount
        {
            get
            {
                return (int)InstadosePlus;
            }
        }

        public int Instadose2Count
        {
            get
            {
                return (int)Instadose2Plus;
            }
        }

        /// <summary>
        /// Set fields and properties from parent class vw_GDSInstaOrder
        /// </summary>
        /// <param name="orders"></param>
        public GDSInstaOrderList(vw_GDSInstaOrder orders)
        {
            foreach (System.Reflection.FieldInfo prop in orders.GetType().GetFields()) 
            {
                GetType().GetField(prop.Name).SetValue(this, prop.GetValue(orders));
            }

            foreach (System.Reflection.PropertyInfo prop in orders.GetType().GetProperties())
            {
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(orders));
            }
        }
    }

    public class GDSInstaOrderQueueList : vw_GDSInstaOrdersQueue
    {
        public int InstadoseCount
        {
            get
            {
                return (int)InstadoseOne + (int)Instadose2Elite;
            }
        }

        public int InstadosePlusCount
        {
            get
            {
                return (int)InstadosePlus;
            }
        }

        public int Instadose2Count
        {
            get
            {
                return (int)Instadose2Plus;
            }
        }

        /// <summary>
        /// Set fields and properties from parent class vw_GDSInstaOrder
        /// </summary>
        /// <param name="orders"></param>
        public GDSInstaOrderQueueList(vw_GDSInstaOrdersQueue orders)
        {
            foreach (System.Reflection.FieldInfo prop in orders.GetType().GetFields())
            {
                GetType().GetField(prop.Name).SetValue(this, prop.GetValue(orders));
            }

            foreach (System.Reflection.PropertyInfo prop in orders.GetType().GetProperties())
            {
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(orders));
            }
        }
    }
}