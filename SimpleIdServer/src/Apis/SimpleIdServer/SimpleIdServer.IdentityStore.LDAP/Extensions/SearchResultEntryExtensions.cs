using System;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace SimpleIdServer.IdentityStore.LDAP.Extensions
{
    public static class SearchResultEntryExtensions
    {
        public static string GetString(this SearchResultEntry searchResultEntry, string name)
        {
            if (searchResultEntry.Attributes.Contains(name) == false)
            {
                return null;
            }

            return searchResultEntry.Attributes[name].GetValues(typeof(string)).Select(v => v.ToString()).First();
        }

        public static int GetInt(this SearchResultEntry searchResultEntry, string name)
        {
            if (searchResultEntry.Attributes.Contains(name) == false)
            {
                return 0;
            }

            return searchResultEntry.Attributes[name].GetValues(typeof(int)).Select(v => int.Parse(v.ToString())).First();
        }

        public static double GetDouble(this SearchResultEntry searchResultEntry, string name)
        {
            if (searchResultEntry.Attributes.Contains(name) == false)
            {
                return 0;
            }

            return searchResultEntry.Attributes[name].GetValues(typeof(double)).Select(v => double.Parse(v.ToString())).First();
        }

        public static bool GetBoolean(this SearchResultEntry searchResultEntry, string name)
        {
            if (searchResultEntry.Attributes.Contains(name) == false)
            {
                return false;
            }

            return searchResultEntry.Attributes[name].GetValues(typeof(bool)).Select(v => bool.Parse(v.ToString())).First();
        }

        public static DateTime GetDateTime(this SearchResultEntry searchResultEntry, string name)
        {
            if (searchResultEntry.Attributes.Contains(name) == false)
            {
                return default(DateTime);
            }

            return searchResultEntry.Attributes[name].GetValues(typeof(DateTime)).Select(v => DateTime.Parse(v.ToString())).First();
        }

        public static DateTime? GetNullableDateTime(this SearchResultEntry searchResultEntry, string name)
        {
            if (searchResultEntry.Attributes.Contains(name) == false)
            {
                return null;
            }

            return searchResultEntry.Attributes[name].GetValues(typeof(DateTime?)).Select(v =>
            {
                DateTime r;
                if (!DateTime.TryParse(v.ToString(), out r))
                {
                    return (DateTime?)null;
                }

                return r;
            }).First();
        }
    }
}
