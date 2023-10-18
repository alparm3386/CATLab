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
    public class TBService : ITBService
    {
        private readonly IDataStorage _dataStorage = default!;
        //private static BackupConnector backupClient = new BackupConnector(); no backup

        public TBService(IDataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }

        /// <summary>
        /// ValidateLanguages
        /// </summary>
        /// <param name="aLanguages"></param>
        /// <returns></returns>
        private static bool ValidateLanguages(string[] aLanguages)
        {
            var tmpLangCodes = Array.ConvertAll(Constants.LanguageCodes.Keys.ToArray(), langCode => langCode.ToLower());
            return aLanguages.All(aLang => tmpLangCodes.Contains(aLang));
        }

        /// <summary>
        /// CreateTB
        /// </summary>
        /// <param name="tbType"></param>
        /// <param name="idType"></param>
        /// <param name="langCodes"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
                tbLanguages.UnionWith(langCodes);

                _dataStorage.UpdateLanguages(tbInfo.id, tbLanguages.ToArray());
                tbInfo.languages = tbLanguages.ToArray();

                return tbInfo;
            }
            var id = _dataStorage.CreateTB((int)tbType, idType, langCodes);
            tbInfo = new TBInfo() { id = id, languages = langCodes, metadata = "" };

            return tbInfo;
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
            tbInfo.id = (int)(long)tbRow!["id"];
            tbInfo.languages = tbRow["languages"].ToString()!.Split(',');

            //the metadata
            var metadata = new Dictionary<String, String>
            {
                { "tbType", tbRow["tbType"]!.ToString()! },
                { "idType", tbRow["idType"].ToString()! },
                { "dateCreated", tbRow["dateCreated"].ToString()! },
                { "dateUpdated", tbRow["dateUpdated"].ToString()! }
            };
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
                return null!;

            var tbRow = dsTermbase?.Tables[0]?.Rows[0];
            tbInfo.id = (int)(long)tbRow!["id"];
            tbInfo.languages = tbRow["languages"].ToString()!.Split(',');

            //the metadata
            var metadata = new Dictionary<String, String>
            {
                { "tbType", tbRow["tbType"].ToString()! },
                { "idType", tbRow["idType"].ToString()! },
                { "dateCreated", tbRow["dateCreated"].ToString()! },
                { "dateUpdated", tbRow["dateUpdated"].ToString()! }
            };
            tbInfo.metadata = JsonConvert.SerializeObject(metadata);

            return tbInfo;
        }

        public void AddLanguageToTB(int idTermbase, String langCode)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");
            //get the languages
            var languages = dsTermbase?.Tables[0]?.Rows[0]["languages"].ToString();
            var tbLanguages = new HashSet<String>(languages!.Split(','));

            if (!tbLanguages.Contains(langCode))
            {
                tbLanguages.Add(langCode);
                //update the languages and the last update time
                if (languages != String.Join(",", tbLanguages))
                    _dataStorage.UpdateLanguages(idTermbase, tbLanguages.ToArray());
                _dataStorage.UpdateLastModified(idTermbase);
            }
        }

        public void RemoveLanguageFromTB(int idTermbase, String langCode)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");
            //get the languages
            var languages = dsTermbase?.Tables[0]?.Rows[0]["languages"].ToString();
            var tbLanguages = new HashSet<String>(languages!.Split(','));

            if (tbLanguages.Contains(langCode))
            {
                //delete the terms for the language
                _dataStorage.RemoveTerms(idTermbase, langCode);

                tbLanguages.Remove(langCode);
                //update the languages and the last update time
                if (languages != String.Join(",", tbLanguages))
                    _dataStorage.UpdateLanguages(idTermbase, tbLanguages.ToArray());
                _dataStorage.UpdateLastModified(idTermbase);
            }
        }

        public int AddOrUpdateTBEntry(int idTermbase, TBEntry tbEntry)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");
            //get the languages
            var languages = dsTermbase?.Tables[0]?.Rows[0]["languages"].ToString();
            var tbLanguages = new HashSet<String>(languages!.Split(','));

            int idEntry = tbEntry.id;
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

            return idEntry;
        }

        public void DeleteTBEntry(int idTermbase, int idEntry)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");

            //delete the entry
            _dataStorage.DeleteTBEntry(idTermbase, idEntry);
            //last modified
            _dataStorage.UpdateLastModified(idTermbase);
        }

        public TBImportResult ImportTBEntries(int idTermbase, TBEntry[] tbEntries)
        {
            var dsTermbase = _dataStorage.GetTBInfo(idTermbase);
            if (dsTermbase?.Tables[0]?.Rows.Count != 1)
                throw new Exception("Termbase not found.");
            //get the languages
            var languages = dsTermbase?.Tables[0]?.Rows[0]["languages"].ToString();
            var tbLanguages = new HashSet<String>(languages!.Split(','));

            var importResult = new TBImportResult();
            var batchLimit = 500;
            int cntr = 0;
            while (cntr < tbEntries.Length)
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

            }

            return importResult;
        }

        public TBEntry[] ListTBEntries(int idTermbase, String[] languages)
        {
            if (languages != null)
                languages = Array.ConvertAll(languages, lang => lang.ToLower());
            //get the entry rows
            var dsEntries = _dataStorage.ListTBEntries(idTermbase, languages!);
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
                    var tbEntry = new TBEntry
                    {
                        id = (int)tbRow["id"],
                        comment = (String)tbRow["comment"],
                        //tbEntry.user = (String)tbRow["modifiedBy"];
                        metadata = JsonConvert.SerializeObject(new
                        {
                            createdBy = (String)tbRow["createdBy"],
                            dateCreated = (DateTime)tbRow["dateCreated"],
                            modifiedBy = (String)tbRow["modifiedBy"],
                            dateModified = (DateTime)tbRow["dateModified"],
                        }),
                        terms = new Dictionary<String, String>
                        {
                            { (String)tbRow["language"], (String)tbRow["term"] }
                        }
                    };
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
