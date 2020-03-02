using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Data.SqlClient;

/// <summary>
/// Summary description for DBSql  ..test
/// </summary>
public class DBSql
{
    private string strConnection = "connection string needed";
    private SqlConnection myConnection;
    private bool bConnected;
    private string strError = "";

    public DBSql()
    {
        strConnection = "";

        bConnected = false;
        strError = "";
    }

    public bool OpenMyConnection()
    {
        strError = "";
        if (strConnection == "")
        {
            return false;
        }
        if (!bConnected)
        {
            try
            {
                myConnection = new SqlConnection(strConnection);
                if (myConnection.State == ConnectionState.Closed)
                    myConnection.Open();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                if (myConnection != null)
                    myConnection.Dispose();
            }
        }
        return bConnected;
    }
    public void CloseMyConnection()
    {
        bConnected = false;
        try
        {
            myConnection.Close();
            if (myConnection != null)
                myConnection.Dispose();
        }
        catch (Exception ex)
        {
            strError = ex.Message;
            myConnection.Close();
        }
    }
    public SqlDataReader GetDataReader(string mySelectQuery)
    {
        SqlDataReader myReader;
        OpenMyConnection();

        try
        {
            SqlCommand myCommand = new SqlCommand(mySelectQuery, myConnection);
            myReader = myCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
        catch (Exception ex)
        {
            strError = ex.Message;
            return null;
        }
        finally
        {
            //				if (myCommand != null)
            //					myCommand.Dispose();
        }
        return myReader;
    }
    public DataSet GetDataSet(string mySelectQuery)
    {
        DataSet ds = new DataSet();
        OpenMyConnection();
        try
        {
            SqlCommand myCommand = new SqlCommand(mySelectQuery, myConnection);
            SqlDataAdapter sda = new SqlDataAdapter(myCommand);
            sda.Fill(ds, mySelectQuery);
            CloseMyConnection();
            return ds;

        }
        catch (Exception ex)
        {
            strError = ex.Message;
            return null;
        }
        finally
        {
        }
    }
    public double ExecuteNonQuery(string CmdText)
    {
        //Executes an SQL statement against the Connection and returns the number of rows affected.
        SqlCommand objCommand;
        OpenMyConnection();
        int resp = 0;
        try
        {
            objCommand = new SqlCommand(CmdText, myConnection);
            resp = objCommand.ExecuteNonQuery();
            CloseMyConnection();
            return resp;
        }
        catch (Exception ex)
        {
            strError = ex.Message;
            return 0;
        }
        finally
        {

        }
    }
    public double ExecuteScalar(string CmdText)
    {
        //Executes an SQL statement against the Connection and returns the number of rows affected.
        SqlCommand objCommand;
        OpenMyConnection();
        double resp = 0;
        try
        {
            objCommand = new SqlCommand(CmdText, myConnection);
            resp = Convert.ToDouble(objCommand.ExecuteScalar());
            CloseMyConnection();
            return resp;
        }
        catch (Exception ex)
        {
            strError = ex.Message;
            return 0;
        }
        finally
        {

        }
    }

}
