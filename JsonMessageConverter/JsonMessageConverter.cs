using Apache.NMS;
using Newtonsoft.Json;
using Spring.Messaging.Nms.Support.Converter;
using System;
using System.Collections;

namespace Spring.Messaging.Nms.Support.Converter
{
    /// <summary>
    /// Convert an object via JSON serialization for sending via an ITextMessage
    /// </summary>
    /// <author>Ivan R. Perez</author>
    public class JsonMessageConverter : IMessageConverter
    {
        private IMessageConverter defaultMessageConverter = new SimpleMessageConverter();
        private TypeMapper typeMapper = new TypeMapper();

        public JsonMessageConverter()
        {
            typeMapper.UseAssemblyQualifiedName = true;
        }

        /// <summary>
        /// Sets the type mapper
        /// </summary>
        /// <value>The type mapper.</value>
        public TypeMapper TypeMapper
        {
            set { typeMapper = value; }
        }

        /// <summary>
        /// Convert a .NET object to a NMS Message using the supplied session
        /// to create the message object.
        /// </summary>
        /// <param name="objectToConvert">THe object to convert</param>
        /// <param name="session">The Session to use for creating a NMS Message</param>
        /// <returns>The NMS Message</returns>
        /// <throws>NMSException if thrown by NMS API methods</throws>
        /// <throws>MessageConversionException in case of conversion failure</throws>
        public IMessage ToMessage(object objectToConvert, ISession session)
        {
            if (objectToConvert == null)
            {
                throw new MessageConversionException("Can't convert null object");
            }

            try
            {
                if (objectToConvert.GetType().Equals(typeof(string)) ||
                    typeof(IDictionary).IsAssignableFrom(objectToConvert.GetType()) ||
                    objectToConvert.GetType().Equals(typeof(Byte[])))
                {
                    return defaultMessageConverter.ToMessage(objectToConvert, session);
                }
                string jsonString = GetJsonString(objectToConvert);
                IMessage msg = session.CreateTextMessage(jsonString);
                msg.Properties.SetString(typeMapper.TypeIdFieldName, typeMapper.FromType(objectToConvert.GetType()));
                return msg;
            }
            catch (Exception ex)
            {
                throw new MessageConversionException("Can't convert object of type " + objectToConvert.GetType(), ex);
            }
        }

        /// <summary>
        /// Gets the JSON string for an object
        /// </summary>
        /// <param name="objectToConvert">The object to convert.</param>
        /// <returns>JSON string</returns>
        protected virtual string GetJsonString(object objectToConvert)
        {
            string jsonString;
            try
            {
                jsonString = JsonConvert.SerializeObject(objectToConvert);
            }
            catch (Exception ex)
            {
                throw new MessageConversionException("Can't convert object of type " + objectToConvert.GetType(), ex);
            }

            return jsonString;
        }

        /// <summary>
        /// Convert from a NMS Message to a .NET object.
        /// </summary>
        /// <param name="messageToConvert">The message to convert</param>
        /// <returns>The converted .NET object</returns>
        /// <throws>MessageConversionException in case of converesion failure</throws>
        public object FromMessage(IMessage messageToConvert)
        {
            if (messageToConvert == null)
            {
                throw new MessageConversionException("Can't convert null message");
            }

            try
            {
                string converterId = messageToConvert.Properties.GetString(typeMapper.TypeIdFieldName);
                if (converterId == null)
                {
                    return defaultMessageConverter.FromMessage(messageToConvert);
                }
                ITextMessage textMessage = messageToConvert as ITextMessage;
                if (textMessage == null)
                {
                    throw new MessageConversionException("Can't convert message of type " + messageToConvert.GetType());
                }

                var obj = JsonConvert.DeserializeObject(textMessage.Text, GetTargetType(textMessage));
                return obj;
            }
            catch (Exception ex)
            {
                throw new MessageConversionException("Can't convert message of type " + messageToConvert.GetType(), ex);
            }
        }

        /// <summary>
        /// Gets the type of the target given the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Type of the target</returns>
        protected virtual Type GetTargetType(ITextMessage message)
        {
            return typeMapper.ToType(message.Properties.GetString(typeMapper.TypeIdFieldName));
        }
    }
}