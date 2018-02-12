using System;
using Microsoft.Deployment.WindowsInstaller;
using System.Data.SqlClient;
namespace WiXDemoWebApp.CustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult SaveUserInfo(Session session)
        {
            session.Log("Begin Saving User Information");

            try
            {
                //string serverName = session["SQL_SERVERNAME"];
                //string databaseName = session["SQL_DATABASE"];
                //string userID = session["SQL_USERNAME"];
                //string password = session["SQL_PASSWORD"];
                string userName = session["USER_NAME"];
                string email = session["EMAIL"];
                string contactNumber = session["CONTACT_NUMBER"];

                //bool isSqlAuth = session["IS_SQLAUTH"] == "1";

                //string connectionString = string.Empty;

                //if (isSqlAuth)
                //{
                //    connectionString = $"Server={serverName};Initial Catalog={databaseName};User ID={userID};Password={password};Integrated Security=false;Persist Security Info=false;";
                //}
                //else
                //{
                //    connectionString = $"Server={serverName};Initial Catalog={databaseName};Integrated Security=true;Persist Security Info=false;";
                //}
                string connectionString = session["CONNECTION_STRING"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"INSERT INTO dbo.WiXUserInfo VALUES('{userName}','{email}','{contactNumber}')";
                    var command = new SqlCommand(query, connection);
                    var rowsInserted = command.ExecuteNonQuery();
                    if (rowsInserted != 0)
                    {
                        session["IS_USER_CREATED"] = "1";
                    }
                }
            }
            catch (Exception ex)
            {
                session["IS_USER_CREATED"] = "0";
                session["ERROR_MESSAGE"] = $"Error occured while saving user information. Please contact administrator.{ex.ToString()}";
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult ExecuteInitialDbSetupSP(Session session)
        {
            session.Log("Begin Connection to SQL Server");
            int rowsAffected = 0;
            try
            {
                //string serverName = session["SQL_SERVERNAME"];
                //string databaseName = session["SQL_DATABASE"];
                //string userID = session["SQL_USERNAME"];
                //string password = session["SQL_PASSWORD"];
                //bool isSqlAuth = session["IS_SQLAUTH"] == "1";

                //string connectionString = string.Empty;


                //if (isSqlAuth)
                //{
                //    connectionString = $"Server={serverName};Initial Catalog={databaseName};User ID={userID};Password={password};Integrated Security=false;Persist Security Info=false;";
                //}
                //else
                //{
                //    connectionString = $"Server={serverName};Initial Catalog={databaseName};Integrated Security=true;Persist Security Info=false;";
                //}

                string connectionString = session["CONNECTION_STRING"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var sqlCommand = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = "InitialDbSetup",
                        CommandType = System.Data.CommandType.StoredProcedure
                    };

                    rowsAffected = sqlCommand.ExecuteNonQuery();

                    session["IS_CONNECTED"] = "1";
                    session["ERROR_MESSAGE"] = "Store Procedure executed successfully";
                }
            }
            catch (SqlException ex)
            {
                session["IS_CONNECTED"] = "0";
                session["ERROR_MESSAGE"] = $"Script Failed. Error occured while executing the store procedure. Please contact administrator.";
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult IsConnectedToSQLServer(Session session)
        {
            session.Log("Begin Connection to SQL Server");
            string errorMessage = string.Empty;
            string serverName = session["SQL_SERVERNAME"];
            string databaseName = session["SQL_DATABASE"];
            string userID = session["SQL_USERNAME"];
            string password = session["SQL_PASSWORD"];
            bool isSqlAuth = session["IS_SQLAUTH"] == "1";

            if (string.IsNullOrEmpty(serverName) 
                || string.IsNullOrEmpty(databaseName) 
                || (isSqlAuth && (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(password))))
            {
                errorMessage = $"Please enter the required details.";
                session["ERROR_MESSAGE"] = errorMessage;
                return ActionResult.Success;
            }

            string connectionString = string.Empty;

            if (isSqlAuth)
            {
                connectionString = $"Server={serverName};Initial Catalog={databaseName};User ID={userID};Password={password};Integrated Security=false;Persist Security Info=false;";
            }
            else
            {
                connectionString = $"Server={serverName};Initial Catalog={databaseName};Integrated Security=true;Persist Security Info=false;";
            }

            session["CONNECTION_STRING"] = connectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        session["IS_CONNECTED"] = "1";
                        session["ERROR_MESSAGE"] = $"Test Successful.Please click Next to continue.";
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("server was not found"))
                {
                    errorMessage = "Invalid Server Name";
                }
                else if (ex.Message.Contains(databaseName))
                {
                    errorMessage = "Invalid Database Name";
                }
                else if (ex.Message.Contains(userID))
                {
                    errorMessage = "Invalid User Credentials";
                }

                session["IS_CONNECTED"] = "0";
                session["ERROR_MESSAGE"] = $"Test Failed.{errorMessage}.";
            }

            return ActionResult.Success;
        }
    }
}
