using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace nMapper
{
    public static class NamingConvention
    {
        static NamingConvention()
        {
            ObjectNamingConvention = NamingConventions.PascalCase;
        }

        private enum NamingConventions
        {
            UnderScoreCase = 1,
            PascalCase = 2
        }

        #region Properties
        private static NamingConventions DbNamingConvention { get; set; }

        private static NamingConventions ObjectNamingConvention { get; set; }
        #endregion

        #region Public Function
        /// <summary>
        /// Set database naming convention. Only two types of naming conventions are supported as of now i.e Pascal and UnderScore Casing
        /// </summary>
        /// <param name="type">1 for underscore naming convention. 2 for Pascal Naming Convention</param>
        public static void SetDataBaseNamingConvention(int type)
        {

            if (type > 2 || type < 1)
            {
                throw new Exception("Value cannot be more than 2 and less than 1 as of now.");
            }
            else
            {
                switch (type)
                {
                    case 1:
                        DbNamingConvention = NamingConventions.UnderScoreCase;
                        break;
                    case 2:
                        DbNamingConvention = NamingConventions.PascalCase;
                        break;
                }
            }

        }

        public static void SetObjectNamingConvention(int type)
        {
            if (type > 2 || type < 1)
            {
                throw new Exception("Value cannot be more than 2 and less than 1 as of now.");
            }
            switch (type)
            {
                case 1:
                    ObjectNamingConvention = NamingConventions.UnderScoreCase;
                    break;
                case 2:
                    ObjectNamingConvention = NamingConventions.PascalCase;
                    break;
            }
        }

        public static string ProcessColumnNamesToObjectNames(string columnName)
        {
            IList<string> rawNameList;
            switch (DbNamingConvention)
            {
                case NamingConventions.UnderScoreCase:
                    rawNameList = SplitUnderScoreNamingConvention(columnName);
                    break;
                case NamingConventions.PascalCase:
                    rawNameList = SplitPascalNamingConvention(columnName);
                    break;
                default:
                    return null;
            }
            return GetObjectNamesFromRawList(rawNameList);
        }

        public static string ProcessObjectNamesToColumnNames(string columnName)
        {
            IEnumerable<string> rawNameList;
            switch (ObjectNamingConvention)
            {
                case NamingConventions.UnderScoreCase:
                    rawNameList = SplitUnderScoreNamingConvention(columnName);
                    break;
                case NamingConventions.PascalCase:
                    rawNameList = SplitPascalNamingConvention(columnName);
                    break;
                default:
                    return null;
            }
            return GetColumnNamesFromRawList(rawNameList);
        }
        #endregion

        private static string GetObjectNamesFromRawList(IList<string> rawNameList)
        {
            var nameList = rawNameList as IList<string> ?? rawNameList.ToList();
            switch (ObjectNamingConvention)
            {
                case NamingConventions.PascalCase:
                    var strList = nameList.Select(splitedName => splitedName.ToFirstLetterUpper());
                    return string.Join("", strList);
                case NamingConventions.UnderScoreCase:
                    return string.Join("_", nameList);
            }

            return null;
        }

        private static string GetColumnNamesFromRawList(IEnumerable<string> rawNameList)
        {
            var nameList = rawNameList as IList<string> ?? rawNameList.ToList();
            switch (DbNamingConvention)
            {
                case NamingConventions.UnderScoreCase:
                    return string.Join("_", nameList);
                case NamingConventions.PascalCase:
                    var tempList = nameList.Select(name => name.ToFirstLetterUpper()).ToList();
                    return string.Join("", tempList);
                default:
                    return null;
            }
        }

        #region Extensions
        private static string ToFirstLetterUpper(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var letters = value.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }
        #endregion

        #region SplitStringsRegion
        private static IList<string> SplitUnderScoreNamingConvention(string name)
        {
            return name.ToLower().Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static IList<string> SplitPascalNamingConvention(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var newText = new StringBuilder(name.Length * 2);
            newText.Append(name[0]);
            for (var i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && name[i - 1] != ' ')
                {
                    newText.Append(' ');
                }
                newText.Append(name[i]);
            }
            return newText.ToString().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
        #endregion

    }
}
