﻿using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Extensions;

public static class TypeExtensions
{
    public static string GetDisplayValue(this Type type, string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("propertyName");

        var propertyInfo = type.GetProperty(propertyName);
        if (propertyInfo == null) 
            throw new InvalidOperationException("Type with this property does not exists");
            
        var displayAttribute = propertyInfo.GetCustomAttributes(true).FirstOrDefault(i => i.GetType().IsAssignableFrom(typeof(DisplayAttribute))) as DisplayAttribute;

        if (displayAttribute != null)
        {
            return displayAttribute.GetName();
        }

        return propertyName;
    }
}
