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
    /// Summary description for RateDetails: Provide backend functions for RateDetails
    /// </summary>
    public class RateDetails
    {
        public RateDetails()
        {
            //
            // TODO: Add constructor logic here
            //
        }



        /// <summary>
        ///  Returns a dataset containing the rate details
        /// </summary>
        /// <param name="pSearchString">Pass the search string</param>
        /// <returns>recordset as dataset</returns>
        public DataSet GetAllRateDetail(string pSearchString)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();

                SqlParameter paramUser1 = new SqlParameter("@SearchString", pSearchString);
                paramUser1.DbType = DbType.String;
                paramUser1.Direction = ParameterDirection.Input;
                parameters.Add(paramUser1);

                DataLayer datalayer = new DataLayer();
                return datalayer.GetDataSet("[dbo].[sp_f_GetAllRateDetails]", CommandType.StoredProcedure, parameters);
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
        /// Performs INSERT, UPDATE and DELETE functions for table
        /// </summary>
        /// <param name="pRateDetailID">RateDetailID</param>
        /// <param name="pRateID">RateID</param>
        /// <param name="pMinQty">MinQty</param>
        /// <param name="pMaxQty">MaxQty</param>
        /// <param name="mode">"INSERT", "UPDATE" or "DELETE"</param>
        /// <returns>boolean</returns>
        public Boolean RateDetailInsertEditDelete(string pRateDetailID, string pRateID, string pMinQty, string pMaxQty, string pMode)
        {
            try
            {
                DataSet myResultDS = new DataSet();

                List<SqlParameter> parameters = new List<SqlParameter>();

                SqlParameter paramUser1 = new SqlParameter("@RateDetailID", pRateDetailID);
                paramUser1.DbType = DbType.Int32;
                paramUser1.Direction = ParameterDirection.Input;
                parameters.Add(paramUser1);

                SqlParameter paramUser2 = new SqlParameter("@RateID", pRateID);
                paramUser2.DbType = DbType.Int32;
                paramUser2.Direction = ParameterDirection.Input;
                parameters.Add(paramUser2);

                SqlParameter paramUser3 = new SqlParameter("@MinQty", pMinQty);
                paramUser3.DbType = DbType.Int32;
                paramUser3.Direction = ParameterDirection.Input;
                parameters.Add(paramUser3);

                SqlParameter paramUser4 = new SqlParameter("@MaxQty", pMaxQty);
                paramUser4.DbType = DbType.Int32;
                paramUser4.Direction = ParameterDirection.Input;
                parameters.Add(paramUser4);

                SqlParameter paramUser5 = new SqlParameter("@Mode", pMode);
                paramUser5.DbType = DbType.String;
                paramUser5.Direction = ParameterDirection.Input;
                parameters.Add(paramUser5);

                DataLayer datalayer = new DataLayer();
                myResultDS = datalayer.GetDataSet("[dbo].[sp_f_RateDetailsInsertEditDelete]", CommandType.StoredProcedure, parameters);

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
