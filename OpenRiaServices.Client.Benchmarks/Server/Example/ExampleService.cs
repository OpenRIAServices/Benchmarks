extern alias server;

using server::OpenRiaServices.Server;
using server::OpenRiaServices.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ClientBenchmarks.Server.Example
{
    [DataContract]
    public class Example
    {
        [DataMember]
        [Key]
        public int Id { get; set; }

        [DataMember]
        public string Value { get; set; }
    }


    [EnableClientAccess]
    public class ExampelService : DomainService
    {
        private static readonly Example[] data = new Example[] { new Example { Id = 1, Value = "value1" }
            , new Example {Id= 2, Value = "value2" } };

        // GET api/<controller>
        [Query]
        //[Route("GetExamples")]
        public IQueryable<Example> GetExamples()
        {
           // await Task.Delay(5);
            return data.AsQueryable();
        }

        //// GET api/<controller>/5
        //public Example GetExample(int id)
        //{
        //    return data.FirstOrDefault(e => e.Id == id);
        //}

    }
}
