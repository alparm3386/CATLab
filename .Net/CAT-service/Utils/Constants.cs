﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace cat.utils
{
    public static class Constants
    {
        public static readonly Dictionary<String, String> LANGUAGE_CODES = new Dictionary<String, String>() {
              {"afr", "af"},
            {"alb", "sq"},
            {"amh", "am"},
            {"ara", "ar"},
            {"ara-DZ", "ar-dz"},
            {"ara-BH", "ar-bh"},
            {"ara-EG", "ar-eg"},
            {"ara-IQ", "ar-iq"},
            {"ara-JO", "ar-jo"},
            {"ara-KW", "ar-kw"},
            {"ara-LB", "ar-lb"},
            {"ara-LY", "ar-ly"},
            {"ara-MA", "ar-ma"},
            {"ara-QA", "ar-qa"},
            {"ara-SA", "ar-SA"},
            {"ara-SY", "ar-sy"},
            {"ara-TN", "ar-tn"},
            {"ara-AE", "ar-AE"},
            {"ara-YE", "ar-ye"},
            {"hye", "hy"},
            {"asm", "as"},
            {"aze", "az"},
            {"baq", "eu"},
            {"bel", "be"},
            {"ben", "bn"},
            {"ben-BD", "bn-bd"},
            {"ben-IN", "bn-in"},
            {"bis", "bi"},
            {"bos", "bs"},
            {"bul", "bg"},
            {"mya", "my"},
            {"cat", "ca"},
            {"ctd", "ctd"},
            {"zho-MO", "zh-mo"},
            {"zho-SG", "zh-sg"},
            {"zho-CN", "zh-cn"},
            {"zho-HK", "zh-hk"},
            {"zho-TW", "zh-tw"},
            {"hrv", "hr"},
            {"cze", "cs"},
            {"dan", "da"},
            {"prs", "prs-AF"},
            {"din", "din"},
            {"dut", "nl"},
            {"eng", "en"},
            {"eng-AU", "en-au"},
            {"eng-BZ", "en-bz"},
            {"eng-CA", "en-ca"},
            {"eng-CB", "en-cb"},
            {"eng-IE", "en-ie"},
            {"eng-JM", "en-jm"},
            {"eng-NZ", "en-nz"},
            {"eng-PH", "en-ph"},
            {"eng-ZA", "en-za"},
            {"eng-TT", "en-tt"},
            {"eng-US", "en-us"},
            {"epo", "eo"},
            {"est", "et"},
            {"fas", "fa"},
            {"fry", "fy"},
            {"fil", "fil"},
            {"fin", "fi"},
            {"dut-BE", "nl-BE"},
            {"fre", "fr"},
            {"fre-BE", "fr-be"},
            {"fre-CA", "fr-ca"},
            {"fre-CH", "fr-ch"},
            {"fre-MC", "fr-mc"},
            {"fre-LU", "fr-lu"},
            {"gla", "gd"},
            {"glg", "gl"},
            {"kat", "ka"},
            {"ger", "de"},
            {"ger-AT", "de-at"},
            {"ger-CH", "de-ch"},
            {"ger-LU", "de-lu"},
            {"gre", "el"},
            {"guj", "gu"},
            {"hat", "ht"},
            {"hau", "ha"},
            {"haz", "haz"},
            {"heb", "he"},
            {"hin", "hi"},
            {"hmn", "hmn"},
            {"hun", "hu"},
            {"ice", "is"},
            {"ibo", "ig"},
            {"ind", "id"},
            {"gle", "ga"},
            {"ita", "it"},
            {"ita-CH", "it-ch"},
            {"jpn", "ja"},
            {"jav", "jv"},
            {"kan", "kn"},
            {"ksw", "ksw"},
            {"kas", "ks"},
            {"kyu", "kyu"},
            {"eky", "eky"},
            {"kaz", "kk"},
            {"khm", "km"},
            {"gil", "gil"},
            {"qqq", "qqq"},
            {"kor", "ko"},
            {"kur", "kr"},
            {"kmr", "kmr"},
            {"lao", "lo"},
            {"lat", "la"},
            {"lav", "lv"},
            {"lin", "ln"},
            {"lit", "lt"},
            {"ltz", "lb"},
            {"mac", "mk"},
            {"msa", "ms"},
            {"mal", "ml"},
            {"mlt", "mt"},
            {"mno", "mno"},
            {"mnk", "mnk"},
            {"mri", "mi"},
            {"mar", "mr"},
            {"mah", "mah"},
            {"mol", "mo"},
            {"khk", "mn"},
            {"cgy", "cgy"},
            {"cgl", "cg-Latn"},
            {"nau", "nau"},
            {"nep", "ne"},
            {"nor", "no"},
            {"nnb", "nb"},
            {"nno", "nn"},
            {"ori", "or"},
            {"pbu", "ps"},
            {"pol", "pl"},
            {"por", "pt"},
            {"por-BR", "pt-br"},
            {"por-PT", "pt-pt"},
            {"pan", "pa"},
            {"pnb", "pnb"},
            {"rhg", "rhg"},
            {"rum", "ro"},
            {"run", "run"},
            {"rus", "ru"},
            {"kin", "kin"},
            {"smo", "smo"},
            {"san", "sa"},
            {"scc", "sr"},
            {"scr", "sh"},
            {"sot", "st"},
            {"shn", "shn"},
            {"sin", "si"},
            {"slo", "sk"},
            {"slv", "sl"},
            {"som", "so"},
            {"som-DJ", "so-dj"},
            {"som-ET", "so-et"},
            {"som-KE", "so-ke"},
            {"spa", "es"},
            {"spa-AR", "es-ar"},
            {"spa-BO", "es-bo"},
            {"spa-CL", "es-cl"},
            {"spa-CO", "es-co"},
            {"spa-CR", "es-cr"},
            {"spa-DO", "es-do"},
            {"spa-EC", "es-ec"},
            {"spa-SV", "es-sv"},
            {"spa-GT", "es-gt"},
            {"spa-HN", "es-hn"},
            {"spa-MX", "es-mx"},
            {"spa-NI", "es-ni"},
            {"spa-PA", "es-pa"},
            {"spa-PY", "es-py"},
            {"spa-PE", "es-pe"},
            {"spa-PR", "es-pr"},
            {"spa-UY", "es-uy"},
            {"spa-US", "es-us"},
            {"spa-VE", "es-ve"},
            {"swa", "sw"},
            {"swe", "sv"},
            {"swe-FI", "sv-fi"},
            {"tgl", "tl"},
            {"tgk", "tg"},
            {"tam", "ta"},
            {"tel", "te"},
            {"tha", "th"},
            {"tir", "ti"},
            {"tpi", "tpi"},
            {"ton", "ton"},
            {"tcs", "tcs"},
            {"tsn", "tn"},
            {"tur", "tr"},
            {"tuk", "tk"},
            {"tvl", "tvl"},
            {"twi", "twi"},
            {"ukr", "uk"},
            {"urd", "ur"},
            {"uzb", "uz"},
            {"vie", "vi"},
            {"wel", "cy"},
            {"wol", "wo"},
            {"xho", "xh"},
            {"yor", "yo"},
            {"zul", "zu"}
        };
    }
}