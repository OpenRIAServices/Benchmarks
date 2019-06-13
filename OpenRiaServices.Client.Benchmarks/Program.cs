using BenchmarkDotNet.Running;
using ClientBenchmarks.Server;
using System.Threading.Tasks;

namespace ClientBenchmarks
{
    internal class Program
    {
        static bool onlyProfiling = false;

        private static void Main(string[] args)
        {
            if (onlyProfiling)
            {
                Task.Run(() => RunBenchmarks()).GetAwaiter().GetResult();
                return;

                const int num = 1;
                for (int i = 0; i < 1; ++i)
                {
                    var a = new LoadBenchmarks();
                    a.NumEntities = num;
                    a.LoadEntities();
                    a.LoadAndRefreshEntities();
                    a.LoadAndMergeEntities();

                    var s = new ChangeSetBenchmarks();
                    s.NumEntities = num;
                    s.SubmitAdded();

                    var es = new EntitySetBenchmarks();
                    es.NumEntities = num;
                    es.Add();
                    es.AddAndDetach();
                    es.AddAndRemove();
                    es.Attach();
                    es.AttachAndDetach();
                    es.AttachAndModify();
                    es.AttachAndRemove();
                }
            }
            else
            {
                BenchmarkRunner.Run<E2Ebenchmarks>();
                //BenchmarkRunner.Run<EntityBenchmarks>();
                //BenchmarkRunner.Run<EntitySetBenchmarks>();
                //BenchmarkRunner.Run<ChangeSetBenchmarks>();
                //BenchmarkRunner.Run<LoadBenchmarks>();
            }
        }

        private static async Task RunBenchmarks()
        {
            var b = new E2Ebenchmarks();
            b.Start();

            for (int i = 0; i < 10000; ++i)
            {
                await b.GetCititesReuseContext();
                //Console.WriteLine("Got {0} entities", res.Count);
            }

            b.Stop();
        }
    }
}
