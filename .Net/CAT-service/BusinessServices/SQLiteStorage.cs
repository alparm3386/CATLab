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
        private DBParams GetDBParams(String tmIds)
        {
            var aPathElements = tmIds.Split('/');
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

        public void CreateTranslationMemory(String tmId)
        {
            try
            {
                var dbParams = GetDBParams(tmId);
                var tmTableName = dbParams.tmTableName;

                var dbFolder = Path.Combine(_tmRepository, dbParams.dbName, "SQLData");
                //check the db folder
                if (!Directory.Exists(dbFolder))
                    Directory.CreateDirectory(dbFolder);
                var dbPath = Path.Combine(dbFolder, dbParams.dbName + ".db");
                string connectionString = $"Data Source={dbPath};Version=3;Pooling=True;";
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
                _logger.LogError("CreateTranslationMemory -> tmId: " + tmId + " error: " + ex.Message);
                throw;
            }
        }

        public bool TMExists(String tmId)
        {
            var dbParams = GetDBParams(tmId);
            var dbPath = Path.Combine(_tmRepository, dbParams.dbName + "/SQLData/" + dbParams.dbName + ".db");
            if (!File.Exists(dbPath))
                return false;

            string connectionString = $"Data Source={dbPath};Version=3;Pooling=True;";
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

        public int GetTMEntriesNumber(String tmId)
        {
            var dbParams = GetDBParams(tmId);
            var dbPath = Path.Combine(_tmRepository, dbParams.dbName + "/SQLData/" + dbParams.dbName + ".db");

            string connectionString = $"Data Source={dbPath};Version=3;Pooling=True;";
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
                    _logger.LogError("GetTMEntriesNumber -> tmId: " + tmId + " error: " + ex);
                    throw;
                }
            }
        }

        public DataSet GetTMListFromDatabase(string dbName)
        {
            var dbPath = Path.Combine(_tmRepository, dbName + "/SQLData/" + dbName + ".db");
            string connectionString = $"Data Source={dbPath};Version=3;Pooling=True;";
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
                    _logger.LogError("GetTMListFromDatabase -> dbName " + dbName + " error: " + ex);
                    throw;
                }
            }
        }

        public DataSet InsertTMEntry(String tmId, TextFragment source, TextFragment target, String context, String user, 
            int speciality, int jobId, DateTime dateCreated, DateTime dateModified, String extensionData)
        {
            var dbParams = GetDBParams(tmId);
            var dbPath = Path.Combine(_tmRepository, dbParams.dbName + "/SQLData/" + dbParams.dbName + ".db");
            string connectionString = $"Data Source={dbPath};Version=3;Pooling=True;";
            using (var sqlConnection = new SQLiteConnection(connectionString))
            {
                try
                {
                    //check if the source exists
                    var sSql = "Select * from [TM_TABLE] where sourcehash=:sourceHash and source=:source";
                    sSql = sSql.Replace("[TM_TABLE]", dbParams.tmTableName);
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = sSql;
                    sqlCommand.CommandType = CommandType.Text;
                    //set the query params
                    var sourceText = source.GetCodedText();
                    sqlCommand.Parameters.Add(new SQLiteParameter(":source", DbType.String) { Value = sourceText });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":sourceHash", CATUtils.djb2hash(sourceText)));

                    var isNew = sqlCommand.ExecuteScalar() == null;
                    // Close the connection before opening it again

                    sSql = _sqlCommands["InsertTMEntry"];
                    sSql = sSql.Replace("[TM_TABLE]", dbParams.tmTableName);
                    sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = sSql;
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    var targetText = target.GetCodedText();
                    //this trick forces the execution plan reuse
                    sqlCommand.Parameters.Add(new SQLiteParameter(":source", DbType.String) { Value = sourceText });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":sourceHash", CATUtils.djb2hash(sourceText)));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":target", DbType.String) { Value = targetText });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":targetHash", CATUtils.djb2hash(targetText)));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":context", DbType.String) { Value = context });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":speciality", speciality));

                    //the metadata
                    var metadata = JsonConvert.SerializeObject(new
                    {
                        jobId,
                        createdBy = user,
                        dateCreated = DateTime.UtcNow,
                        modifiedBy = user,
                        dateModified = DateTime.UtcNow
                    });
                    sqlCommand.Parameters.Add(new SQLiteParameter(":metadata", metadata));

                    var adpt = new SQLiteDataAdapter(sqlCommand);
                    DataSet ds = new DataSet();
                    adpt.Fill(ds);
                    ds.Tables[0].Rows[0]["isNew"] = isNew;

                    //update the sourceId
                    sSql = "UPDATE [TM_TABLE] SET sourceId = last_insert_rowid() WHERE id=:id;";
                    sSql = sSql.Replace("[TM_TABLE]", dbParams.tmTableName);
                    sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = sSql;
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":id", DbType.Int32) 
                        { Value = (int)(long)ds.Tables[0].Rows[0]["sourceId"] });
                    sqlCommand.ExecuteNonQuery();

                    return ds;
                }
                catch (SQLiteException ex)
                {
                    _logger.LogError("InsertTMEntry -> tmId: " + tmId + " error: " + ex);
                    throw ex;
                }
            }
        }

        public int DeleteTMEntry(String tmId, int entryId)
        {
            var dbParams = GetDBParams(tmId);
            var dbPath = Path.Combine(_tmRepository, dbParams.dbName + "/SQLData/" + dbParams.dbName + ".db");
            string connectionString = $"Data Source={dbPath};Version=3;Pooling=True;";
            using (var sqlConnection = new SQLiteConnection(connectionString))
            {
                try
                {
                    var sSql = _sqlCommands["DeleteTMEntry"];
                    sSql = sSql.Replace("TM_TABLE", dbParams.tmTableName);
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = sSql;
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":id", entryId));

                    int luceneId = Convert.ToInt32(sqlCommand.ExecuteScalar());
                    return luceneId;
                }
                catch (SQLiteException ex)
                {
                    _logger.LogError("InsertTMEntry -> tmId: " + tmId + " error: " + ex);
                    throw ex;
                }
            }
        }

        public DataSet GetTMEntriesBySourceIds(String tmId, int[] aSourceIds)
        {
            var dbParams = GetDBParams(tmId);
            var dbPath = Path.Combine(_tmRepository, dbParams.dbName + "/SQLData/" + dbParams.dbName + ".db");
            string connectionString = $"Data Source={dbPath};Version=3;Pooling=True;";
            using (var sqlConnection = new SQLiteConnection(connectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandType = CommandType.Text;
                    var idSourceList = String.Join(",", aSourceIds);
                    sqlCommand.CommandText = "Select * from [" + dbParams.tmTableName + "] where sourceId in (" + idSourceList + ");";

                    var adpt = new SQLiteDataAdapter(sqlCommand);
                    DataSet ds = new DataSet();
                    adpt.Fill(ds);

                    return ds;
                }
                catch (SQLiteException ex)
                {
                    _logger.LogError("InsertTMEntry -> tmId: " + tmId + " error: " + ex);
                    throw ex;
                }
            }
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
            //        var sqlCommand = new SQLiteCommand();
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
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "GetSourceIndexData: " + ex);
            //        throw ex;
            //    }
            //}

            throw new NotImplementedException();
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select * from [" + dbParams.tmTableName + "]";
            //        sqlCommand.CommandType = CommandType.Text;

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "GetSourceIndexData: " + ex);
            //        throw ex;
            //    }
            //}

            throw new NotImplementedException();
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = sSql;
            //        sqlCommand.CommandType = CommandType.Text;

            //        SQLiteParameter queryTableParameter = new SQLiteParameter("@queryTable", queryTable);
            //        queryTableParameter.TypeName = "ContextMatchQueryTable";
            //        queryTableParameter.SqlDbType = SqlDbType.Structured;
            //        sqlCommand.Parameters.Add(queryTableParameter);

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "CheckIncontextMatches: " + ex);
            //        throw ex;
            //    }
            //}

            throw new NotImplementedException();
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select * from [" + dbParams.tmTableName + "] where sourceHash=@sourceHash and context=@context and source=@source";
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@source", source));
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@sourceHash", CATUtils.djb2hash(source)));
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@context", context));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "GetIncontextMatch: " + ex);
            //        throw ex;
            //    }
            //}

            throw new NotImplementedException();
        }

        public bool DBExists(String dbName)
        {
            //var connectionString = String.Format(_translationMemoriesConnectionString, "master");
            //using (var sqlConnection = new SqlConnection(connectionString))
            //{
            //    sqlConnection.Open();
            //    String sSql = "Select DB_ID(@dbName)";
            //    var cmd = new SqlCommand(sSql, sqlConnection);
            //    cmd.Parameters.Add(new SQLiteParameter("@dbName", dbName));
            //    var dbId = cmd.ExecuteScalar();

            //    return dbId != DBNull.Value;
            //}

            throw new NotImplementedException();
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select * from [" + dbParams.tmTableName + "] where sourceHash=@sourceHash and source=@source order by dateModified desc";
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@source", SqlDbType.NVarChar, 4000));
            //        sqlCommand.Parameters["@source"].Value = source;
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@sourceHash", CATUtils.djb2hash(source)));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "GetIncontextMatch: " + ex);
            //        throw ex;
            //    }
            //}

            throw new NotImplementedException();
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
            //        var sqlCommand = new SQLiteCommand();
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
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "GetTMEntriesByIdSource: " + ex);
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "DBCC SHRINKDATABASE (@dbName)";
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.CommandTimeout = ShrinkDbTimeout;
            //        //set the query params
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@dbName", dbName));

            //        int rows = sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "shrinkDatabase: " + ex);
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
                        sqlCmd.Parameters.Add(new SQLiteParameter("@catalog", dbName));

                        SqlDataAdapter adpt = new SqlDataAdapter(sqlCmd);
                        adpt.Fill(ds);
                        return ds;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("DB Errors.log", "GetTMEntriesByIdSource: " + ex);
                    throw ex;
                }
            }*/
        }


        #endregion

        #region term base
        public int CreateTB(int tbType, int idType, String[] aLanguages)
        {
            String languages = String.Join(",", aLanguages);
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    using (var sqlCommand = new SQLiteCommand(sqlConnection))
                    {

                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "SELECT id FROM termbases WHERE tbType = :tbType AND idType = :idType;";
                        sqlCommand.Parameters.Add(new SQLiteParameter(":tbType", tbType));
                        sqlCommand.Parameters.Add(new SQLiteParameter(":idType", idType));
                        sqlCommand.CommandType = CommandType.Text;
                        var result = sqlCommand.ExecuteScalar();
                        var termbaseId = -1;
                        if (result != null && result != DBNull.Value)
                            termbaseId = Convert.ToInt32(result);

                        if (termbaseId > 0)
                        {
                            sqlCommand.CommandText = @"UPDATE termbases 
                                SET dateUpdated = CURRENT_TIMESTAMP 
                                WHERE tbType = :tbType AND IdType = :idType;";

                            sqlCommand.Parameters.Add(new SQLiteParameter(":tbType", tbType));
                            sqlCommand.Parameters.Add(new SQLiteParameter(":idType", idType));
                            sqlCommand.ExecuteNonQuery();

                            return termbaseId;
                        }
                        else
                        {
                            sqlCommand.CommandText = @"INSERT INTO termbases (dateCreated, dateUpdated, languages, tbType, IdType)
                                VALUES (CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, :languages, :tbType, :idType);";

                            sqlCommand.Parameters.Add(new SQLiteParameter(":tbType", tbType));
                            sqlCommand.Parameters.Add(new SQLiteParameter(":idType", idType));
                            sqlCommand.Parameters.Add(new SQLiteParameter(":languages", languages));
                            sqlCommand.ExecuteNonQuery();
                            return (int)sqlConnection.LastInsertRowId;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("CreateTB -> tbType: " + tbType + "idType: " + idType + " error: " + e);
                    throw;
                }
            }
        }

        public DataSet GetTBInfo(int tbType, int idType)
        {
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    using (var sqlCommand = new SQLiteCommand(sqlConnection))
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "SELECT * from termbases WHERE tbType = :tbType and idType=:idType";
                        sqlCommand.CommandType = CommandType.Text;

                        //set the query params
                        sqlCommand.Parameters.Add(new SQLiteParameter(":tbType", tbType));
                        sqlCommand.Parameters.Add(new SQLiteParameter(":idType", idType));

                        var adpt = new SQLiteDataAdapter(sqlCommand);
                        DataSet ds = new DataSet();
                        adpt.Fill(ds);

                        return ds;
                    }
                }
                catch (SQLiteException e)
                {
                    _logger.LogError("GetTBInfo -> tbType: " + tbType + "IdType: " + idType + " error: " + e);
                    throw;
                }
            }
        }

        public DataSet GetTBInfo(int termbaseId)
        {
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "SELECT * from Termbases WHERE id=:termbaseId";
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":termbaseId", termbaseId));

                    var adpt = new SQLiteDataAdapter(sqlCommand);
                    DataSet ds = new DataSet();
                    adpt.Fill(ds);

                    return ds;
                }
                catch (Exception e)
                {
                    _logger.LogError("GetTBInfoById ->  termbaseId: " + termbaseId + " error: " + e);
                    throw;
                }
            }
        }

        public void UpdateLanguages(int termbaseId, String[] aLanguages)
        {
            var sortedLanguages = aLanguages.Distinct().OrderBy(lang => lang).ToArray();
            //sort the languages
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "Update Termbases set languages=:languages WHERE id=:termbaseId";
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":termbaseId", termbaseId));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":languages", String.Join(",", sortedLanguages)));

                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    _logger.LogError("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
                    throw;
                }
            }
        }

        public void UpdateLastModified(int termbaseId)
        {
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "Update Termbases set dateUpdated=:dateUpdated WHERE id = :termbaseId";
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":termbaseId", termbaseId));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":dateUpdated", DateTime.Now));

                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    _logger.LogError("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
                    throw;
                }
            }
        }

        public void RemoveTerms(int termbaseId, String langCode)
        {
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = @"DELETE FROM termbaseTerms WHERE entryId IN (SELECT termbaseEntries.id FROM termbaseEntries 
                        WHERE termbaseId = :termbaseId) AND language = :language;";
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":termbaseId", termbaseId));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":language", langCode));

                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    _logger.LogError("DB Errors.log", "RemoveTerms: " + e + "\ntermbaseId: ");
                    throw;
                }
            }
        }

        public void DeleteTBEntry(int termbaseId, int entryId)
        {
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "Delete from termbaseEntries where termbaseId = :termbaseId and id = :entryId";
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":termbaseId", termbaseId));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":entryId", entryId));

                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    _logger.LogError("DeleteTBEntry -> termbaseId: " + termbaseId + " error: " + e);
                    throw;
                }
            }
        }

        public int InsertTBEntry(int termbaseId, string metadata)
        {
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = @"INSERT INTO termbaseEntries (termbaseId, metadata) 
                        VALUES (:termbaseId, :metadata);";
                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":termbaseId", termbaseId));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":metadata", metadata));

                    sqlCommand.ExecuteNonQuery();
                    return (int)sqlConnection.LastInsertRowId;
                }
                catch (Exception e)
                {
                    _logger.LogError("InsertTBEntry -> termbaseId: " + termbaseId + " error: " + e.Message);
                    throw;
                }
            }
        }

        public int InsertTerm(int entryId, KeyValuePair<String, String> term, string metadata)
        {
            using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            {
                try
                {
                    //open connection
                    sqlConnection.Open();
                    var sqlCommand = new SQLiteCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = @"DELETE FROM termbaseTerms WHERE entryId = :entryId AND language = :language;
                           INSERT INTO termbaseTerms (entryId, language, term, metadata) 
                           VALUES (:entryId, :language, :term, :metadata);";

                    sqlCommand.CommandType = CommandType.Text;

                    //set the query params
                    sqlCommand.Parameters.Add(new SQLiteParameter(":entryId", entryId));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":language", term.Key));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":term", term.Value));
                    sqlCommand.Parameters.Add(new SQLiteParameter(":metadata", metadata));

                    sqlCommand.ExecuteNonQuery();
                    return (int)sqlConnection.LastInsertRowId;
                }
                catch (Exception e)
                {
                    _logger.LogError("InsertTBEntry -> entryId: " + entryId + " error: " + e.Message);
                    throw;
                }
            }
        }

        public DataSet GetTBEntry(int entryId)
        {
            //using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select * from termbaseEntries where id=@entryId";
            //        //set the query params
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@entryId", entryId));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (Exception e)
            //    {
            //        _logger.LogError("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
            //        throw e;
            //    }
            //}

            return null!;
        }

        public DataSet ListTBEntries(int termbaseId, String[] languages)
        {
            //using (var sqlConnection = new SQLiteConnection(_termbasesConnectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select termbaseEntries.id, termbaseEntries.dateCreated, termbaseEntries.dateModified, termbaseEntries.comment, termbaseEntries.createdBy, termbaseEntries.modifiedBy, termbaseTerms.language, termbaseTerms.term from Termbases inner join termbaseEntries on Termbases.id = termbaseEntries.termbaseId inner join termbaseTerms on termbaseTerms.entryId = termbaseEntries.id where Termbases.id=:termbaseId";
            //        if (languages?.Length > 0)
            //        {
            //            languages = Array.ConvertAll(languages, lang => "'" + lang + "'");
            //            sqlCommand.CommandText += " and language in (" + String.Join(",", languages) + ")";
            //        }
            //        sqlCommand.CommandType = CommandType.Text;

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SQLiteParameter(":termbaseId", termbaseId));

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (Exception e)
            //    {
            //        _logger.LogError("DB Errors.log", "GetCommentsForTranslationUnit: " + e);
            //        throw e;
            //    }
            //}

            return null!;
        }

        public void UpdateTMEntry(String tmId, int entryId, Dictionary<String, String> fieldsToUpdate)
        {
            //var dbParams = GetDBParams(tmId);
            //var connectionString = String.Format(_translationMemoriesConnectionString, dbParams.dbName);
            //using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        //open connection
            //        sqlConnection.Open();
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Update [" + dbParams.tmTableName + "] set target=@target, targetHash=@targetHash, dateModified=@dateModified, modifiedBy=@modifiedBy where id=@entryId";
            //        //sqlCommand.CommandText = "InsertTMEntry";
            //        sqlCommand.CommandType = CommandType.Text;

            //        //for now we update only the user and the target
            //        var user = fieldsToUpdate["user"];
            //        var target = CATUtils.TmxSegmentToTextFragmentSimple(fieldsToUpdate["target"]).GetCodedText().Trim();

            //        //set the query params
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@target", target));
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@targetHash", CATUtils.djb2hash(target)));
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@dateModified", DateTime.Now));
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@modifiedBy", user));
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@entryId", entryId));

            //        sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "InsertTMEntry: " + ex);
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT name FROM master.sys.databases WHERE database_id > 4";
            //        sqlCommand.CommandType = CommandType.Text;

            //        DataSet ds = new DataSet();
            //        SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
            //        sqlAdapter.Fill(ds);
            //        return ds;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "GetDbList: " + ex);
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT name FROM master.sys.databases WHERE database_id > 4";
            //        sqlCommand.CommandType = CommandType.Text;

            //        DataSet ds = new DataSet();
            //        SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
            //        sqlAdapter.Fill(ds);
            //        return ds;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "GetDbList: " + ex);
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select sysobjects.name tmName, sysindexes.rows entryNum from sysobjects inner join sysindexes on sysindexes.id = sysobjects.id and sysindexes.indid in (0,1) where sysobjects.xtype = 'U' ";
            //        sqlCommand.CommandType = CommandType.Text;

            //        DataSet ds = new DataSet();
            //        SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
            //        sqlAdapter.Fill(ds);
            //        return ds;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "GetDbList: " + ex);
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandType = CommandType.Text;
            //        sqlCommand.CommandText = "Backup database @dbName to disk=@backupPath WITH COMPRESSION, INIT";
            //        //sqlCommand.CommandText = "Backup database @dbName to disk=@backupPath"; //for SQL express

            //        var backupPath = Path.Combine(WebConfigurationManager.AppSettings["SQLBackupFolder"], dbName + ".bak");
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@dbName", dbName));
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@backupPath", backupPath));

            //        sqlCommand.ExecuteNonQuery();
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError("DB Errors.log", "BackupDatabase: " + dbName + "\n" + ex);
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "Select specialityId, description from TR_Specialities";
            //        sqlCommand.CommandType = CommandType.Text;

            //        SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
            //        DataSet ds = new DataSet();
            //        adpt.Fill(ds);

            //        return ds;
            //    }
            //    catch (Exception e)
            //    {
            //        _logger.LogError("DB Errors.log", "GetUserFullNameTask: " + e);
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
            //        var sqlCommand = new SQLiteCommand();
            //        sqlCommand.Connection = sqlConnection;
            //        sqlCommand.CommandText = "SELECT TOP(1) value FROM [_SystemSettings] WHERE name = @SettingName";
            //        sqlCommand.Parameters.Add(new SQLiteParameter("@SettingName", "Shrink_DB_Timeout"));
            //        var value = (string) sqlCommand.ExecuteScalar();
            //        if(!String.IsNullOrEmpty(value) )
            //        {
            //            retval = Convert.ToInt32(value);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        _logger.LogError("DB Errors.log", "GetShrinkDbTimeout: " + e);
            //        //throw e;
            //    }
            //}
            //return retval;

            return 0;
        }
        #endregion
    }
}