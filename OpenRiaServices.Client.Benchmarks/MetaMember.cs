using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    [DebuggerDisplay("Name = {Member.Name}")]
    internal sealed class MetaMember<T, TProp> : MetaMember
    {
        private Func<T, TProp> _getter;

        public MetaMember(MetaType metaType, PropertyInfo propertyInfo)
           : base(metaType, propertyInfo)
        {

            Initialize(metaType, propertyInfo);
        }

        public MetaMember()
           : base(null, null)
        {

        }

        public void Initialize(MetaType metaType, PropertyInfo propertyInfo)
        {
            MetaType = metaType;
            Member = propertyInfo;

            var getMethod = propertyInfo.GetGetMethod();
            _getter = (Func<T, TProp>)Delegate.CreateDelegate(typeof(Func<T, TProp>), getMethod);
        }

        public override string GetValueAsStringVirtual(object instance)
        {
            return _getter((T)instance).ToString();
        }

        public override object GetValueVirtual(object instance)
        {
            return _getter((T)instance);
        }
    }

    /// <summary>
    /// This class caches all the interesting attributes of an property.
    /// </summary>
    [DebuggerDisplay("Name = {Member.Name}")]
    internal class MetaMember
    {
        private static MethodInfo s_getterDelegateHelper = typeof(MetaMember).GetMethod(nameof(MetaMember.CreateGetterDelegateHelper), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        private static MethodInfo s_setterDelegateHelper = typeof(MetaMember).GetMethod(nameof(MetaMember.CreateSetterDelegateHelper), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        private static MethodInfo s_createMemberHelper = typeof(MetaMember).GetMethod(nameof(MetaMember.CreateMemberHelper), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        private Func<object, object> _getter;
        private Action<object, object> _setter;

        public MetaMember(MetaType metaType, PropertyInfo propertyInfo)
        {
            this.Member = propertyInfo;
            this.MetaType = metaType;
        }

        public string Name => Member.Name;

        public Type PropertyType => Member.PropertyType;

        public MetaType MetaType { get; protected set; }

        public EditableAttribute EditableAttribute { get; internal set; }

        public PropertyInfo Member { get; protected set; }

        public bool IsAssociationMember { get; internal set; }

        public bool IsDataMember { get; internal set; }

        public bool IsKeyMember { get; internal set; }

        public bool IsRoundtripMember { get; internal set; }

        public bool IsComplex { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether this member is a supported collection type.
        /// </summary>
        public bool IsCollection { get; internal set; }

        /// <summary>
        /// Returns <c>true</c> if the member has a property validator.
        /// </summary>
        /// <remarks>The return value does not take into account whether or not the member requires
        /// type validation.</remarks>
        public bool RequiresValidation { get; internal set; }

        /// <summary>
        /// Get the value of the member
        /// </summary>
        /// <param name="instance">the instance from which the member should be accessed</param>
        /// <returns>the value of the property</returns>
        public object GetValue(object instance)
        {
            if (_getter == null)
            {
                _getter = CreateGetterDelegate(Member);
            }
            return _getter(instance);
        }

        public string GetValueAsString(object instance)
        {
            return GetValue(instance).ToString();
        }

        public virtual object GetValueVirtual(object instance)
        {
            throw new NotImplementedException();
        }

        public virtual string GetValueAsStringVirtual(object instance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the value of the member
        /// </summary>
        /// <param name="instance">the instance from which the member should be accessed</param>
        /// <param name="value">the value to set</param>
        /// <returns>the value of the property</returns>
        public void SetValue(object instance, object value)
        {
            if (_setter == null)
            {
                _setter = CreateSetterDelegate(Member);
            }
            _setter(instance, value);
        }

        public bool IsMergable { get; internal set; }

        /// <summary>
        /// <c>true</c> if the member is marked with a <see cref="CompositionAttribute"/>
        /// </summary>
        public bool IsComposition { get; internal set; }

        /// <summary>
        /// Helper method which creates a delegate which can be used to invoke a specific getter
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private static Func<object, object> CreateGetterDelegate(PropertyInfo propertyInfo)
        {
            var helper = s_getterDelegateHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (Func<object, object>)helper.Invoke(null, new[] { propertyInfo });
        }

        private static Func<object, object> CreateGetterDelegateHelper<T, Tprop>(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
            {
                // If no getter was found, fallback to method throw same type of exception
                // which exception would do, these should never propagate to the user
                return obj => { throw new ArgumentException("Internal error: No getter"); };
            }

            var getter = (Func<T, Tprop>)Delegate.CreateDelegate(typeof(Func<T, Tprop>), getMethod);
            // Add a wrapper which performs boxing of the function
            return (object instance) => (object)getter((T)instance);
        }

        /// <summary>
        /// Helper method which creates a delegate which can be used to invoke a specific getter
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private static Action<object, object> CreateSetterDelegate(PropertyInfo propertyInfo)
        {
            var helper = s_setterDelegateHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (Action<object, object>)helper.Invoke(null, new[] { propertyInfo });
        }

        private static Action<object, object> CreateSetterDelegateHelper<T, Tprop>(PropertyInfo propertyInfo)
        {
            // If no getter was found, fallback to using reflection which will throw exception
            var setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
            {
                // If no setter was found, fallback to method throw same type of exception
                // which exception would do, these should never propagate to the user
                return (obj, val) => { throw new ArgumentException("Internal error: No setter"); };
            }

            var setter = (Action<T, Tprop>)Delegate.CreateDelegate(typeof(Action<T, Tprop>), setMethod);
            // Add a wrapper which performs unboxing for the function
            return (object obj, object value) => setter((T)obj, (Tprop)value);
        }

        public static MetaMember CreateMember(MetaType metaType, PropertyInfo propertyInfo)
        {
            var createMethod = s_createMemberHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (MetaMember)createMethod.Invoke(null, new object[] { metaType, propertyInfo });
        }

        private static MetaMember CreateMemberHelper<T, TProp>(MetaType metaType, PropertyInfo propertyInfo)
        {
            var member = FastActivator.CreateInstance<MetaMember<T, TProp>>();
            member.Initialize(metaType, propertyInfo);
            return member;
        }
    }
}
