using System;
using System.Globalization;
using System.Linq;
using Dataverse_Record_Recovery.Localization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Dataverse_Record_Recovery.Services
{
    /// <summary>
    /// Turns column values into readable text
    /// </summary>
    public static class ValueFormatter
    {
        public static string Format(object value, AttributeMetadata attribute)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var reference = value as EntityReference;
            if (reference != null)
            {
                return string.IsNullOrEmpty(reference.Name)
                    ? $"{reference.LogicalName}: {reference.Id}"
                    : $"{reference.Name} ({reference.LogicalName}: {reference.Id})";
            }

            var option = value as OptionSetValue;
            if (option != null)
            {
                return FormatOption(option.Value, attribute);
            }

            var options = value as OptionSetValueCollection;
            if (options != null)
            {
                return string.Join("; ", options.Select(o => FormatOption(o.Value, attribute)));
            }

            if (value is bool)
            {
                var booleanAttribute = attribute as BooleanAttributeMetadata;
                var label = (bool)value
                    ? booleanAttribute?.OptionSet?.TrueOption?.Label?.UserLocalizedLabel?.Label
                    : booleanAttribute?.OptionSet?.FalseOption?.Label?.UserLocalizedLabel?.Label;
                return label ?? value.ToString();
            }

            var money = value as Money;
            if (money != null)
            {
                return money.Value.ToString("N2", CultureInfo.CurrentCulture);
            }

            if (value is DateTime)
            {
                return ((DateTime)value).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            }

            var collection = value as EntityCollection;
            if (collection != null)
            {
                return Strings.RecordCount(collection.Entities.Count);
            }

            if (value is byte[])
            {
                return Strings.Binary;
            }

            return Convert.ToString(value, CultureInfo.CurrentCulture);
        }

        private static string FormatOption(int value, AttributeMetadata attribute)
        {
            var enumAttribute = attribute as EnumAttributeMetadata;
            var label = enumAttribute?.OptionSet?.Options?
                .FirstOrDefault(o => o.Value == value)?
                .Label?.UserLocalizedLabel?.Label;
            return label == null ? value.ToString() : $"{label} ({value})";
        }
    }
}
