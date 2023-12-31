﻿using CAT.BusinessServices;
using CAT.Okapi.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using utils;

namespace CAT.BusinessServices
{
    public class DataStorage : IDataStorage
    {
        private String _termbasesConnectionString;
        private readonly String _translationMemoriesConnectionString;
        private readonly String _mainDbConnectionString;

        private readonly string _tmRepository;
        private readonly ILogger _logger;
        private readonly Dictionary<String, String> _sqlCommands = new Dictionary<String, String>();

        private readonly IConfiguration _configuration;

        private sealed class DBParams
        {
            public String dbName = default!;
            public String tmTableName = default!;
        }

        #region Translation memory
        private DBParams GetDBParams(String tmId)
        {
            var aPathElements = tmId.Split('/');
            var dbName = aPathElements[0];
            var tmTableName = aPathElements[1];

            var dbParams = new DBParams()
            {
                dbName = dbName,
                tmTableName = tmTableName
            };

            return dbParams;
        }

        public DataStorage(IConfiguration configuration, ILogger<DataStorage> logger)
        {
            _configuration = configuration;
            _termbasesConnectionString = _configuration!["SQLiteStorage:TermbasesConnectionString"]!;
            _translationMemoriesConnectionString = _configuration["SQLiteStorage:TranslationMemoriesConnectionString"]!;
            _mainDbConnectionString = _configuration["SQLiteStorage:MainDbConnectionString"]!;

            _tmRepository = _configuration["TMPath"]!;

            _logger = logger;
            //load the sql commands

            var resourceDir = _configuration["Resources"];

            var command = File.ReadAllText(Path.Combine(resourceDir!, "InsertTMEntry.sql"));
            _sqlCommands.Add("InsertTMEntry", command);
            command = File.ReadAllText(Path.Combine(resourceDir!, "CheckIncontextMatches.sql"));
            _sqlCommands.Add("CheckIncontextMatches", command);
            command = File.ReadAllText(Path.Combine(resourceDir!, "RestoreDatabase.sql"));
            _sqlCommands.Add("RestoreDatabase", command);
            command = File.ReadAllText(Path.Combine(resourceDir!, "DeleteTMEntry.sql"));
            _sqlCommands.Add("DeleteTMEntry", command);
        }

        public void CreateTranslationMemory(String tmId)
        {
            //try
            //{
            //    var dbParams = GetDBParams(tmId);
            //    var tmDir = Path.Combine(_tmRepository, dbParams.dbName);

            //    var dbName = dbParams.dbName;
            //    var tmTableName = dbParams.tmTableName;

            //    //SQL directory
            //    //var sqlDir = Path.Combine(tmDir, "SQL data");
            //    //if (!Directory.Exists(sqlDir))
            //    //    Directory.CreateDirectory(sqlDir);

            //    //Check the TM database
            //    var connectionString = String.Format(_translationMemoriesConnectionString, "");
            //    using (var sqlConnection = new SqlConnection(connectionString))
            //    {
            //        sqlConnection.Open();
            //        String sSql = "SELECT count(name) FROM sys.databases where name = @dbName";
            //        var cmd = new SqlCommand(sSql, sqlConnection);
            //        cmd.Parameters.Add(new SqlParameter("@dbName", dbName));
            //        var tmNum = (int)cmd.ExecuteScalar();

            //        if (tmNum == 0)
            //        {
            //            //create the database
            //            sSql = "CREATE DATABASE \"" + dbName + "\"";

            //            cmd = new SqlCommand(sSql, sqlConnection);
            //            cmd.ExecuteNonQuery();
            //        }
            //    }

            //    //create the entries table
            //    connectionString = String.Format(_translationMemoriesConnectionString, dbName);
            //    using (var sqlConnection = new SqlConnection(connectionString))
            //    {
            //        sqlConnection.Open();
            //        //check if the table exists

            //        String sSql = "SELECT count(TABLE_NAME)  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME=@tableName";
            //        var cmd = new SqlCommand(sSql, sqlConnection);
            //        cmd.Parameters.Add(new SqlParameter("@tableName", tmTableName));
            //        var tableNum = (int)cmd.ExecuteScalar();

            //        if (tableNum == 0)
            //        {
            //            //the TM table
            //            var resourceDir = WebConfigurationManager.AppSettings["Resources"];
            //            sSql = File.ReadAllText(Path.Combine(resourceDir, "tmTable.sql"));
            //            sSql = sSql.Replace("[TM_TABLE]", tmTableName);
            //            sSql = sSql.Replace("[PUBLIC_KEY_NAME]", "PK_" + tmTableName);
            //            cmd = new SqlCommand(sSql, sqlConnection);
            //            cmd.ExecuteNonQuery();

            //            //types
            //            sSql = File.ReadAllText(Path.Combine(resourceDir, "Types.sql"));
            //            cmd = new SqlCommand(sSql, sqlConnection);
            //            cmd.ExecuteNonQuery();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //Logging.DEBUG_LOG("DB Errors.log", "InsertOEVersion: " + e + "\nidSourceElement: " + idSourceElement);
            //    throw ex;
            //}
        }

        public bool TMExists(String tmId)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, "");
            //using (var sqlConnection = new SqlConnection(connectionString))
            //{
            //    sqlConnection.Open();
            //    String sSql = "SELECT count(name) FROM sys.databases where name = @dbName";
            //    var cmd = new SqlCommand(sSql, sqlConnection);
            //    cmd.Parameters.Add(new SqlParameter("@dbName", dbParams.dbName));
            //    var tmNum = (int)cmd.ExecuteScalar();

            //    if (tmNum == 0)
            //        return false;
            //}

            //connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (var sqlConnection = new SqlConnection(connectionString))
            //{
            //    sqlConnection.Open();
            //    //check if the table exists

            //    String sSql = "SELECT count(TABLE_NAME)  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME=@tableName";
            //    var cmd = new SqlCommand(sSql, sqlConnection);
            //    cmd.Parameters.Add(new SqlParameter("@tableName", dbParams.tmTableName));
            //    var tableNum = (int)cmd.ExecuteScalar();

            //    return tableNum > 0;
            //}

            return false;
        }

        public DataSet InsertTMEntry(String tmId, TextFragment source, TextFragment target, String context, String user, int speciality,
            int idTranslation, DateTime dateCreated, DateTime dateModified, String extensionData)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        var sSql = sqlCommands["InsertTMEntry"];
            //        sSql = sSql.Replace("TM_TABLE", dbParams.tmTableName);
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = sSql;
            //        //sqlCommand.CommandText = "InsertTMEntry";
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        var sourceText = source.GetCodedText();
            //        var targetText = target.GetCodedText();
            //        //this trick forces the execution plan reuse
            //        sqlCommand.Parameters.Add(new SqlParameter("@tmTableName", dbParams.tmTableName));
            //        sqlCommand.Parameters.Add(new SqlParameter("@source", SqlDbType.NVarChar, 4000));
            //        sqlCommand.Parameters["@source"].Value = sourceText;
            //        sqlCommand.Parameters.Add(new SqlParameter("@sourceHash", CATUtils.djb2hash(sourceText)));
            //        sqlCommand.Parameters.Add(new SqlParameter("@target", SqlDbType.NVarChar, 4000));
            //        sqlCommand.Parameters["@target"].Value = targetText;
            //        sqlCommand.Parameters.Add(new SqlParameter("@targetHash", CATUtils.djb2hash(targetText)));
            //        sqlCommand.Parameters.Add(new SqlParameter("@context", SqlDbType.VarChar, 16));
            //        sqlCommand.Parameters["@context"].Value = context;
            //        sqlCommand.Parameters.Add(new SqlParameter("@user", SqlDbType.NVarChar, 100));
            //        sqlCommand.Parameters["@user"].Value = user == null ? "" : user;
            //        sqlCommand.Parameters.Add(new SqlParameter("@speciality", speciality));
            //        sqlCommand.Parameters.Add(new SqlParameter("@idTranslation", idTranslation));
            //        sqlCommand.Parameters.Add(new SqlParameter("@dateCreated", dateCreated));
            //        sqlCommand.Parameters.Add(new SqlParameter("@dateModified", dateModified));
            //        sqlCommand.Parameters.Add(new SqlParameter("@extensionData", extensionData));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "InsertTMEntry: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public int DeleteTMEntry(String tmId, int idEntry)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        var sSql = sqlCommands["DeleteTMEntry"];
            //        sSql = sSql.Replace("TM_TABLE", dbParams.tmTableName);
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = sSql;
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@id", idEntry));

            //        int exactMatches = Convert.ToInt32(sqlCommand.ExecuteScalar());
            //        return exactMatches;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "InsertTMEntry: " + ex);
            //        throw ex;
            //    }
            //}

            return 0;
        }

        public DataSet GetSourceIndexData(String tmId)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        //sqlCommand.CommandText = "Select max(source) as source, idSource from [" + dbParams.tmTableName + "] group by idSource order by idSource";
            //        sqlCommand.CommandText = "Select source, idSource, speciality from [" + dbParams.tmTableName + "]";
            //        sqlCommand.CommandType = CommandType.Text;
            //        //sqlCommand.CommandTimeout = 180; 

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetSourceIndexData: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public DataSet GetTranslationMemoryData(String tmId)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select * from [" + dbParams.tmTableName + "]";
            //        sqlCommand.CommandType = CommandType.Text;

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetSourceIndexData: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public DataSet CheckIncontextMatches(String tmId, DataTable queryTable) //is there a nicer parameter name? : )
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //get the query
            //        var sSql = sqlCommands["CheckIncontextMatches"];
            //        sSql = sSql.Replace("[TM_TABLE]", dbParams.tmTableName);
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = sSql;
            //        sqlCommand.CommandType = CommandType.Text;

            //        SqlParameter queryTableParameter = new SqlParameter("@queryTable", queryTable);
            //        queryTableParameter.TypeName = "dbo.ContextMatchQueryTable";
            //        queryTableParameter.SqlDbType = SqlDbType.Structured;
            //        sqlCommand.Parameters.Add(queryTableParameter);

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "CheckIncontextMatches: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public DataSet GetIncontextMatch(String tmId, String source, String context)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select * from [" + dbParams.tmTableName + "] where sourceHash=@sourceHash and context=@context and source=@source";
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.Parameters.Add(new SqlParameter("@source", source));
            //        sqlCommand.Parameters.Add(new SqlParameter("@sourceHash", CATUtils.djb2hash(source)));
            //        sqlCommand.Parameters.Add(new SqlParameter("@context", context));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetIncontextMatch: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public bool DBExists(String dbName)
        {
            //var connectionString = String.Format(_translationMemoriesConnectionString, "master");
            //using (var sqlConnection = new SqlConnection(connectionString))
            //{
            //    sqlConnection.Open();
            //    String sSql = "Select DB_ID(@dbName)";
            //    var cmd = new SqlCommand(sSql, sqlConnection);
            //    cmd.Parameters.Add(new SqlParameter("@dbName", dbName));
            //    var dbId = cmd.ExecuteScalar();

            //    return dbId != DBNull.Value;
            //}

            return false;
        }

        public int GetTMEntriesNumber(String tmId)
        {
            //var dbParams = GetDBParams(tmId);
            ////check if the db exists
            //if (!DBExists(dbParams.dbName))
            //    return -1; //no database

            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "if OBJECT_ID('" + dbParams.tmTableName +
            //            "') is not null Select count(*) from [" + dbParams.tmTableName + "] else Select -1";
            //        //sqlCommand.Parameters.Add(new SqlParameter("@tableName", dbParams.tmTableName));
            //        //sqlCommand.CommandText = "if OBJECT_ID('@tableName') is not null Select count(*) from @tableName else Select -1";
            //        //sqlCommand.Parameters.Add(new SqlParameter("@tableName", dbParams.tmTableName));
            //        sqlCommand.CommandType = CommandType.Text;

            //        return (int)sqlCommand.ExecuteScalar();
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetTMEntriesNumber: " + ex);
            //        throw ex;
            //    }
            //}

            return 0;
        }

        public DataSet GetExactMatchesBySource(String tmId, String source)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select * from [" + dbParams.tmTableName + "] where sourceHash=@sourceHash and source=@source order by dateModified desc";
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.Parameters.Add(new SqlParameter("@source", SqlDbType.NVarChar, 4000));
            //        sqlCommand.Parameters["@source"].Value = source;
            //        sqlCommand.Parameters.Add(new SqlParameter("@sourceHash", CATUtils.djb2hash(source)));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetIncontextMatch: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public DataSet GetTMEntriesBySourceIds(String tmId, int[] aIdSource)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandType = CommandType.Text;
            //        var idSourceList = String.Join(",", aIdSource);
            //        //sqlCommand.CommandText = "Select * from [" + dbParams.tmTableName + "] where idSource in (@idSourceList)";
            //        //sqlCommand.Parameters.Add(new SqlParameter("@idSourceList", idSourceList));
            //        sqlCommand.CommandText =
            //            "Select * from [" + dbParams.tmTableName + "] where idSource in (" + idSourceList + ")";

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetTMEntriesByIdSource: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public DataSet GetTMEntriesByTargetText(String tmId, String sTarget)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandType = CommandType.Text;
            //        //remove multiple whitespaces
            //        sTarget = Regex.Replace(sTarget, @"\s+", " ");
            //        var aWords = sTarget.Split(' ');
            //        var searchText = "'%" + String.Join("%", aWords) + "%'";
            //        sqlCommand.CommandText =
            //            "Select * from [" + dbParams.tmTableName + "] where target like " + searchText;

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetTMEntriesByIdSource: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public void ShrinkDatabase(String dbName)
        {
            //var ShrinkDbTimeout = GetShrinkDbTimeout();

            //var connectionString = String.Format(_translationMemoriesConnectionString, "master");
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "DBCC SHRINKDATABASE (@dbName)";
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.CommandTimeout = ShrinkDbTimeout;
            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@dbName", dbName));

            //        int rows = sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "shrinkDatabase: " + ex);
            //        throw ex;
            //    }
            //}
        }

        public DataSet GetTMListFromDatabase(string dbName)
        {
            //var connectionString = String.Format(_translationMemoriesConnectionString, "");
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT count(*) FROM master.sys.databases where name=@catalog";
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.Parameters.Add(new SqlParameter("@catalog", dbName));

            //        var num = (int)sqlCommand.ExecuteScalar();
            //        DataSet ds = new DataSet();
            //        if (num > 0)
            //        {
            //            connectionString = String.Format(_translationMemoriesConnectionString, dbName);
            //            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            //            {
            //                sqlConn.Open();
            //                SqlCommand sqlCmd = new SqlCommand();
            //                sqlCmd.Connection = sqlConn;
            //                sqlCmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_CATALOG=@catalog";
            //                sqlCmd.CommandType = CommandType.Text;
            //                sqlCmd.Parameters.Add(new SqlParameter("@catalog", dbName));

            //                SqlDataAdapter adpt = new SqlDataAdapter(sqlCmd);
            //                adpt.Fill(ds);
            //                return ds;
            //            }
            //        }

            //        //an empty dataset is better than null
            //        sqlCommand.CommandText = "SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_CATALOG=''";
            //        SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
            //        sqlAdapter.Fill(ds);
            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetTMEntriesByIdSource: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public void SetDatabaseLastAccess(String dbName)
        {
            /*var connectionString = String.Format(_translationMemoriesConnectionString, "TranslationMemories");
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    connectionString = String.Format(_translationMemoriesConnectionString, dbName);
                    using (SqlConnection sqlConn = new SqlConnection(connectionString))
                    {
                        sqlConn.Open();
                        SqlCommand sqlCmd = new SqlCommand();
                        sqlCmd.Connection = sqlConn;
                        sqlCmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_CATALOG=@catalog";
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.Parameters.Add(new SqlParameter("@catalog", dbName));

                        SqlDataAdapter adpt = new SqlDataAdapter(sqlCmd);
                        adpt.Fill(ds);
                        return ds;
                    }
                }
                catch (SqlException ex)
                {
                    logger.Log("DB Errors.log", "GetTMEntriesByIdSource: " + ex);
                    throw ex;
                }
            }*/
        }


        #endregion

        #region term base
        public int CreateTB(int tbType, int IdType, String[] aLanguages)
        {
            //String sLanguages = String.Join(",", aLanguages);
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Insert_Termbase";
            //        sqlCommand.CommandType = CommandType.StoredProcedure;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@tbType", tbType));
            //        sqlCommand.Parameters.Add(new SqlParameter("@IdType", IdType));
            //        sqlCommand.Parameters.Add(new SqlParameter("@languages", sLanguages));
            //        sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@id", DbType = DbType.Int32, Direction = ParameterDirection.Output });

            //        sqlCommand.ExecuteNonQuery();
            //        return (int)sqlCommand.Parameters["@id"].Value;
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Log("DB Errors.log", "CreateTB: " + e + "\ntbType: " + tbType +
            //            "\nIdType: " + IdType);
            //        EmailHelper.SendDebugEmail("ERROR: " + e, "CreateTB", "alpar.meszaros@toppandigital.com");
            //        throw e;
            //    }
            //}

            return 0;
        }

        public DataSet GetTBInfo(int tbType, int idType)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT * from Termbases WHERE tbType = @tbType and idType=@idType";
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@tbType", tbType));
            //        sqlCommand.Parameters.Add(new SqlParameter("@idType", idType));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetTBInfo: " + e);
            //        throw e;
            //    }
            //}

            return null!;
        }

        public DataSet GetTBInfo(int idTermbase)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT * from Termbases WHERE id = @idTermbase";
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idTermbase", idTermbase));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
            //        throw e;
            //    }
            //}

            return null!;
        }

        public void UpdateLanguages(int idTermbase, String[] aLanguages)
        {
            ////sort the languages
            //Array.Sort(aLanguages);
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Update Termbases set languages=@languages WHERE id=@idTermbase";
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idTermbase", idTermbase));
            //        sqlCommand.Parameters.Add(new SqlParameter("@languages", String.Join(",", aLanguages)));

            //        sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
            //        throw e;
            //    }
            //}
        }

        public void DeleteTBEntry(int idTermbase, int idEntry)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Delete termbaseEntries where idTermbase = @idTermbase and id = @idEntry";
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idTermbase", idTermbase));
            //        sqlCommand.Parameters.Add(new SqlParameter("@idEntry", idEntry));

            //        sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
            //        throw e;
            //    }
            //}
        }

        public void UpdateLastModified(int idTermbase)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Update Termbases set dateUpdated=@dateUpdated WHERE id = @idTermbase";
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idTermbase", idTermbase));
            //        sqlCommand.Parameters.Add(new SqlParameter("@dateUpdated", DateTime.Now));

            //        sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
            //        throw e;
            //    }
            //}
        }

        public int InsertTBEntry(int idTermbase, String user)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Insert_TBEntry";
            //        sqlCommand.CommandType = CommandType.StoredProcedure;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idTermbase", idTermbase));
            //        sqlCommand.Parameters.Add(new SqlParameter("@comment", comment));
            //        sqlCommand.Parameters.Add(new SqlParameter("@user", user));
            //        sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@id", DbType = DbType.Int32, Direction = ParameterDirection.Output });

            //        sqlCommand.ExecuteNonQuery();
            //        return (int)sqlCommand.Parameters["@id"].Value;
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Log("DB Errors.log", "InsertTBEntry: " + e + "\nidTermbase: ");
            //        EmailHelper.SendDebugEmail("ERROR: " + e, "CreateTB", "alpar.meszaros@toppandigital.com");
            //        throw e;
            //    }
            //}

            return 0;
        }

        public int InsertTerm(int idEntry, KeyValuePair<String, String> term, String user)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Insert_Term";
            //        sqlCommand.CommandType = CommandType.StoredProcedure;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idEntry", idEntry));
            //        sqlCommand.Parameters.Add(new SqlParameter("@language", term.Key));
            //        sqlCommand.Parameters.Add(new SqlParameter("@term", term.Value));
            //        sqlCommand.Parameters.Add(new SqlParameter("@user", user));
            //        sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@id", DbType = DbType.Int32, Direction = ParameterDirection.Output });

            //        sqlCommand.ExecuteNonQuery();
            //        return (int)sqlCommand.Parameters["@id"].Value;
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Log("DB Errors.log", "InsertTBEntry: " + e + "\nidTermbase: ");
            //        EmailHelper.SendDebugEmail("ERROR: " + e, "CreateTB", "alpar.meszaros@toppandigital.com");
            //        throw e;
            //    }
            //}

            return 0;
        }

        public void RemoveTerms(int idTermbase, String langCode)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Remove_Terms";
            //        sqlCommand.CommandType = CommandType.StoredProcedure;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idTermbase", idTermbase));
            //        sqlCommand.Parameters.Add(new SqlParameter("@language", langCode));

            //        sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Log("DB Errors.log", "RemoveTerms: " + e + "\nidTermbase: ");
            //        EmailHelper.SendDebugEmail("ERROR: " + e, "RemoveTerms", "alpar.meszaros@toppandigital.com");
            //        throw e;
            //    }
            //}
        }

        public DataSet GetTBEntry(int idEntry)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select * from termbaseEntries where id=@idEntry";
            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idEntry", idEntry));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
            //        throw e;
            //    }
            //}

            return null!;
        }

        public DataSet ListTBEntries(int idTermbase, String[] languages)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select termbaseEntries.id, termbaseEntries.dateCreated, termbaseEntries.dateModified, termbaseEntries.comment, termbaseEntries.createdBy, termbaseEntries.modifiedBy, termbaseTerms.language, termbaseTerms.term from Termbases inner join termbaseEntries on Termbases.id = termbaseEntries.idTermbase inner join termbaseTerms on termbaseTerms.idEntry = termbaseEntries.id where Termbases.id=@idTermbase";
            //        if (languages?.Length > 0)
            //        {
            //            languages = Array.ConvertAll(languages, lang => "'" + lang + "'");
            //            sqlCommand.CommandText += " and language in (" + String.Join(",", languages) + ")";
            //        }
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@idTermbase", idTermbase));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
            //        throw e;
            //    }
            //}

            return null!;
        }

        public void UpdateTMEntry(String tmId, int idEntry, Dictionary<String, String> fieldsToUpdate)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Update [" + dbParams.tmTableName + "] set target=@target, targetHash=@targetHash, dateModified=@dateModified, modifiedBy=@modifiedBy where id=@idEntry";
            //        //sqlCommand.CommandText = "InsertTMEntry";
            //        sqlCommand.CommandType = CommandType.Text;

            //        //for now we update only the user and the target
            //        var user = fieldsToUpdate["user"];
            //        var target = CATUtils.TmxSegmentToTextFragmentSimple(fieldsToUpdate["target"]).GetCodedText().Trim();

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SqlParameter("@target", target));
            //        sqlCommand.Parameters.Add(new SqlParameter("@targetHash", CATUtils.djb2hash(target)));
            //        sqlCommand.Parameters.Add(new SqlParameter("@dateModified", DateTime.Now));
            //        sqlCommand.Parameters.Add(new SqlParameter("@modifiedBy", user));
            //        sqlCommand.Parameters.Add(new SqlParameter("@idEntry", idEntry));

            //        sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "InsertTMEntry: " + ex);
            //        throw ex;
            //    }
            //}
        }
        #endregion

        #region Misc.
        public DataSet GetDbList()
        {
            //var connectionString = String.Format(_translationMemoriesConnectionString, "");
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT name FROM master.sys.databases WHERE database_id > 4";
            //        sqlCommand.CommandType = CommandType.Text;

            //        DataSet ds = new DataSet();
            //        SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
            //        sqlAdapter.Fill(ds);
            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetDbList: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public DataSet GetDbListSyncSource(String backupSourceConnectionString)
        {
            //backupSourceConnectionString = String.Format(backupSourceConnectionString, ""); //use master
            //using (SqlConnection sqlConnection = new SqlConnection(backupSourceConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT name FROM master.sys.databases WHERE database_id > 4";
            //        sqlCommand.CommandType = CommandType.Text;

            //        DataSet ds = new DataSet();
            //        SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
            //        sqlAdapter.Fill(ds);
            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetDbList: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public DataSet GetTMEntriesFromSyncSource(String backupSourceConnectionString, String dbName)
        {
            //backupSourceConnectionString = String.Format(backupSourceConnectionString, dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(backupSourceConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select sysobjects.name tmName, sysindexes.rows entryNum from sysobjects inner join sysindexes on sysindexes.id = sysobjects.id and sysindexes.indid in (0,1) where sysobjects.xtype = 'U' ";
            //        sqlCommand.CommandType = CommandType.Text;

            //        DataSet ds = new DataSet();
            //        SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
            //        sqlAdapter.Fill(ds);
            //        return ds;
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "GetDbList: " + ex);
            //        throw ex;
            //    }
            //}

            return null!;
        }

        public void BackupDatabase(String dbName)
        {
            //var connectionString = String.Format(_translationMemoriesConnectionString, "");
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.CommandText = "Backup database @dbName to disk=@backupPath WITH COMPRESSION, INIT";
            //        //sqlCommand.CommandText = "Backup database @dbName to disk=@backupPath"; //for SQL express

            //        var backupPath = Path.Combine(WebConfigurationManager.AppSettings["SQLBackupFolder"], dbName + ".bak");
            //        sqlCommand.Parameters.Add(new SqlParameter("@dbName", dbName));
            //        sqlCommand.Parameters.Add(new SqlParameter("@backupPath", backupPath));

            //        sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (SqlException ex)
            //    {
            //        logger.Log("DB Errors.log", "BackupDatabase: " + dbName + "\n" + ex);
            //        throw ex;
            //    }
            //}
        }

        public void RestoreDatabase(String backupPath, String dbName)
        {
            //try
            //{
            //    //Check the TM database
            //    var connectionString = String.Format(_translationMemoriesConnectionString, "");
            //    var dbRoot = WebConfigurationManager.AppSettings["DatabaseRootFolder"];
            //    using (var sqlConnection = new SqlConnection(connectionString))
            //    {
            //        sqlConnection.Open();
            //        //restore the database
            //        var sSql = sqlCommands["RestoreDatabase"];
            //        sSql = sSql.Replace("[dbName]", dbName);
            //        sSql = sSql.Replace("[backupPath]", backupPath);
            //        sSql = sSql.Replace("[mdfPath]", Path.Combine(dbRoot, dbName + ".mdf"));
            //        sSql = sSql.Replace("[logName]", dbName + "_log");
            //        sSql = sSql.Replace("[logPath]", Path.Combine(dbRoot, dbName + "_log.ldf"));
            //        var cmd = new SqlCommand(sSql, sqlConnection);

            //        cmd = new SqlCommand(sSql, sqlConnection);
            //        cmd.ExecuteNonQuery();

            //    }
            //}
            //catch (Exception ex)
            //{
            //    //Logging.DEBUG_LOG("DB Errors.log", "InsertOEVersion: " + e + "\nidSourceElement: " + idSourceElement);
            //    throw ex;
            //}
        }

        public DataSet GetSpecialities()
        {
            //using (SqlConnection sqlConnection = new SqlConnection(translationsConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select specialityId, description from TR_Specialities";
            //        sqlCommand.CommandType = CommandType.Text;

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetUserFullNameTask: " + e);
            //        //throw e;
            //        return null!;
            //    }
            //}
            return null!;
        }

        /// <summary>
        /// Get the Timeout value from [Translations].[_SystemSettings] 
        /// if the value <b>Shrink_DB_Timeout</b>is not there then returns <b>120</b> seconds as default value
        /// </summary>
        /// <returns></returns>
        private int GetShrinkDbTimeout()
        {
            //int retval = 2 * 60; //default Value 2 minutes

            //using (SqlConnection sqlConnection = new SqlConnection(translationsConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT TOP(1) value FROM [_SystemSettings] WHERE name = @SettingName";
            //        sqlCommand.Parameters.Add(new SqlParameter("@SettingName", "Shrink_DB_Timeout"));
            //        var value = (string) sqlCommand.ExecuteScalar();
            //        if(!String.IsNullOrEmpty(value) )
            //        {
            //            retval = Convert.ToInt32(value);
            //        }
            //    }
            //    catch (SqlException e)
            //    {
            //        logger.Log("DB Errors.log", "GetShrinkDbTimeout: " + e);
            //        //throw e;
            //    }
            //}
            //return retval;

            return 0;
        }
        #endregion
    }
}