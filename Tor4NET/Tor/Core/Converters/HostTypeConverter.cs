﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace Tor.Converters
{
    /// <summary>
    /// A class providing the methods necessary to convert between a <see cref="Host"/> object and <see cref="System.String"/> object.
    /// </summary>
    public sealed class HostTypeConverter : TypeConverter
    {
        #region System.ComponentModel.TypeConverter

        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType.Equals(typeof(string)))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that represents the converted value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// A string must contain an IP address, or an IP address and port number, format
        /// or
        /// A string containing an IP address and port must contain a valid port number
        /// </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                return Host.Null;

            if (value is string)
            {
                string actual = value as string;
                
                if (actual.Contains(":"))
                {
                    int port;
                    string[] parts = actual.Split(':');

                    if (parts.Length != 2)
                        throw new InvalidCastException("A string must contain an IP address, or an IP address and port number, format");

                    if (!int.TryParse(parts[1], out port))
                        throw new InvalidCastException("A string containing an IP address and port must contain a valid port number");

                    return new Host(parts[0], port);
                }

                return new Host(actual);
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is assumed.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <paramref name="value" /> parameter to.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that represents the converted value.
        /// </returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType.Equals(typeof(string)))
            {
                Host host = (Host)value;

                if (host.IsNull)
                    return "";

                if (host.Port == -1)
                    return host.Address;

                return string.Format("{0}:{1}", host.Address, host.Port);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion
    }
}
