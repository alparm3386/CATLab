package com.tm.okapi.utils;

public class DBFactory {
	/*public static String _translationsConnectionString = ConfigurationManager.AppSettings["TranslationsConnectionString"];

	public static DataSet GetSystemSettings()
        {
            using (SqlConnection sqlConnection = new SqlConnection(_translationsConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "SELECT * from _SystemSettings";
                    sqlCommand.CommandType = CommandType.Text;

                    SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
                    DataSet ds = new DataSet();
                    adpt.Fill(ds);

                    return ds;
                }
                catch (SqlException e)
                {
                    Logging.DEBUG_LOG("DB Errors.log", "GetSystemSettings: " + e);
                    throw e;
                }
            }
        }*/
}
