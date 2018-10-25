using Microsoft.AspNetCore.Http;
using SimpleIdServer.Lib;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace SimpleIdServer.Uma.Host.Extensions
{
    internal static class ParamSerializerExtensions
    {
        public static object Deserialize(this ParamSerializer paramSerializer, NameValueCollection input)
        {
            return paramSerializer.Deserialize(ConvertNameValueCollection(input));
        }

        public static T Deserialize<T>(this ParamSerializer paramSerializer, NameValueCollection input)
        {
            return paramSerializer.Deserialize<T>(ConvertNameValueCollection(input));
        }

        public static T Deserialize<T>(this ParamSerializer paramSerializer, IFormCollection form)
        {
            return paramSerializer.Deserialize<T>(ConvertNameValueCollection(form));
        }

        public static T Deserialize<T>(this ParamSerializer paramSerializer, IQueryCollection query)
        {
            return paramSerializer.Deserialize<T>(ConvertNameValueCollection(query));
        }

        public static string ConvertNameValueCollection(NameValueCollection input)
        {
            var output = new StringBuilder();
            foreach (var key in input.AllKeys)
            {
                var values = input.GetValues(key) ?? new string[] { };
                foreach (var value in values)
                {
                    output.AppendFormat("{0}={1}&", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value));
                }
            }

            return output.ToString().TrimEnd(new[] { '&' });
        }

        public static string ConvertNameValueCollection(IFormCollection form)
        {
            var output = new StringBuilder();
            foreach (var key in form.Keys)
            {
                var values = form[key];
                foreach (var value in values)
                {
                    output.AppendFormat("{0}={1}&", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value));
                }
            }

            return output.ToString().TrimEnd(new[] { '&' });
        }

        public static string ConvertNameValueCollection(IQueryCollection query)
        {
            var output = new StringBuilder();
            foreach (var key in query.Keys)
            {
                var values = query[key];
                foreach (var value in values)
                {
                    output.AppendFormat("{0}={1}&", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value));
                }
            }

            return output.ToString().TrimEnd(new[] { '&' });
        }
    }
}
