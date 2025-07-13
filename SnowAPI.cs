using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using portal_instadose_com_v3.Snow;
using System.IO;
using System.Web.Script.Serialization;

    public class SnowAPI
    {
        private static string applicationInstance = "https://mirion.service-now.com/api/now/";
        private static string tableTask ="sn_customerservice_task";
        private static string username = "extractionapiuser";
        private static string password = "8wz9Qr!v3hI.W@b2[J8@JJ5WoxsYLD4iBYS0R(>Y";

        public static List<SnowTask> GetSnowTask(string TaskNumber = "")
        {
            var endpoint = $"{applicationInstance}table/{tableTask}?sysparm_limit=1&number={TaskNumber}";
            var request = WebRequest.Create(endpoint);
             
            request.Credentials = new NetworkCredential(username, password);

            // Create POST data and convert it to a byte array.
            request.Method = "GET";          
           
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/json; charset=utf-8";          

            WebResponse response = request.GetResponse();
            // Display the status.

            // Get the stream containing content returned by the server.
            var dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            
            JavaScriptSerializer j = new JavaScriptSerializer();
            SnowResponse<SnowTask> dataResp  = j.Deserialize<SnowResponse<SnowTask>>(responseFromServer);       

            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            return dataResp.result;
        }

        public static void UpdateSnowTask(string SysId = "", string message = "", int newState = 1)
        {

            var endpoint = $"{applicationInstance}table/{tableTask}/{SysId}";
            var request = WebRequest.Create(endpoint);
            
            request.Credentials = new NetworkCredential(username, password);

            // Create PUT data and convert it to a byte array.
            request.Method = "PUT";      

            //var payload  = new { work_notes = message };
            SnowPayload payload  = new SnowPayload() { comments = message, state = newState };
            string json = new JavaScriptSerializer().Serialize(payload);

            request.ContentType = "application/json";
            request.ContentLength = json.Length;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            WebResponse response = request.GetResponse();

            // Get the stream containing content returned by the server.
            var dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

        }

    }