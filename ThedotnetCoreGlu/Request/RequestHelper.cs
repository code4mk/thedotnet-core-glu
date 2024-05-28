using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ThedotnetCoreGlu.Request
{
    public static class RequestHelper
    {
        public static async Task<IDictionary<string, object>> the_query(HttpRequest request)
        {
            var data = new Dictionary<string, object>();

            if (request.Query.Count > 0)
            {
                foreach (var (key, value) in request.Query)
                {
                    if (value.Count == 1)
                    {
                        data[key] = value.ToString();
                    }
                    else
                    {
                        data[key] = value.ToArray();
                    }
                }
            }

            if (request.ContentType != null && request.ContentType.StartsWith("application/json"))
            {
                using (var reader = new StreamReader(request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                    foreach (var (key, innerValue) in json)
                    {
                        if (innerValue is string stringValue)
                        {
                            data[key] = stringValue;
                        }
                        else if (innerValue is Newtonsoft.Json.Linq.JArray arrayValue)
                        {
                            data[key] = arrayValue.ToObject<string[]>();
                        }
                        else
                        {
                            Console.WriteLine($"Unsupported value type for key '{key}': {innerValue}");
                        }
                    }
                }
            }
            else if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                foreach (var (key, value) in form)
                {
                    if (!data.ContainsKey(key))
                    {
                        if (value.Count == 1)
                        {
                            data[key] = value.ToString();
                        }
                        else
                        {
                            data[key] = value.ToArray();
                        }
                    }
                    else
                    {
                        var currentValues = (List<string>)data[key];
                        currentValues.AddRange(value.Select(v => v.ToString()));
                    }
                }

                foreach (var file in form.Files)
                {
                    Console.WriteLine($"File key: {file.FileName}");

                    if (data.ContainsKey(file.Name))
                    {
                        if (data[file.Name] is List<IFormFile> fileList)
                        {
                            fileList.Add(file);
                        }
                        else
                        {
                            data[file.Name] = new List<IFormFile> { file };
                        }
                    }
                    else
                    {
                        data[file.Name] = new List<IFormFile> { file };
                    }
                }
            }

            return data;
        }
    }
}
