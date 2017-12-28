using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.ServiceBusMiddleware.Functions
{
    /// <summary>
    ///  Properties of the incoming message that must be exposed as user properties
    /// </summary>
    public class TopicMessageUserProperty
    {
        /// <summary>
        /// Incoming source property name
        /// </summary>
        public string SourcePropertyName { get; set; }

        /// <summary>
        /// Target property name
        /// </summary>
        public string TargetPropertyName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TopicMessageUserProperty()
        {

        }

        public TopicMessageUserProperty(string property) : this(property, property)
        {
        }

        public TopicMessageUserProperty(string sourceProperty, string targetProperty)
        {
            this.SourcePropertyName = sourceProperty;
            this.TargetPropertyName = targetProperty;
        }

        /// <summary>
        /// Parse a raw definition of properties
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IList<TopicMessageUserProperty> Parse(string value)
        {
            var result = new List<TopicMessageUserProperty>();
            var tokens = value.Split(',');
            foreach (var token in tokens)
            {
                var sourceAndTarget = token.Split('=');
                if (sourceAndTarget.Length == 2)
                {
                    result.Add(new TopicMessageUserProperty()
                    {
                        SourcePropertyName = sourceAndTarget[0].Trim(),
                        TargetPropertyName = sourceAndTarget[1].Trim()
                    });
                }
                else
                {
                    var pm = new TopicMessageUserProperty();
                    pm.SourcePropertyName = token.Trim();
                    pm.TargetPropertyName = pm.SourcePropertyName;
                    result.Add(pm);
                }
            }

            return result;
        }
    }
}
