using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace RabbitMQ.Procedur
{
    public class RabbitMQC
    {
        public class setformatjson
        {
            public int id { get; set; }
            public string? actions { get; set; }
            public string? status { get; set; }
            public string? refType { get; set; }
            public string? refId { get; set; }
            public DateTimeOffset createDt { get; set; }
            public DateTimeOffset updateDt { get; set;}
        }
        public void RUN()
        {

            try
            {
                string conStr = "server=localhost;port=3306;database=ntitts;uid=root;password=;TreatTinyAsBoolean=false;Convert Zero Datetime=True";
                MySqlConnection con = new MySqlConnection(conStr);
                con.Open();
                //query in view incident_rabbitmq
                var cmd = new MySqlCommand("select id,actions,status ,refType ,refId ,date_format(createdDt,'%Y-%m-%d %H:%i:%s') as createdDt,date_format(updatedDt,'%Y-%m-%d %H:%i:%s') as updatedDt from ntitts.incident_rabbitmq", con);
                var readers = cmd.ExecuteReader();

                var datatable = new DataTable();
                datatable.Load(readers);
                var ddd = datatable.ConvertDataTable<setformatjson>();
                string jsonString = JsonConvert.SerializeObject(ddd);

                var factory = new ConnectionFactory() //cennect RabbitMG
                {
                    HostName = "localhost", // 10.44.33.5
                    Port = 5672,
                    UserName = "csm", //csm
                    Password = "P@ssw0rd", //P@ssw0rd 
                    VirtualHost = "uat.csm" //vhost:uat.csm or prod,csm
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "uat.salesforce.csm.sync", // queue name 
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = jsonString; // "Hello World!";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "uat.salesforce.csm.sync",//queue name
                                         basicProperties: null,
                                         body: body);
                     //Console.WriteLine(" [x] Sent {0}", message);
                }

            }
            catch (Exception ex)
            {

            }
            
        }
    }

}
