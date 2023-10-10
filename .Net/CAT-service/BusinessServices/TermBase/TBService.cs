using cat.utils;
using CAT.BusinessServices;
using CAT.Enums;
using CAT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Transactions;

namespace CAT.TB
{
    public class TBService
    {
        private IDataStorage _dataStorage = default!;
        //private static BackupConnector backupClient = new BackupConnector(); no backup

        /// <summary>
        /// ValidateLanguages
        /// </summary>
        /// <param name="aLanguages"></param>
        /// <returns></returns>
        private bool ValidateLanguages(String[] aLanguages)
        {
            var tmpLangCodes = Array.ConvertAll(Constants.LanguageCodes.Keys.ToArray(), langCode => langCode.ToLower());
            return aLanguages.All(aLang => tmpLangCodes.Contains(aLang));
        }

        /// <summary>
        /// GetTBInfo
        /// </summary>
        /// <param name="tbType"></param>
        /// <param name="idType"></param>
        /// <returns></returns>
        public TBInfo GetTBInfo(TBType tbType, int idType)
        {
            var dsTermbase = _dataStorage.GetTBInfo((int)tbType, idType);

            var tbInfo = new TBInfo();
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                return null!;

            var tbRow = dsTermbase?.Tables[0]?.Rows[0];
            tbInfo.id = (int)tbRow!["id"];
            tbInfo.languages = tbRow["languages"].ToString().Split(',');

            //the metadata
            var metadata = new Dictionary<String, String>();
            metadata.Add("tbType", tbRow["tbType"].ToString());
            metadata.Add("idType", tbRow["idType"].ToString());
            metadata.Add("dateCreated", tbRow["dateCreated"].ToString());
            metadata.Add("dateUpdated", tbRow["dateUpdated"].ToString());
            tbInfo.metadata = JsonConvert.SerializeObject(metadata);

            return tbInfo;
        }

        /// <summary>
        /// GetTBInfoById
        /// </summary>
        /// <param name="idTermbase"></param>
        /// <returns></returns>
        public TBInfo GetTBInfoById(int idTermbase)
        {
            var dsTermbase = _dataStorage.GetTBInfoById(idTermbase);

            var tbInfo = new TBInfo();
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                return null;

            var tbRow = dsTermbase?.Tables[0]?.Rows[0];
            tbInfo.id = (int)tbRow["id"];
            tbInfo.languages = tbRow["languages"].ToString().Split(',');

            //the metadata
            var metadata = new Dictionary<String, String>();
            metadata.Add("tbType", tbRow["tbType"].ToString());
            metadata.Add("idType", tbRow["idType"].ToString());
            metadata.Add("dateCreated", tbRow["dateCreated"].ToString());
            metadata.Add("dateUpdated", tbRow["dateUpdated"].ToString());
            tbInfo.metadata = JsonConvert.SerializeObject(metadata);

            return tbInfo;
        }

        public TBInfo CreateTB(TBType tbType, int idType, string[] langCodes)
        {
            langCodes = Array.ConvertAll(langCodes, lang => lang.ToLower());
            //check the languages
            if (!ValidateLanguages(langCodes))
                throw new Exception("Invalid language(s).");
            var tbInfo = GetTBInfo(tbType, idType);
            if (tbInfo != null)
            {
                //update the languages
                var tbLanguages = new HashSet<String>(tbInfo.languages);
                foreach (var lang in langCodes)
                {
                    if (!tbLanguages.Contains(lang))
                        tbLanguages.Add(lang);
                }
                _dataStorage.UpdateLanguages(tbInfo.id, tbLanguages.ToArray());
                tbInfo.languages = tbLanguages.ToArray();

                return tbInfo;
            }
            var id = _dataStorage.CreateTB((int)tbType, idType, langCodes);
            tbInfo = new TBInfo() { id = id, languages = langCodes, metadata = "" };

            //backup
            //backupClient.CreateTB(tbType, idType, langCodes);

            return tbInfo;
        }

        public void AddLanguageToTB(int idTermbase, String langCode)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");
            //get the languages
            var languages = dsTermbase?.Tables[0]?.Rows[0]["languages"].ToString();
            var tbLanguages = new HashSet<String>(languages.Split(','));

            //use transaction
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };
            TransactionScope TUTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);

            using (TUTransaction)
            {
                if (!tbLanguages.Contains(langCode))
                {
                    tbLanguages.Add(langCode);
                    //update the languages and the last update time
                    if (languages != String.Join(",", tbLanguages))
                        _dataStorage.UpdateLanguages(idTermbase, tbLanguages.ToArray());
                    _dataStorage.UpdateLastModified(idTermbase);

                    TUTransaction.Complete();

                    //backup
                    //backupClient.AddLanguageToTB(idTermbase, langCode);
                }
            }
        }

        public void RemoveLanguageFromTB(int idTermbase, String langCode)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");
            //get the languages
            var languages = dsTermbase?.Tables[0]?.Rows[0]["languages"].ToString();
            var tbLanguages = new HashSet<String>(languages.Split(','));

            //use transaction
            var transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            transactionOptions.Timeout = TransactionManager.MaximumTimeout;
            TransactionScope TUTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);

            using (TUTransaction)
            {
                if (tbLanguages.Contains(langCode))
                {
                    //delete the terms for the language
                    _dataStorage.RemoveTerms(idTermbase, langCode);

                    tbLanguages.Remove(langCode);
                    //update the languages and the last update time
                    if (languages != String.Join(",", tbLanguages))
                        _dataStorage.UpdateLanguages(idTermbase, tbLanguages.ToArray());
                    _dataStorage.UpdateLastModified(idTermbase);

                    TUTransaction.Complete();

                    //backup
                    //backupClient.AddLanguageToTB(idTermbase, langCode);
                }
            }
        }

        public int AddOrUpdateTBEntry(int idTermbase, TBEntry tbEntry)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");
            //get the languages
            var languages = dsTermbase?.Tables[0]?.Rows[0]["languages"].ToString();
            var tbLanguages = new HashSet<String>(languages.Split(','));

            //use transaction
            var transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            transactionOptions.Timeout = TransactionManager.MaximumTimeout;
            TransactionScope TUTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);

            int idEntry = tbEntry.id;
            using (TUTransaction)
            {
                if (idEntry <= 0)
                {
                    //create entry
                    idEntry = _dataStorage.InsertTBEntry(idTermbase, tbEntry.comment, tbEntry.user);

                    //insert the terms
                    foreach (var term in tbEntry.terms)
                    {
                        if (String.IsNullOrEmpty(term.Value))
                            continue;
                        var lang = term.Key.ToLower();
                        _dataStorage.InsertTerm(idEntry, term, tbEntry.user);
                        if (!tbLanguages.Contains(lang))
                            tbLanguages.Add(lang);
                    }
                }
                else
                {
                    //check if the entry exists
                    var dsEntry = _dataStorage.GetTBEntry(idEntry);
                    if (dsEntry.Tables[0].Rows.Count == 0)
                        idEntry = _dataStorage.InsertTBEntry(idTermbase, tbEntry.comment, tbEntry.user);

                    //update terms
                    foreach (var term in tbEntry.terms)
                    {
                        if (String.IsNullOrEmpty(term.Value))
                            continue;
                        var lang = term.Key.ToLower();
                        _dataStorage.InsertTerm(idEntry, term, tbEntry.user);
                        if (!tbLanguages.Contains(lang))
                            tbLanguages.Add(lang);
                    }
                }

                //update the languages and the last update time
                if (languages != String.Join(",", tbLanguages))
                    _dataStorage.UpdateLanguages(idTermbase, tbLanguages.ToArray());
                _dataStorage.UpdateLastModified(idTermbase);

                TUTransaction.Complete();

                //backup
                //backupClient.AddOrUpdateTBEntry(idTermbase, tbEntry);
            }

            return idEntry;
        }

        public void DeleteTBEntry(int idTermbase, int idEntry)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");

            //use transaction
            var transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            transactionOptions.Timeout = TransactionManager.MaximumTimeout;
            TransactionScope TUTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);

            using (TUTransaction)
            {
                //delete the entry
                _dataStorage.DeleteTBEntry(idTermbase, idEntry);
                //last modified
                _dataStorage.UpdateLastModified(idTermbase);

                TUTransaction.Complete();

                //backup
                //backupClient.DeleteTBEntry(idTermbase, tbEntry);
            }
        }

        public TBImportResult ImportTBEntries(int idTermbase, TBEntry[] tbEntries)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");
            //get the languages
            var languages = dsTermbase?.Tables[0]?.Rows[0]["languages"].ToString();
            var tbLanguages = new HashSet<String>(languages.Split(','));

            var importResult = new TBImportResult();
            var batchLimit = 500;
            int cntr = 0;
            while (cntr < tbEntries.Length)
            {
                //use transaction
                var transactionOptions = new TransactionOptions();
                transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                transactionOptions.Timeout = TransactionManager.MaximumTimeout;
                TransactionScope TUTransaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);

                using (TUTransaction)
                {
                    int batchCntr = 0;
                    while (cntr < tbEntries.Length)
                    {
                        var tbEntry = tbEntries[cntr];
                        int idEntry = tbEntry.id;
                        if (idEntry <= 0)
                        {
                            //create entry
                            idEntry = _dataStorage.InsertTBEntry(idTermbase, tbEntry.comment, tbEntry.user);
                            importResult.newItems++;

                            //insert the terms
                            foreach (var term in tbEntry.terms)
                            {
                                if (String.IsNullOrEmpty(term.Value))
                                    continue;
                                var lang = term.Key.ToLower();
                                _dataStorage.InsertTerm(idEntry, term, tbEntry.user);
                                if (!tbLanguages.Contains(lang))
                                    tbLanguages.Add(lang);
                            }
                        }
                        else
                        {
                            //check if the entry exists
                            var dsEntry = _dataStorage.GetTBEntry(idEntry);
                            if (dsEntry.Tables[0].Rows.Count == 0 || idTermbase != (int)dsEntry.Tables[0].Rows[0]["idTermbase"])
                            {
                                idEntry = _dataStorage.InsertTBEntry(idTermbase, tbEntry.comment, tbEntry.user);
                                importResult.newItems++;
                            }

                            //update terms
                            foreach (var term in tbEntry.terms)
                            {
                                if (String.IsNullOrEmpty(term.Value))
                                    continue;
                                var lang = term.Key.ToLower();
                                _dataStorage.InsertTerm(idEntry, term, tbEntry.user);
                                if (!tbLanguages.Contains(lang))
                                    tbLanguages.Add(lang);
                            }
                        }

                        importResult.allItems++;
                        cntr++;
                        batchCntr++;

                        if (batchCntr > batchLimit)
                            break;
                    }

                    //update the languages and the last update time
                    if (languages != String.Join(",", tbLanguages))
                        _dataStorage.UpdateLanguages(idTermbase, tbLanguages.ToArray());
                    _dataStorage.UpdateLastModified(idTermbase);

                    TUTransaction.Complete();
                }
            }

            //backup
            //backupClient.ImportTBEntries(idTermbase, tbEntries);

            return importResult;
        }

        public TBEntry[] ListTBEntries(int idTermbase, String[] languages)
        {
            if (languages != null)
                languages = Array.ConvertAll(languages, lang => lang.ToLower());
            //get the entry rows
            var dsEntries = _dataStorage.ListTBEntries(idTermbase, languages);
            var tbRows = dsEntries.Tables[0].Rows;
            var dictEntries = new Dictionary<int, TBEntry>(); //it indexes by id

            //convert to TBEntry. Fix me! Is there a faster way?
            foreach (System.Data.DataRow tbRow in tbRows)
            {
                var id = (int)tbRow["id"];
                if (dictEntries.ContainsKey(id))
                {
                    var tbEntry = dictEntries[id];
                    tbEntry.terms.Add((String)tbRow["language"], (String)tbRow["term"]);
                }
                else
                {
                    var tbEntry = new TBEntry();
                    tbEntry.id = (int)tbRow["id"];
                    tbEntry.comment = (String)tbRow["comment"];
                    //tbEntry.user = (String)tbRow["modifiedBy"];
                    tbEntry.metadata = JsonConvert.SerializeObject(new
                    {
                        createdBy = (String)tbRow["createdBy"],
                        dateCreated = (DateTime)tbRow["dateCreated"],
                        modifiedBy = (String)tbRow["modifiedBy"],
                        dateModified = (DateTime)tbRow["dateModified"],
                    });
                    tbEntry.terms = new Dictionary<String, String>();
                    tbEntry.terms.Add((String)tbRow["language"], (String)tbRow["term"]);
                    dictEntries.Add(tbEntry.id, tbEntry);
                }

            }

            return dictEntries.Values.ToArray();
        }

        public TBImportResult ImportTB(int idTermbase, String sCsvContent, String sUserId)
        {
            throw new NotImplementedException();
        }
    }
}
