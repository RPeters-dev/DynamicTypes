using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTypes
{
    public class AttributeGenerator
    {
        ConstructorInfo Constructor { get; set; }
        public object[] Parameter { get; set; }
        public CustomAttributeBuilder AttributeBuilder { get; }
        /// <summary>
        /// Initializes a nw instance of <see cref="AttributeGenerator"/>
        /// </summary>
        /// <param name="attributeType">Type of the attribute. !! if the attribute is not public u cant cosume it via <see cref="MemberInfo.GetCustomAttributes(bool)"/></param>
        /// <param name="parameter">parameter odf the attribute</param>
        public AttributeGenerator(Type attributeType, params object[] parameter)
        {
            var ParameterTypes = parameter.Select(x => x?.GetType()).ToList();
            var constructors = attributeType.GetConstructors();

            var matches = new List<ConstructorInfo>();
            object[] actualParameter = new object[0];
            foreach (var item in constructors)
            {
                var cps = item.GetParameters().ToArray();
                actualParameter = new object[cps.Length];

                //ctor is too short
                if (cps.Length < ParameterTypes.Count)
                {
                    continue;
                }

                bool match = true;
                for (int i = 0; i < cps.Length; i++)
                {
                    //wildcard Parameter
                    if (ParameterTypes.Count <= i)
                    {
                        actualParameter[i] = cps[i].DefaultValue;
                        continue;
                    }
                    else
                    {
                        if (ParameterTypes[i] == null)
                        {
                            actualParameter[i] = cps[i].DefaultValue;
                            continue;
                        }
                    }
                    if (ParameterTypes[i].IsAssignableFrom(cps[i].ParameterType))
                    {
                        actualParameter[i] = parameter[i];
                    }
                    else
                    {
                        match = false;
                    }
                }
                if (match)
                {
                    matches.Add(item);
                }
            }
            if (matches.Count > 1)
            {
                throw new AmbiguousMatchException("Multiple Costructors found that match the Parameter");
            }
            if (matches.Count == 0)
            {
                throw new Exception("No Costructors found that match the Parameter");
            }

            Constructor = matches[0];
            Parameter = actualParameter;
            AttributeBuilder = new CustomAttributeBuilder(Constructor, Parameter);
        }
    }
}
