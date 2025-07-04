using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace MetaRPC.CSharpMT5
{
    /// <summary>
    /// Помогает инспектировать и логировать все публичные свойства объекта через ILogger.
    /// </summary>
    public static class ObjectInspector
    {
        /// <summary>
        /// Логирует все публичные свойства объекта и их значения.
        /// </summary>
        /// <param name="logger">Экземпляр ILogger для вывода.</param>
        /// <param name="obj">Объект для инспекции.</param>
        /// <param name="objectName">Имя объекта, которое будет в заголовке лога.</param>
        public static void LogObjectProperties(ILogger logger, object obj, string objectName = "Object")
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (obj == null)
            {
                logger.LogInformation("{ObjectName} is null.", objectName);
                return;
            }

            var type = obj.GetType();
            logger.LogInformation("Inspecting {ObjectName} of type {TypeName}:", objectName, type.FullName);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(obj);
                    logger.LogInformation("{PropertyName}: {PropertyValue}", prop.Name, value);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error accessing property {PropertyName} on {ObjectName}", prop.Name, objectName);
                }
            }
        }
    }
}
