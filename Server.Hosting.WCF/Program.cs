using System;
using System.ServiceModel.Activation;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using OpenRiaServices.Hosting.Wcf;

namespace Server.Hosting.WCF
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This is using 'raw DomainServiceHost' WCF hosting using, which is not really supported by OpenRia");

            Type type = typeof(CityDomainService);
            Uri uri = new Uri("http://localhost/Temporary_Listen_Addresses/Cities/Services/Citites.svc")
            using (var host = new DomainServiceHost(type, uri))
            {
                //other relevent code to configure host's end point etc
                if (host.Description.Behaviors.Contains(typeof(AspNetCompatibilityRequirementsAttribute)))
                {
                    var compatibilityRequirementsAttribute = host.Description.Behaviors[typeof(AspNetCompatibilityRequirementsAttribute)] as AspNetCompatibilityRequirementsAttribute;
                    compatibilityRequirementsAttribute.RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed;
                }

                host.Open();

                Console.WriteLine($"DomainService {type.Name} running at {uri}");
                Console.WriteLine("Press ENTER to exit");
                Console.ReadLine();
                host.Close();
            }

        }
    }
}