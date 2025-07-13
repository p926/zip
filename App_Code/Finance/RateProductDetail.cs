using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Linq;
using Portal.Instadose;


namespace Portal.Instadose
{
    /// <summary>
    /// Summary description for RateProductDetail: Provide backend functions for RateProductDetail
    /// </summary>
    public class RateProductDetail
    {
        public RateProductDetail()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///  Returns a dataset containing the rate product details
        /// </summary>
        /// <param name="pSearchString">Pass the search string</param>
        /// <returns>recordset as dataset</returns>
        public DataSet GetAllRateProductDetail(string pSearchString)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();

                SqlParameter paramUser1 = new SqlParameter("@SearchString", pSearchString);
                paramUser1.DbType = DbType.String;
                paramUser1.Direction = ParameterDirection.Input;
                parameters.Add(paramUser1);

                DataLayer datalayer = new DataLayer();
                return datalayer.GetDataSet("[dbo].[sp_f_GetAllRateProductDetail]", CommandType.StoredProcedure, parameters);
            }
            catch (System.Exception ex)
            {                
                throw ex;
            }
            
        }

        /// <summary>
        ///  Returns a dataset containing all rate codes
        /// </summary>
        /// <returns>recordset as dataset</returns>
        public DataSet GetAllRateCodes()
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();

                DataLayer datalayer = new DataLayer();
                return datalayer.GetDataSet("[dbo].[sp_f_GetAllRateCodes]", CommandType.StoredProcedure, parameters);
            }
            catch (System.Exception ex)
            {                
                throw ex;
            }
           
        }

        /// <summary>
        ///  Returns a dataset containing all product codes
        /// </summary>
        /// <returns>recordset as dataset</returns>
        public DataSet GetAllProductCodes()
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();

                DataLayer datalayer = new DataLayer();
                return datalayer.GetDataSet("[dbo].[sp_f_GetAllProductCodes]", CommandType.StoredProcedure, parameters);
            }
            catch (System.Exception ex)
            {                
                throw ex;
            }
            
        }

        /// <summary>
        ///  Returns a dataset containing all product codes
        /// </summary>
        /// <returns>recordset as dataset</returns>
        public DataSet GetAllRateDetailsByRateID(int pRateID)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();

                SqlParameter paramUser1 = new SqlParameter("@RateID", pRateID);
                paramUser1.DbType = DbType.Int32;
                paramUser1.Direction = ParameterDirection.Input;
                parameters.Add(paramUser1);

                DataLayer datalayer = new DataLayer();
                return datalayer.GetDataSet("[dbo].[sp_f_GetAllRateDetailsByRateID]", CommandType.StoredProcedure, parameters);
            }
            catch (System.Exception ex)
            {               
                throw ex;
            }
           
        }


        /// <summary>
        /// Performs INSERT, UPDATE and DELETE functions for table
        /// </summary>
        /// <param name="pRateDetailID">RateDetailID</param>
        /// <param name="pProductID">ProductID</param>
        /// <param name="pPrice">Price</param>
        /// <param name="pDiscount">Discount</param>
        /// <param name="mode">"INSERT", "UPDATE" or "DELETE"</param>
        /// <returns>boolean</returns>
        public Boolean  RateProductDetailInsertEditDelete(string pRateDetailID, string pProductID, string pPrice, string pDiscount, string pMode)
        {
            try
            {
                DataSet myResultDS = new DataSet();

                List<SqlParameter> parameters = new List<SqlParameter>();

                SqlParameter paramUser1 = new SqlParameter("@RateDetailID", pRateDetailID);
                paramUser1.DbType = DbType.Int32;
                paramUser1.Direction = ParameterDirection.Input;
                parameters.Add(paramUser1);

                SqlParameter paramUser2 = new SqlParameter("@ProductID", pProductID);
                paramUser2.DbType = DbType.Int32;
                paramUser2.Direction = ParameterDirection.Input;
                parameters.Add(paramUser2);

                SqlParameter paramUser3 = new SqlParameter("@Price", pPrice);
                paramUser3.DbType = DbType.Decimal;
                paramUser3.Direction = ParameterDirection.Input;
                parameters.Add(paramUser3);

                SqlParameter paramUser4 = new SqlParameter("@Discount", pDiscount);
                paramUser4.DbType = DbType.Int16;
                paramUser4.Direction = ParameterDirection.Input;
                parameters.Add(paramUser4);

                SqlParameter paramUser5 = new SqlParameter("@Mode", pMode);
                paramUser5.DbType = DbType.String;
                paramUser5.Direction = ParameterDirection.Input;
                parameters.Add(paramUser5);

                DataLayer datalayer = new DataLayer();
                myResultDS = datalayer.GetDataSet("[dbo].[sp_f_RateProductDetailInsertEditDelete]", CommandType.StoredProcedure, parameters);

                if (myResultDS != null && myResultDS.Tables[0] != null && myResultDS.Tables[0].Rows.Count > 0)
                    if (Convert.ToInt16(myResultDS.Tables[0].Rows[0][0]) == 1)
                        return true;
                    else
                        return false;
                else
                    return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
            
        }

    }
}
