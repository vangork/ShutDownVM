using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftAzureManagement
{
    /// <summary>
    /// Used as a wrapper class for passing a property
    /// </summary>
    /// <typeparam name="T">type of property</typeparam>
    public class PropertyInvoker
    {
        private PropertyInfo propInfo;
        private object obj;

        public delegate string GetDelegateEventHandler();
        public delegate void SetDelegateEventHandler(string value);

        /// <summary>
        /// Construct an property Invoker
        /// </summary>
        /// <param name="PropertyName">the property of an object we want to access</param>
        /// <param name="o">the object</param>
        public PropertyInvoker(string propertyName, object o)
        {
            // set the instance object
            this.obj = o;
            // get the property information and check weather the type matchs T
            this.propInfo = o.GetType().GetProperty(propertyName, typeof(string));
        }

        /// <summary>
        /// Wrapping the get and set into a property
        /// </summary>
        public string Property
        {
            get
            {
                return (string)propInfo.GetValue(obj, null);
            }
            set
            {
                propInfo.SetValue(obj, value, null);
            }
        }

        /// <summary>
        /// Wrapping the get function
        /// </summary>
        /// <returns></returns>
        public string GetProperty()
        {
            return this.Property;
        }

        /// <summary>
        /// Wrapping the set function
        /// </summary>
        /// <param name="value"></param>
        public void SetProperty(string value)
        {
            this.Property = value;
        }

        public static PropertyInvoker operator +(PropertyInvoker c1, string c2)
        {
            if (c1 == null)
                return null;
            GetDelegateEventHandler get = c1.GetProperty;
            SetDelegateEventHandler set = c1.SetProperty;
            set.Invoke(get.Invoke() + c2);
            return c1;
        }
        public static PropertyInvoker operator +(string c1, PropertyInvoker c2)
        {
            if (c2 == null)
                return null;
            return c2 + c1;
        }

    }
}
