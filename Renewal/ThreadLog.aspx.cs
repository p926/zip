using Instadose.Data;
using Instadose.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace portal_instadose_com_v3.Finance.Renewal
{
	public partial class ThreadLog : System.Web.UI.Page
	{
		private InsDataContext _idc;

		#region Events
		protected void Page_Load(object sender, EventArgs e)
		{
			_idc = new InsDataContext();

            hdnUserName.Value = ActiveDirectoryQueries.GetUserName();
			string renewalNo = Request.QueryString["no"];

			if (!IsPostBack)
			{
				Session["sortDirection"] = SortDirection.Ascending;

				if (!string.IsNullOrEmpty(renewalNo))
				{
					txtRenewalNumber.Text = renewalNo;
					gvThreadLogs.DataSource = this.GetRenewalThreadLogs();
					gvThreadLogs.DataBind();
				}
			}
		}

		protected void lnkbtnDisplay_Click(object sender, EventArgs e)
		{
			gvThreadLogs.PageIndex = 0;
			gvThreadLogs.DataSource = this.GetRenewalThreadLogs();
			gvThreadLogs.DataBind();
		}

		protected void lnkClear_Click(object sender, EventArgs e)
		{
			this.ClearFilterInputs();

			gvThreadLogs.DataSource = new List<RenewalThreadLog>();
			gvThreadLogs.DataBind();
		}

		protected void gvThreadLogs_Sorting(object sender, GridViewSortEventArgs e)
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

			List<RenewalThreadLog> exceptions = this.GetRenewalThreadLogs();

			if (exceptions != null && exceptions.Count > 0)
			{
				var pInfo = typeof(RenewalThreadLog).GetProperty(e.SortExpression);

				if (e.SortDirection == SortDirection.Ascending)
					exceptions = exceptions.OrderBy(r => pInfo.GetValue(r, null)).ToList();
				else
					exceptions = exceptions.OrderByDescending(r => pInfo.GetValue(r, null)).ToList();
			}

			gvThreadLogs.PageIndex = 0;
			gvThreadLogs.DataSource = exceptions;
			gvThreadLogs.DataBind();
		}

		protected void gvThreadLogs_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			gvThreadLogs.PageIndex = e.NewPageIndex;
			gvThreadLogs.DataSource = this.GetRenewalThreadLogs();
			gvThreadLogs.DataBind();
		}

		protected void gvThreadLogs_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				RenewalThreadLog log = e.Row.DataItem as RenewalThreadLog;

				if (log != null)
				{
					LinkButton btnRenewalNo = e.Row.FindControl("btnRenewalNo") as LinkButton;
					Image imgComplete = e.Row.FindControl("imgComplete") as Image;
					Literal ltTotalCount = e.Row.FindControl("ltTotalCount") as Literal;
					Literal ltProcessedCount = e.Row.FindControl("ltProcessedCount") as Literal;
					Literal ltWaitingCount = e.Row.FindControl("ltWaitingCount") as Literal;

					btnRenewalNo.Text = log.RenewalNo;
					btnRenewalNo.CommandArgument = log.RenewalThreadLogID.ToString() + ":" + log.RenewalNo;
					
					int totalCnt = log.RenewalThreadLogDetails.Count();
					int processedCnt = log.RenewalThreadLogDetails.Where(ld => ld.RenewalLogs != null).Count();
					int waitingCnt = log.RenewalThreadLogDetails.Where(ld => ld.RenewalLogs == null).Count();

					ltTotalCount.Text = totalCnt.ToString();
					ltProcessedCount.Text = processedCnt.ToString();
					ltWaitingCount.Text = waitingCnt.ToString();

					if (!string.IsNullOrEmpty(log.ErrorMessage))
					{
						imgComplete.ToolTip = "Error Occured";
						imgComplete.ImageUrl = "/images/icons/bullet_error.png";
					}
					else
					{
						if (log.Complete)
						{
							imgComplete.ToolTip = "Complete";
							imgComplete.ImageUrl = "/images/icons/accept.png";
						}
						else
						{
							imgComplete.ToolTip = "Processing";
							imgComplete.ImageUrl = "/images/icons/hourglass.png";
						}
					}
				}
			}
		}

		protected void gvThreadLogs_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "OpenRenewalThreadLogDetail")
			{
				int renewalThreadLogID = 0;
				string[] arguments = e.CommandArgument.ToString().Split(':');

				if (!int.TryParse(arguments[0], out renewalThreadLogID))
					return;

				RenewalThreadLog log = _idc.RenewalThreadLogs.SingleOrDefault(l => l.RenewalThreadLogID == renewalThreadLogID);

				if (log == null)
					return;

				bool isReProcessable = true;
				// check time elapsed
				TimeSpan duration = DateTime.Now.Subtract(log.StartDate);

				// renewal request cannot be reprocessed within 20 minutes.
				if (duration.TotalMinutes < 20)
					isReProcessable = false;
				else
					isReProcessable = log.RenewalThreadLogDetails.Count(ld => ld.RenewalLogs == null) > 0;
				
				hdnThreadLogDetailDialog_ThreadLogID.Value = renewalThreadLogID.ToString();

				gvThreadLogDetails.PageIndex = 0;
				gvThreadLogDetails.DataSource = log.RenewalThreadLogDetails.OrderBy(ld => ld.AccountID).ToList();
				gvThreadLogDetails.DataBind();

				string jscript = "$(function () { openRenewalthreadDetailDialog('" + arguments[1] + "', " + isReProcessable.ToString().ToLower() + "); });";

				ScriptManager.RegisterClientScriptBlock(this, this.GetType(), e.CommandName + "_" + e.CommandArgument, jscript, true);
			}
		}

		protected void gvThreadLogDetails_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			int threadLogID = 0;

			if (int.TryParse(hdnThreadLogDetailDialog_ThreadLogID.Value, out threadLogID))
			{
				gvThreadLogDetails.PageIndex = e.NewPageIndex;
				gvThreadLogDetails.DataSource = this.GetRenewalThreadLogDetails(threadLogID);
				gvThreadLogDetails.DataBind();
			}
		}

		protected void gvThreadLogDetails_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				RenewalThreadLogDetail logDetail = e.Row.DataItem as RenewalThreadLogDetail;

				if (logDetail != null)
				{
					Image imgComplete = e.Row.FindControl("imgComplete") as Image;
					Literal ltOrderID = e.Row.FindControl("ltOrderID") as Literal;
					Literal ltAccountName = e.Row.FindControl("ltAccountName") as Literal;
					Literal ltOrderStatus = e.Row.FindControl("ltOrderStatus") as Literal;
					Literal ltBrandName = e.Row.FindControl("ltBrandName") as Literal;
					Literal ltBillingTermDesc = e.Row.FindControl("ltBillingTermDesc") as Literal;
					Literal ltPaymentType = e.Row.FindControl("ltPaymentType") as Literal;
					Literal ltRenewalAmount = e.Row.FindControl("ltRenewalAmount") as Literal;

					if (logDetail.RenewalLogs != null)
					{
                        if (string.IsNullOrEmpty(logDetail.RenewalLogs.ErrorMessage))
                        {
                            imgComplete.ToolTip = "Processed";
                            imgComplete.ImageUrl = "/images/icons/accept.png";
                        }
                        else
                        {
                            imgComplete.ToolTip = "Error";
                            imgComplete.ImageUrl = "/images/icons/bullet_error.png";
                        }
					}
					else
					{
						if (logDetail.RenewalThreadLog.Complete)
						{
							imgComplete.ToolTip = "Not Processed";
							imgComplete.ImageUrl = "/images/icons/cross.png";
						}
						else
						{
							imgComplete.ToolTip = "Processing";
							imgComplete.ImageUrl = "/images/icons/hourglass.png";
						}
					}

					ltAccountName.Text = logDetail.Account.AccountName;

					if (logDetail.RenewalLogs != null)
					{
						ltOrderID.Text = logDetail.RenewalLogs.OrderID.ToString();
						ltOrderStatus.Text = logDetail.RenewalLogs.OrderStatus == null ? "" : logDetail.RenewalLogs.OrderStatus.OrderStatusName;
						ltBrandName.Text = logDetail.RenewalLogs.BrandSource.BrandName;
						ltBillingTermDesc.Text = logDetail.RenewalLogs.BillingTerm == null ? "" : logDetail.RenewalLogs.BillingTerm.BillingTermDesc;
						ltPaymentType.Text = logDetail.RenewalLogs.PaymentMethod == null ? "" : logDetail.RenewalLogs.PaymentMethod.PaymentMethodName;
						ltRenewalAmount.Text = logDetail.RenewalLogs.RenewalAmount.ToString("N2");
					}
				}
			}
		}
		#endregion

		#region Helper
		private List<RenewalThreadLog> GetRenewalThreadLogs()
		{
			IQueryable<RenewalThreadLog> log = _idc.RenewalThreadLogs;

			if (!string.IsNullOrEmpty(txtPeriodFrom.Text))
			{
				DateTime fromDate;
				if (DateTime.TryParse(txtPeriodFrom.Text, out fromDate))
					log = log.Where(l => l.StartDate >= new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0));
			}

			if (!string.IsNullOrEmpty(txtPeriodTo.Text))
			{
				DateTime toDate;
				if (DateTime.TryParse(txtPeriodTo.Text, out toDate))
					log = log.Where(l => l.StartDate <= new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59));
			}

			if (!string.IsNullOrEmpty(txtRenewalNumber.Text))
				log = log.Where(l => l.RenewalNo.ToLower() == txtRenewalNumber.Text.ToLower());

			return log.OrderBy(l => l.StartDate).ToList();
		}

		private List<RenewalThreadLogDetail> GetRenewalThreadLogDetails(int renewalThreadLogID)
		{
			return _idc.RenewalThreadLogDetails.Where(ld => ld.RenewalThreadLogID == renewalThreadLogID).OrderBy(ld => ld.AccountID).ToList();
		}

		private void ClearFilterInputs()
		{
			txtPeriodFrom.Text = string.Empty;
			txtPeriodTo.Text = string.Empty;
			txtRenewalNumber.Text = string.Empty;
		}
		#endregion
	}
}