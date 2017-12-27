using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.ServiceBusMiddleware.Functions
{
    /// <summary>
    ///  Properties of the incoming message that must be exposed as user properties
    /// </summary>
    public class TopicMessageCustomProperty
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
        public TopicMessageCustomProperty()
        {

        }

        public TopicMessageCustomProperty(string property) : this(property, property)
        {
        }

        public TopicMessageCustomProperty(string sourceProperty, string targetProperty)
        {
            this.SourcePropertyName = sourceProperty;
            this.TargetPropertyName = targetProperty;
        }

        /// <summary>
        /// Parse a raw definition of properties
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IList<TopicMessageCustomProperty> Parse(string value)
        {
            var result = new List<TopicMessageCustomProperty>();
            var tokens = value.Split(',');
            foreach (var token in tokens)
            {
                var sourceAndTarget = token.Split('=');
                if (sourceAndTarget.Length == 2)
                {
                    result.Add(new TopicMessageCustomProperty()
                    {
                        SourcePropertyName = sourceAndTarget[0].Trim(),
                        TargetPropertyName = sourceAndTarget[1].Trim()
                    });
                }
                else
                {
                    var pm = new TopicMessageCustomProperty();
                    pm.SourcePropertyName = token.Trim();
                    pm.TargetPropertyName = pm.SourcePropertyName;
                    result.Add(pm);
                }
            }

            return result;
        }
    }
}
