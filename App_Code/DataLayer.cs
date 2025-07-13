using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;

namespace Portal.Instadose
{

    /// <summary>
    /// Summary description for DataLayer: Provide backend functions shared across the application
    /// </summary>
    public class DataLayer
    {
        public DataLayer()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Method to instantiate database sql related objects

        /// <summary>
        /// This method returns the Connection object to be assigned to a database command object
        /// </summary>
        /// <returns></returns>
        private SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString());
            return conn;
        }
        /// <summary>
        /// This method returns the command object for a particular database operation
        /// </summary>
        /// <param name="dbCommandType"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private SqlCommand GetCommand(CommandType dbCommandType, string commandText, List<SqlParameter> parameters)
        {
            SqlCommand dbcmd = null;
            try
            {
                dbcmd = new SqlCommand(commandText, GetConnection());
                dbcmd.CommandType = dbCommandType;
                dbcmd.CommandTimeout = 120;
                if (parameters != null)
                {
                    if (parameters.Count > 0)
                    {
                        foreach (SqlParameter param in parameters)
                        {
                            dbcmd.Parameters.Add(param);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (dbcmd.Connection.State == ConnectionState.Open)
                {
                    dbcmd.Connection.Close();
                }

                dbcmd.Dispose();
                return null;
            }


            return dbcmd;
        }
        /// <summary>
        /// This method when passed the commandtext,parameters will return the resultset as a dataset.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dbCommandType"></param>
        /// <param name="parameters"></param>
        /// <param name="outputparametername"></param>
        /// <param name="outputvalue"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string commandText, CommandType dbCommandType, List<SqlParameter> parameters, string outputparametername, out string outputvalue)
        {
            SqlDataAdapter dbadapter = null;
            try
            {
                dbadapter = new SqlDataAdapter(GetCommand(dbCommandType, commandText, parameters));
                DataSet dbDataSet = new DataSet();
                dbadapter.Fill(dbDataSet);
                outputvalue = "";
                if (outputparametername != null)
                {
                    outputvalue = dbadapter.SelectCommand.Parameters[outputparametername].Value.ToString();
                }
                dbadapter.Dispose();
                return dbDataSet;
            }
            catch (Exception ex)
            {
                dbadapter.Dispose();
                throw ex;
            }
        }


        public DataSet GetDataSet(string commandText, CommandType dbCommandType, List<SqlParameter> parameters)
        {
            SqlDataAdapter dbadapter = null;
            try
            {
                dbadapter = new SqlDataAdapter(GetCommand(dbCommandType, commandText, parameters));
                DataSet dbDataSet = new DataSet();
                dbadapter.Fill(dbDataSet);
                dbadapter.Dispose();
                return dbDataSet;
            }
            catch (Exception ex)
            {
                dbadapter.Dispose();
                throw ex;
            }
        }



        public int ExecuteNonQuery(string commandText, CommandType dbCommandType, List<SqlParameter> parameters, string outputparametername, out string outputvalue)
        {
            SqlCommand cmd = null;
            int i = 0;
            try
            {
                cmd = GetCommand(dbCommandType, commandText, parameters);
                outputvalue = "";
                cmd.Connection.Open();
                i = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (outputparametername != null)
                {
                    outputvalue = cmd.Parameters[outputparametername].Value.ToString();
                }

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
                cmd.Dispose();
                i = -1;
                throw ex;
            }
            return i;
        }


        public int ExecuteNonQuery(string commandText, CommandType dbCommandType, List<SqlParameter> parameters)
        {
            SqlCommand cmd = null;
            int i = 0;
            try
            {
                cmd = GetCommand(dbCommandType, commandText, parameters);
                cmd.Connection.Open();
                i = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
                cmd.Dispose();
                i = -1;
                throw ex;
            }
            return i;
        }

        #endregion


    }

}
