using CAT.BusinessServices;
using CAT.Okapi.Resources;
using CAT.Utils;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using utils;

namespace CAT.BusinessServices
{
    public class SQLiteStorage : IDataStorage
    {
        private String _termbasesConnectionString;

        public readonly String _tmRepository;
        private readonly Dictionary<String, String> _sqlCommands = new();
        private ILogger _logger;

        private IConfiguration _configuration;

        private class DBParams
        {
            public String dbName = default!;
            public String tmTableName = default!;
        }

        #region Translation memory
        private DBParams GetDBParams(String tmPath)
        {
            var aPathElements = tmPath.Split('/');
            var dbName = aPathElements[0];
            var tmTableName = aPathElements[1];

            var dbParams = new DBParams()
            {
                dbName = dbName,
                tmTableName = tmTableName
            };

            return dbParams;
        }

        public SQLiteStorage(IConfiguration configuration, ILogger<SQLiteStorage> logger)
        {
            _configuration = configuration;
            _logger = logger;

            //these should be static fields
            _tmRepository = _configuration["TMPath"]!;
            _termbasesConnectionString = _configuration!["SQLiteStorage:TermbasesConnectionString"]!;

            //load the sql commands
            var resourceDir = Path.Combine(_configuration["Resources"]!, "SQLite");
            var command = File.ReadAllText(Path.Combine(resourceDir!, "InsertTMEntry.sql"));
            _sqlCommands.Add("InsertTMEntry", command);
            command = File.ReadAllText(Path.Combine(resourceDir!, "CheckIncontextMatches.sql"));
            _sqlCommands.Add("CheckIncontextMatches", command);
            command = File.ReadAllText(Path.Combine(resourceDir!, "RestoreDatabase.sql"));
            _sqlCommands.Add("RestoreDatabase", command);
            command = File.ReadAllText(Path.Combine(resourceDir!, "DeleteTMEntry.sql"));
            _sqlCommands.Add("DeleteTMEntry", command);
            command = File.ReadAllText(Path.Combine(resourceDir!, "CreateTMTable.sql"));
            _sqlCommands.Add("CreateTMTable", command);
        }

        public void CreateTranslationMemory(String tmPath)
        {
            try
            {
                var dbParams = GetDBParams(tmPath);
                var tmTableName = dbParams.tmTableName;

                var dbFolder = Path.Combine(_tmRepository, dbParams.dbName, "SQLData");
                //check the db folder
                if (!Directory.Exists(dbFolder))
                    Directory.CreateDirectory(dbFolder);
                var dbPath = Path.Combine(dbFolder, dbParams.dbName + ".db");
                string connectionString = $"Data Source={dbPath};Version=3;";
                //create the entries table
                using (var sqlConnection = new SQLiteConnection(connectionString))
                {
                    sqlConnection.Open();
                    //check if the table exists

                    String sSql = "SELECT count(name) FROM sqlite_master WHERE type='table' AND name=@tableName";
                    var cmd = new SQLiteCommand(sSql, sqlConnection);
                    cmd.Parameters.Add(new SQLiteParameter("@tableName", tmTableName));
                    var tableNum = (long)cmd.ExecuteScalar();

                    if (tableNum == 0)
                    {
                        //the TM table
                        sSql = _sqlCommands["CreateTMTable"];
                        sSql = sSql.Replace("[TM_TABLE]", tmTableName);
                        sSql = sSql.Replace("[PUBLIC_KEY_NAME]", "PK_" + tmTableName);
                        cmd = new SQLiteCommand(sSql, sqlConnection);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateTranslationMemory -> tmPath: " + tmPath + " error: " + ex.Message);
                throw;
            }
        }

        public bool TMExists(String tmPath)
        {
            var dbParams = GetDBParams(tmPath);
            var dbPath = Path.Combine(_tmRepository, dbParams.dbName + "/SQLData/" + dbParams.dbName + ".db");
            if (!File.Exists(dbPath))
                return false;

            string connectionString = $"Data Source={dbPath};Version=3;";
            using (var sqlConnection = new SQLiteConnection(connectionString))
            {
                sqlConnection.Open(); //it will create the database if it doesn't exists
                //check if the table exists
                String sSql = "SELECT count(name) FROM sqlite_master WHERE type='table' AND name=@tableName;";
                var cmd = new SQLiteCommand(sSql, sqlConnection);
                cmd.Parameters.Add(new SQLiteParameter("@tableName", dbParams.tmTableName));
                var tableNum = (long)cmd.ExecuteScalar();

                return tableNum > 0;
            }
        }

        public int GetTMEntriesNumber(String tmPath)
        {
            var dbParams = GetDBParams(tmPath);
            var dbPath = Path.Combine(_tmRepository, dbParams.dbName + "/SQLData/" + dbParams.dbName + ".db");

            string connectionString = $"Data Source={dbPath};Version=3;";
            using (var sqlConnection = new SQLiteConnection(connectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = $"SELECT CASE WHEN EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = '{dbParams.tmTableName}') THEN(SELECT count(*) FROM [{dbParams.tmTableName}]) ELSE - 1 END;";
                    sqlCommand.CommandType = CommandType.Text;

                    return (int)((long)sqlCommand.ExecuteScalar());
                }
                catch (SQLiteException ex)
                {
                    _logger.LogError("GetTMEntriesNumber -> tmPath: " + tmPath  + " error: " + ex);
                    throw;
                }
            }
        }

        public DataSet GetTMListFromDatabase(string dbName)
        {
            var dbPath = Path.Combine(_tmRepository, dbName + "/SQLData/" + dbName + ".db");
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (var sqlConnection = new SQLiteConnection(connectionString))
            {
                try
                {
                    //open connection
                    connectionString = String.Format(connectionString);
                    using (var sqlConn = new SQLiteConnection(connectionString))
                    {
                        sqlConn.Open();
                        var sqlCmd = new SQLiteCommand();
                        sqlCmd.Connection = sqlConn;
                        sqlCmd.CommandText = "SELECT name AS TABLE_NAME FROM sqlite_master WHERE type='table';";
                        sqlCmd.CommandType = CommandType.Text;

                        var adpt = new SQLiteDataAdapter(sqlCmd);
                        DataSet ds = new DataSet();
                        adpt.Fill(ds);
                        return ds;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("GetTMListFromDatabase -> dbName " +dbName + " error: "+ ex);
                    throw;
                }
            }
        }

        public DataSet InsertTMEntry(String tmPath, TextFragment source, TextFragment target, String context, String user, int speciality,
            int jobId, DateTime dateCreated, DateTime dateModified, String extensionData)
        {
            var dbParams = GetDBParams(tmPath);
            var dbPath = Path.Combine(_tmRepository, dbParams.dbName + "/SQLData/" + dbParams.dbName + ".db");
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (var sqlConnection = new SQLiteConnection(connectionString))
            {
                try
                {
                    var sSql = _sqlCommands["InsertTMEntry"];
                    sSql = sSql.Replace("[TM_TABLE]", dbParams.tmTableName);
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = sSql;
                    //sqlCommand.CommandText = "InsertTMEntry";
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    var sourceText = source.GetCodedText();
                    var targetText = target.GetCodedText();
                    //this trick forces the execution plan reuse
                    sqlCommand.Parameters.Add(new SQLiteParameter(":source", DbType.String) { Value = sourceText });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":sourceHash", CATUtils.djb2hash(sourceText)));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":target", DbType.String) { Value = targetText });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":targetHash", CATUtils.djb2hash(targetText)));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":context", DbType.String) { Value = context });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":speciality", speciality));

                    //the metadata
                    var metadata = JsonConvert.SerializeObject(new { jobId, createdBy = user, dateCreated = DateTime.UtcNow, modifiedBy = user, 
                        dateModified = DateTime.UtcNow });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":metadata", metadata));

                    var adpt = new SQLiteDataAdapter(sqlCommand);
                    DataSet ds = new DataSet();
                    adpt.Fill(ds);

                    return ds;
                }
                catch (SQLiteException ex)
                {
                    _logger.LogError("InsertTMEntry -> tmPath: " + tmPath + " error: " + ex);
                    throw ex;
                }
            }
        }

        public int DeleteTMEntry(String tmPath, int idEntry)
        {
            //var dbParams = GetDBParams(tmPath);
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

        public DataSet GetSourceIndexData(String tmPath)
        {
            //var dbParams = GetDBParams(tmPath);
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

        public DataSet GetTranslationMemoryData(String tmPath)
        {
            //var dbParams = GetDBParams(tmPath);
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

        public DataSet CheckIncontextMatches(String tmPath, DataTable queryTable) //is there a nicer parameter name? : )
        {
            //var dbParams = GetDBParams(tmPath);
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

        public DataSet GetIncontextMatch(String tmPath, String source, String context)
        {
            //var dbParams = GetDBParams(tmPath);
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

        public DataSet GetExactMatchesBySource(String tmPath, String source)
        {
            //var dbParams = GetDBParams(tmPath);
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

        public DataSet GetTMEntriesBySourceIds(String tmPath, int[] aIdSource)
        {
            //var dbParams = GetDBParams(tmPath);
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

        public DataSet GetTMEntriesByTargetText(String tmPath, String sTarget)
        {
            //var dbParams = GetDBParams(tmPath);
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

        public DataSet GetTBInfoById(int idTermbase)
        {
            //using (SqlConnection sqlConnection = new SqlConnection(termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        SqlCommand sqlCommand = new SqlCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT * from Termbases WHERE id=@idTermbase";
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

        public int InsertTBEntry(int idTermbase, String comment, String user)
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

        public void UpdateTMEntry(String tmPath, int idEntry, Dictionary<String, String> fieldsToUpdate)
        {
            //var dbParams = GetDBParams(tmPath);
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