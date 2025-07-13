using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using Instadose;

namespace portal_instadose_com_v3.Manufacturing
{
    public partial class ConvertProductSKU : System.Web.UI.Page
    {
        public int orderID = 0;

        InsDataContext idc = new InsDataContext();

        #region ON PAGE LOAD.
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack) return;

            InvisibleErrorMessage();
            InvisibleSuccessMessage();

            this.txtOrderNumber.Text = string.Empty;
        }
        #endregion

        #region IS ORDERID VALID?
        private bool IsOrderIDValid(int orderid)
        {
            int[] validStatus = { 1, 2, 3, 6 };

            var getOrderRecord = idc.Orders.Where(o => o.OrderID == orderid && o.OrderStatusID != 10).FirstOrDefault();

            return validStatus.Contains(getOrderRecord.OrderStatusID);
        }
        #endregion

        #region DISPLAY ORDER DETAILS.
        protected void btnFindOrder_Click(object sender, EventArgs e)
        {
            // Reset Error/Success Messages.
            InvisibleErrorMessage();
            InvisibleSuccessMessage();

            int.TryParse(this.txtOrderNumber.Text, out orderID);

            if (IsOrderIDValid(orderID) == true)
            {
                // These are the current NEW ProductSKU's for Instadose 1.
                // Please add to this STRING ARRAY as necessary for future additions as needed.
                string[] productSKUs = { "INSTA10", "INSTA10-B", "CASE CVR BLK", "CASE CVR BLUE", "CASE CVR GREEN", "CASE CVR PINK", "CASE CVR SLVR" };

                // Looks to see if the Order Details of a given OrderID has items listed for Instadose 1 IMI.
                // IF TRUE then IsConverted = TRUE
                // ELSE IsConverted = FALSE
                int getRecordCount = (from od in idc.OrderDetails
                                      join p in idc.Products on od.ProductID equals p.ProductID
                                      where od.OrderID == orderID
                                      && productSKUs.Contains(p.ProductSKU)
                                      select od.OrderID).Count();

                if (getRecordCount >= 1)
                {
                    GetOrderDetails(orderID, true);
                    this.btnConvertProductSKUs.Enabled = false;
                    VisibleErrorMessage("Product SKUs have alerady been converted.");
                }
                else
                {
                    GetOrderDetails(orderID, false);
                    this.btnConvertProductSKUs.Enabled = true;
                }

                this.divOrderDetails.Visible = true;
            }
            else
            {
                VisibleErrorMessage("Order # entered does not exist in our records OR Order SKU's cannot be changed due to status.");
                return;
            }
        }
        #endregion

        #region FUNCTION -> GET ORDER DETAILS.
        private void GetOrderDetails(int orderid, bool isconverted)
        {
            decimal unitPrice = 0;
            int itemQuantity = 0;
            decimal subtotal = 0;
            decimal total = 0;

            string currencyCode = "";

            // Create the SQL Connection from the Connection String in the web.config file.
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add(new SqlParameter("@OrderID", orderid));
            sqlCmd.Parameters.Add(new SqlParameter("@IsConverted", isconverted));

            // Pass the query string to the command

            sqlCmd.Connection = sqlConn;

            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();

            sqlCmd.CommandText = "sp_GetConvertedProductSKUOrderDetails";

            SqlDataReader sqlDtRdr = sqlCmd.ExecuteReader();

            DataTable dtOrderDetails = new DataTable();

            // Create the Order Details DataTable to hold the contents of the order.
            dtOrderDetails = new DataTable("Order Details");

            // Create the Columns for the Order Details DataTable.
            dtOrderDetails.Columns.Add("AccountID", Type.GetType("System.Int32"));
            dtOrderDetails.Columns.Add("OrderID", Type.GetType("System.Int32"));
            dtOrderDetails.Columns.Add("AccountName", Type.GetType("System.String"));
            dtOrderDetails.Columns.Add("Status", Type.GetType("System.String"));
            dtOrderDetails.Columns.Add("OrderDate", Type.GetType("System.DateTime"));
            dtOrderDetails.Columns.Add("CurrencyCode", Type.GetType("System.String"));
            dtOrderDetails.Columns.Add("OrderDetailID", Type.GetType("System.Int32"));
            dtOrderDetails.Columns.Add("ProductSKU", Type.GetType("System.String"));
            dtOrderDetails.Columns.Add("ProductName", Type.GetType("System.String"));
            dtOrderDetails.Columns.Add("UnitPrice", Type.GetType("System.Decimal"));
            dtOrderDetails.Columns.Add("ItemQuantity", Type.GetType("System.Int32"));
            dtOrderDetails.Columns.Add("SubTotal", Type.GetType("System.Decimal"));

            while (sqlDtRdr.Read())
            {
                this.lblOrderID.Text = sqlDtRdr["OrderID"].ToString();
                this.hyprlnkAccountName.Text = sqlDtRdr["AccountName"].ToString();
                string accountID = sqlDtRdr["AccountID"].ToString();
                this.hyprlnkAccountName.NavigateUrl = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}#Order_tab", accountID);
                this.lblOrderDate.Text = string.Format("{0}", sqlDtRdr["OrderDate"].ToString());
                this.lblStatus.Text = sqlDtRdr["Status"].ToString();
                currencyCode = sqlDtRdr["CurrencyCode"].ToString();
                this.lblCurrencyCode.Text = currencyCode;

                // Create a NEW Order Details Row.
                DataRow dr = dtOrderDetails.NewRow();

                // Fill the Row Details.
                dr["OrderDetailID"] = sqlDtRdr["OrderDetailID"].ToString();
                dr["ProductSKU"] = sqlDtRdr["ProductSKU"].ToString();
                dr["ProductName"] = sqlDtRdr["ProductName"].ToString();

                // Get UnitPrice.
                decimal.TryParse(sqlDtRdr["UnitPrice"].ToString(), out unitPrice);

                // Get ItemQuantity.
                int.TryParse(sqlDtRdr["ItemQuantity"].ToString(), out itemQuantity);

                // Get Sub-Total.
                subtotal = unitPrice * itemQuantity;

                // Get Total Balance (Example: 10.00 USD will become 10.00)
                total += FormatMoneyValues(subtotal.ToString());

                // Display UnitPrice, Quantity, and Sub-Total.
                dr["UnitPrice"] = unitPrice.ToString();
                dr["ItemQuantity"] = itemQuantity.ToString();
                dr["SubTotal"] = subtotal.ToString();

                // ADD Row to the DataTable.
                dtOrderDetails.Rows.Add(dr);
            }

            string completeDisplayOfTotal = String.Format("{0:#,0.00}", total) + " " + currencyCode;
            this.lblTotal.Text = completeDisplayOfTotal;

            this.gvOrderDetails.DataSource = dtOrderDetails;
            this.gvOrderDetails.DataBind();
        }
        #endregion

        #region CURRENCY FORMATTING.
        /// <summary>
        /// Example: string balance = 3.00 AUD, 3.11 USD. The function will return 3.00
        /// </summary>
        /// <param name="balance"></param>
        /// <returns></returns>
        private decimal FormatMoneyValues(string balance)
        {
            try
            {
                string balance1 = balance.Split(',')[0];
                string balance2 = balance1.Split(' ')[0];
                return decimal.Parse(balance2);
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        #region CONVERT PRODUCT SKUS.
        protected void btnConvertProductSKUs_Click(object sender, EventArgs e)
        {
            int.TryParse(this.txtOrderNumber.Text, out orderID);
            UpdateOrderSKUs(orderID);
        }
        #endregion

        #region FUNCTION -> UPDATE PRODUCT SKUS.
        private void UpdateOrderSKUs(int orderid)
        {
            int totalQuantity = 0;
            decimal unitPrice = 0;
            int productID = 0;

            OrderDetail orderDetailColorSKUs = null;

            // Modified 09/08/2015 by Anuradha Nandi
            // INSTA10 is no longer in stock, change to INSTA10-B going forward.
            //int insta10ProductID = (from p in idc.Products
            //                        where p.ProductSKU == "INSTA10"
            //                        select p.ProductID).FirstOrDefault();
            int insta10BProductID = (from p in idc.Products
                                    where p.ProductSKU == "INSTA10-B"
                                    select p.ProductID).FirstOrDefault();

            // Loop through the GridView to find the Quantity of each BadgeType/BadgeSKU.
            foreach (GridViewRow rowitem in this.gvOrderDetails.Rows)
            {
                int orderDetailID = 0;
                int.TryParse(this.gvOrderDetails.DataKeys[rowitem.RowIndex].Value.ToString(), out orderDetailID);

                // Get the ProductSKU as STRING Value.
                string productSKU = rowitem.Cells[0].Text;

                // Get Total Item Quantity (for a given ProductSKU).
                int itemQuantity = 0;
                int.TryParse(rowitem.Cells[3].Text, out itemQuantity);

                if (productSKU == "/LOST BADGE")
                {
                    totalQuantity += 0;
                }
                else
                {
                    totalQuantity += itemQuantity;
                }

                // Get Unit Price for EACH ProductSKUs.
                decimal.TryParse(rowitem.Cells[2].Text, out unitPrice);

                // Get NEW ProductSKUs.
                string oldProductSKU = rowitem.Cells[0].Text;
                string newProductSKU = "";

                switch (oldProductSKU)
                {
                    case "INSTA10BLU":
                        newProductSKU = "CASE CVR BLUE";
                        break;
                    case "INSTA10BLK":
                        newProductSKU = "CASE CVR BLK";
                        break;
                    case "INSTA10GRN":
                        newProductSKU = "CASE CVR GREEN";
                        break;
                    case "INSTA10PNK":
                        newProductSKU = "CASE CVR PINK";
                        break;
                    case "INSTA10SLV":
                        newProductSKU = "CASE CVR SLVR";
                        break;
                    case "/LOST BADGE":
                        newProductSKU = "/LOST BADGE";
                        break;
                    default:
                        newProductSKU = "";
                        break;
                }

                if (newProductSKU == "") return;

                // Get ProductID of the NEW ProductSKU.
                productID = (from p in idc.Products
                             where p.ProductSKU == newProductSKU
                             select p.ProductID).FirstOrDefault();

                if (newProductSKU == "/LOST BADGE" || productID == 11)
                {
                    // CREATE a OrderDetails Record for /LOST BADGE.
                    orderDetailColorSKUs = new OrderDetail
                    {
                        OrderID = orderID,
                        Price = unitPrice,
                        Quantity = itemQuantity,
                        OrderDetailDate = DateTime.Now,
                        ProductID = productID,
                        LabelID = null,
                        OrderDevInitialize = false,
                        OrderDevAssign = false
                    };
                }
                else
                {
                    // CREATE a OrderDetails Record ALL other ProductSKU's.
                    orderDetailColorSKUs = new OrderDetail
                    {
                        OrderID = orderID,
                        Price = 0,
                        Quantity = itemQuantity,
                        OrderDetailDate = DateTime.Now,
                        ProductID = productID,
                        LabelID = null,
                        OrderDevInitialize = false,
                        OrderDevAssign = false
                    };
                }

                idc.OrderDetails.InsertOnSubmit(orderDetailColorSKUs);

                // Delete previous Order Detail record.
                DeletePreviousOrderDetails(orderDetailID);
            }

            // Get the NEW OrderDetailID.
            int newOrderDetailID = orderDetailColorSKUs.OrderID;

            // Check to see if there are any /LOST BADGE records.
            int countLostBadgeRecords = (from od in idc.OrderDetails
                                         where od.OrderID == newOrderDetailID
                                         && od.ProductID == 11
                                         select od).Count();

            //OrderDetail orderDetailsTotalInsta10 = null;
            OrderDetail orderDetailsTotalInsta10B = null;

            // CREATE another OrderDetails Record for the Order's Total Quantity and Total Price for Instadose IMI (INSTA10-B).
            if (countLostBadgeRecords >= 1)
            {
                //orderDetailsTotalInsta10 = new OrderDetail
                orderDetailsTotalInsta10B = new OrderDetail
                {
                    OrderID = orderID,
                    Price = 0,
                    Quantity = totalQuantity,
                    OrderDetailDate = DateTime.Now,
                    ProductID = insta10BProductID,
                    //ProductID = insta10ProductID,
                    LabelID = null,
                    OrderDevInitialize = false,
                    OrderDevAssign = false
                };
            }
            else
            {
                //orderDetailsTotalInsta10 = new OrderDetail
                orderDetailsTotalInsta10B = new OrderDetail
                {
                    OrderID = orderID,
                    Price = unitPrice,
                    Quantity = totalQuantity,
                    OrderDetailDate = DateTime.Now,
                    ProductID = insta10BProductID,
                    //ProductID = insta10ProductID,
                    LabelID = null,
                    OrderDevInitialize = false,
                    OrderDevAssign = false
                };
            }

            //idc.OrderDetails.InsertOnSubmit(orderDetailsTotalInsta10);
            idc.OrderDetails.InsertOnSubmit(orderDetailsTotalInsta10B);

            // SUBMIT the changes to the DataBase.
            try
            {
                idc.SubmitChanges();

                // SEND an Order to MAS.
                if (!Instadose.Integration.MAS.SendOrderToMAS(orderID, ActiveDirectoryQueries.GetUserName()))
                    throw new Exception("The order has been created, but the records could not be sent to MAS. Please contact IT for assistance.");

                GetOrderDetails(orderID, true);

                string successMessage = "Product SKU's were updated successfully!";
                VisibleSuccessMessage(successMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.ToString();
                VisibleErrorMessage(errorMessage);
                return;
            }
        }
        #endregion

        #region DELETE ORDER DETAIL RECORDS WHERE OLD PRODUCT SKU IS USED.
        private void DeletePreviousOrderDetails(int orderdetailid)
        {
            // Get Old Order Details Records.
            var deleteThisRecord = (from od in idc.OrderDetails
                                    where od.OrderDetailID == orderdetailid
                                    select od).FirstOrDefault();

            if (deleteThisRecord == null) return;

            idc.OrderDetails.DeleteOnSubmit(deleteThisRecord);
            idc.SubmitChanges();
        }
        #endregion

        #region ERROR/SUCCESS MESSAGES.
        private void InvisibleErrorMessage()
        {
            this.spnFormErrorMessage.InnerHtml = "";
            this.divFormError.Visible = false;
        }

        private void VisibleErrorMessage(string error)
        {
            this.spnFormErrorMessage.InnerHtml = error;
            this.divFormError.Visible = true;
        }

        private void InvisibleSuccessMessage()
        {
            this.spnFormSuccessMessage.InnerHtml = "";
            this.divFormSuccess.Visible = false;
        }

        private void VisibleSuccessMessage(string success)
        {
            this.spnFormSuccessMessage.InnerHtml = success;
            this.divFormSuccess.Visible = true;
        }
        #endregion
    }
}