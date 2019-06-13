using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using System.ComponentModel;
using System.Reflection;

namespace ClientBenchmarks
{
    [LegacyJitX86Job, RyuJitX64Job]
    [MemoryDiagnoser]
    public class InvokeBenchmarks
    {
        City _city = new City() { CountyName = "Dummy" };

        [Params(10, 50, 100)]
        public int NumInvocations { get; set; } = 1;

        [Benchmark()]
        public object Reflection()
        {
            var propertyInfo = typeof(City).GetProperty(nameof(City.CountyName));

            object result = null;
            for (int i = 0; i < NumInvocations; ++i)
            {
                result = propertyInfo.GetValue(_city, null);
            }
            return result;
        }

        
        [Benchmark(Baseline = true)]
        public object Reflection_PropertyDescriptor()
        {
            var propertyInfo = TypeDescriptor.GetProperties(typeof(City))[nameof(City.CountyName)];

            object result = null;
            for (int i = 0; i < NumInvocations; ++i)
            {
                result = propertyInfo.GetValue(_city);
            }
            return result;
        }

        //[Benchmark] This is so slow so no reason to continue testing
        public object ExpressionCompile()
        {
            var propertyInfo = typeof(City).GetProperty(nameof(City.CountyName));
            Func<object, object> func = CreatePropertyGetter(propertyInfo);

            object result = null;
            for (int i = 0; i < NumInvocations; ++i)
            {
                result = func(_city);
            }
            return result;
        }

        //[Benchmark]
        public object DelegateInvoke()
        {
            var propertyInfo = TypeDescriptor.GetProperties(typeof(City))[nameof(City.CountyName)];
            Func<object, object> func = CreateDelegate(propertyInfo);

            object result = null;
            for (int i = 0; i < NumInvocations; ++i)
            {
                result = func(_city);
            }
            return result;
        }

        [Benchmark]
        public object MetaMember_Method()
        {
            var propertyInfo = typeof(City).GetProperty(nameof(City.CountyName));
            var member = new MetaMember(null, propertyInfo);

            object result = null;
            for (int i = 0; i < NumInvocations; ++i)
            {
                result = member.GetValue(_city);
            }
            return result;
        }

        [Benchmark]
        public object MetaMember_VirtualMethod()
        {
            var propertyInfo = typeof(City).GetProperty(nameof(City.CountyName));
            var member = MetaMember.CreateMember(null, propertyInfo);

            object result = null;
            for (int i = 0; i < NumInvocations; ++i)
            {
                result = member.GetValueVirtual(_city);
            }
            return result;
        }


        public static Func<object, object> CreatePropertyGetter(System.Reflection.PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
            {
                return x => propertyInfo.GetValue(x, null);
            }

            var type = propertyInfo.DeclaringType;

            var instance = Expression.Parameter(typeof(object));
            var typedInstance = Expression.Convert(instance, type);
            var res = Expression.Call(typedInstance, getMethod);
            var result = Expression.Convert(res, typeof(object));

            var lambda = Expression.Lambda<Func<object, object>>(result, instance);
            return lambda.Compile();
        }

        /// <summary>
        /// Helper method which creates a delegate which can be used to invoke a specific getter
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Func<object, object> CreateDelegate(PropertyDescriptor property)
        {
            PropertyInfo propertyInfo = property.ComponentType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

            if (propertyInfo == null)
                return (object x) => property.GetValue(x);

            return CreateDelegate(propertyInfo);
        }

        public static Func<object, object> CreateDelegate(PropertyInfo propertyInfo)
        {
            var helperMethod = typeof(InvokeBenchmarks).GetMethod(nameof(InvokeBenchmarks.CreateDelegateHelper), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            var helper = helperMethod.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (Func<object, object>)helper.Invoke(null, new[] { propertyInfo });
        }

        private static Func<object, object> CreateDelegateHelper<T, Tres>(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
            {
                return CreateFallbackGetter(propertyInfo);
            }

            var func = (Func<T, Tres>)Delegate.CreateDelegate(typeof(Func<T, Tres>), getMethod);
            // Add a wrapper which performs boxing of the function
            Func<object, object> res = (object x) => (object)func((T)x);
            return res;
        }

        private static Func<object, object> CreateFallbackGetter(PropertyInfo propertyInfo)
        {
            return x => propertyInfo.GetValue(x, null);
        }
    }
}
