using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace FirstAppNetCore
{
    public class Program
    {
        public string EndpointUrl = "https://iotnewspaper.documents.azure.com:443/";
        public string PrimaryKey = "EIhinHob5k8ihOGTENdJq0HaZ6eTnzmGXiuFHywjB5IZp6CQ1CTRdF1O4EXxds1ovFmCfLdA1xllm270YpX4Qg==";
        static DocumentClient client;



        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
