using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using System.Web;
using bomoserv.Common;

public class clsConnectionSQL
{
    public  SqlConnection getConnection()
    {
        return getConnection("");
    }
    public  string getConnectionString(string connectionString)
    {
        string res = "";
        try
        {
            if (connectionString.Trim().Length == 0)
            {
                clsCommon common = new clsCommon();
                res = common.GetConfig("sqlconn");
            }
            else
                return connectionString;

        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("connectionstring", exp.Message, true, exp);
        }
        return res;
    }
    public  SqlConnection getConnection(string connectionString)
    {
        SqlConnection Sq1 = new SqlConnection();
        try
        {
            Sq1 = new SqlConnection(getConnectionString(connectionString));
            if (Sq1.State == 0)
                Sq1.Open();
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Establish Connection with SQL Server", exp.Message, true, exp);
        }
        return Sq1;
    }
    public  void KillCommand(SqlCommand comm)
    {
        try
        {
            if (comm.Connection.State > 0)
                comm.Connection.Close();
            comm.Dispose();
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("KillCommand", exp.Message, true, exp);
        }
    }
    public  DataTable getDataTable(string SQLQuery, string connectionstring)
    {
        DataTable dt = new DataTable();
        SqlCommand comm = new SqlCommand();
        SqlDataAdapter adap = new SqlDataAdapter();
        try
        {
            comm.CommandText = SQLQuery.Trim();
            comm.Connection = getConnection(connectionstring);
            comm.CommandType = CommandType.Text;
            adap.SelectCommand = comm;
            adap.Fill(dt);
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Get Data From SQL Database", exp.Message + "(query:" + SQLQuery + ")", true, exp);
        }
        finally
        {
            if (comm.Connection.State > 0)
                comm.Connection.Close();
            comm.Connection.Dispose();
            comm.Dispose();
            adap.Dispose();
        }
        return dt;
    }
    public  DataTable getDataTable(string SQLQuery)
    {
        return getDataTable(SQLQuery, "");
    }
    public  string ExecuteScalar(string query, SqlParameter[] SqlPar, CommandType comm_type, SqlCommand comm)
    {
        string res = ""; 
        try
        {
            if (comm == null)
            {
                comm = new SqlCommand(query, getConnection(""));
            }
            comm.CommandText = query;
            comm.Parameters.Clear();
            comm.Parameters.AddRange(SqlPar);
            comm.CommandType = comm_type;
            return ExecuteScalar(query, "", comm); 
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Execute Scalar Query in SQL", exp.Message + "(query:" + query + ")", true, exp);
        }
        return res;
    }
     
    public  string ExecuteScalar(string query, string connectionstring)
    {
        string res = "";
        SqlConnection conn = getConnection(connectionstring);
        try
        {
            if (conn.State == 0)
                conn.Open();
            SqlCommand comm = new SqlCommand(query, conn);
            object obj = comm.ExecuteScalar();
            if (conn.State > 0)
                conn.Close();
            conn.Close();
            comm.Dispose();
            if (obj == null)
                return "";
            res = obj.ToString();
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Execute Scalar Query in SQL", exp.Message + "(query:" + query + ")", true, exp);
        }
        return res;
    }
    public  string ExecuteScalar(string query)
    {
        return ExecuteScalar(query, "");
    }
    public  bool ExecuteNonQuery(string query, SqlParameter[] SqlPar, CommandType comm_type, SqlCommand comm)
    {
        bool res = false;
        try
        {
            if (comm == null)
            {
                comm = new SqlCommand(query, getConnection("")); 
            }
            comm.CommandText = query;
            comm.Parameters.Clear();
            comm.Parameters.AddRange(SqlPar);
            comm.CommandType = comm_type;
            return ExecuteNonQuery(query, comm);
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Execute Non Query in SQL with Par", exp.Message + "(query:" + query + ")", true, exp);
        }
        return res;
    }
    public  bool ExecuteNonQuery(string query, string connectionstring)
    {
        bool res = false;
        try
        {
            SqlConnection conn = getConnection(connectionstring);
            SqlCommand comm = new SqlCommand(query, conn);
            int rAff = comm.ExecuteNonQuery();
            res = rAff > 0 ? true : false;
            if (conn.State > 0)
                conn.Close();
            comm.Dispose();
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Execute Non Query in SQL", exp.Message + "(query:" + query + ")", true, exp);
        }
        return res;
    }
    public  bool ExecuteNonQuery(string query)
    {
        return ExecuteNonQuery(query, "");
    }
    public  bool ExecuteNonQuery(string query, SqlParameter[] SqlPar, string connectionstring, CommandType comm_type)
    {
        bool res = false;
        try
        {
            SqlConnection conn = getConnection(connectionstring);
            SqlCommand comm = new SqlCommand(query, conn);
            comm.CommandType = comm_type;
            comm.Parameters.AddRange(SqlPar);
            int rAff = comm.ExecuteNonQuery();
            res = rAff > 0 ? true : false;
            if (conn.State > 0)
                conn.Close();
            comm.Dispose();
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Execute Non Query in SQL with Par", exp.Message + "(query:" + query + ")", true, exp);
        }
        return res;
    }
    public  bool ExecuteNonQuery(string query, SqlParameter[] SqlPar, string connectionstring)
    {
        return ExecuteNonQuery(query, SqlPar, connectionstring, CommandType.Text);
    }
    public  DataTable getDataTable(string SQLQuery, SqlParameter[] SqlPar, string connectionstring, CommandType comm_type)
    {
        DataTable dt = new DataTable();
        SqlCommand comm = new SqlCommand();
        SqlDataAdapter adap = new SqlDataAdapter();
        try
        {
            comm = new SqlCommand(SQLQuery.Trim(), getConnection(connectionstring));
            comm.CommandType = comm_type;
            comm.Parameters.AddRange(SqlPar);
            adap = new SqlDataAdapter(comm);
            adap.Fill(dt);
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Get Data From SQL Database with Par", SQLQuery + "-" + exp.Message + "(query:" + SQLQuery + ")", true, exp);
        }
        finally
        {
            if (comm.Connection.State > 0)
                comm.Connection.Close();
            comm.Connection.Dispose();
            comm.Dispose();
            adap.Dispose();
        }
        return dt;
    }
    public  DataTable getDataTable(string SQLQuery, SqlParameter[] SqlPar, string connectionstring)
    {
        return getDataTable(SQLQuery, SqlPar, connectionstring, CommandType.Text);
    }
    //with trans
    public  SqlCommand GetCommandWithTransaction()
    {
        return GetCommandWithTransaction("");
    }
    public  void Commit(SqlCommand comm)
    {
        // try
        {
            comm.Transaction.Commit();
            if (comm.Connection.State > 0)
                comm.Connection.Close();
            comm.Dispose();
        }
        //  catch (Exception exp)
        {
            //     bomoserv.Common.clsLog.Log(true, true, true, "Commit SQL Query Execution", exp.Message, bomoserv.Common.clsLog.LOG_TYPE.DATABASE_ERROR, "", "", "", exp);
        }
    }
    public  void RollBack(SqlCommand comm)
    {
        try
        {
            if (comm == null)
                return;
            if (comm.Transaction != null)
                comm.Transaction.Rollback();
            else
                return;
            if (comm.Connection.State > 0)
                comm.Connection.Close();
            if (comm != null)
                comm.Dispose();
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Roll Back SQL Query Execution", exp.Message, true, exp);
        }
    }
    public  SqlCommand GetCommandWithTransaction(string connectionstring)
    {
        SqlCommand comm = new SqlCommand();
        try
        {
            comm.Connection = getConnection(connectionstring);
            SqlTransaction trans = comm.Connection.BeginTransaction();
            comm.Transaction = trans;
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Get Command with Transaction in SQL", exp.Message + ", Connection string: " + connectionstring, true, exp);
        }
        return comm;
    }

    public  bool ExecuteNonQuery(string query, SqlCommand comm)
    {
        return ExecuteNonQuery(query, "", comm);
    }
    public  bool ExecuteNonQuery(string query, string connectionstring, SqlCommand comm)
    {
        bool res = false;
        bool is_new_command = false;
        try
        {
            if (comm == null)
            {
                comm = new SqlCommand(query, getConnection(connectionstring));
                is_new_command = true;
            }
            else
                comm.CommandText = query;
            int rAff = comm.ExecuteNonQuery();
            res = rAff > 0 ? true : false;
            if (is_new_command)
            {
                if (comm.Connection.State > 0)
                    comm.Connection.Close();
                comm.Dispose();
            }
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Execute Non Query in SQL with Command Par", exp.Message + "(query:" + query + ")", true, exp);
        }
        return res;
    }
    public  DataTable getDataTable(string SQLQuery, SqlCommand comm)
    {
        return getDataTable(SQLQuery, "", comm);
    }
    public  DataTable getDataTable(string SQLQuery, string connectionstring, SqlCommand comm)
    {
        DataTable dt = new DataTable();
        SqlDataAdapter adap = new SqlDataAdapter();
        bool is_new_command = false;
        try
        {
            comm.CommandText = SQLQuery.Trim();
            if (comm == null)
            {
                comm.Connection = getConnection(connectionstring);
                is_new_command = true;
            }
            comm.CommandType = CommandType.Text;
            adap.SelectCommand = comm;
            adap.Fill(dt);
        }
        catch (Exception exp)
        {
            bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
            clslog.Log("Get Data From SQL Database with Command as Par", exp.Message + "(query:" + SQLQuery + ")", true, exp);
        }
        finally
        {
            if (is_new_command)
            {
                if (comm.Connection.State > 0)
                    comm.Connection.Close();
                comm.Connection.Dispose();
                comm.Dispose();
                adap.Dispose();
            }
        }
        return dt;
    }
    public  string ExecuteScalar(string query, SqlCommand comm)
    {
        return ExecuteScalar(query, "", comm);
    }
    public  string ExecuteScalar(string query, string connectionstring, SqlCommand comm)
    {
        bool is_new_command = false;
        if (comm == null)
        {
            is_new_command = true;
            comm = new SqlCommand(query, getConnection(connectionstring));
        }
        else
            comm.CommandText = query;
        object obj = comm.ExecuteScalar();
        if (is_new_command)
        {
            if (comm.Connection.State > 0)
                comm.Connection.Close();
            comm.Connection.Close();
            comm.Dispose();
        }
        if (obj == null)
            return "";
        return obj.ToString();
    }
}
