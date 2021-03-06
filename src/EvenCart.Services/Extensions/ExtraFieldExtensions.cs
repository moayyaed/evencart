﻿// #region Author Information
// // ExtraFieldExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Collections.Generic;
using System.Linq;
using EvenCart.Core.Data;
using EvenCart.Core.DataStructures;
using EvenCart.Core.Infrastructure;
using EvenCart.Data.Entity.ExtraFields;
using EvenCart.Data.Entity.Users;
using EvenCart.Data.Enum;
using EvenCart.Data.Extensions;
using EvenCart.Data.Interfaces;
using EvenCart.Services.Enum;
using EvenCart.Services.ExtraFields;

namespace EvenCart.Services.Extensions
{
    public static class ExtraFieldExtensions
    {
        public const string ExtraFieldsRequestKey = "e";

        public static string ExtraFieldNameFormat = $"{ExtraFieldsRequestKey}_{0}";

        public static ExtraFieldValidationResult ValidateValueForUser(this ExtraField extraField, string fieldValue, User user)
        {
            var required = (user.IsRegistered() && extraField.RequiredForUsers) ||
                           (user.IsVendor() && extraField.RequiredForAgents);

            if (required && fieldValue.IsNullEmptyOrWhiteSpace())
                return ExtraFieldValidationResult.EmptyValueForRequiredField;

            var visible = (user.IsRegistered() && extraField.VisibleToUsers) ||
                           (user.IsVendor() && extraField.VisibleToAgents);

            if (user.IsRegistered() && (!extraField.IsUserEditable || !visible))
                return ExtraFieldValidationResult.NonEditableField;

            switch (extraField.FieldType)
            {
                case InputFieldType.Number:
                    if(!fieldValue.IsNumeric())
                        return ExtraFieldValidationResult.InvalidValueForFieldType;

                    if (extraField.MinimumValue.IsNumeric())
                    {
                        if (extraField.MinimumValue.GetInteger(false) > fieldValue.GetInteger(false))
                        {
                            return ExtraFieldValidationResult.OutOfRangeValue;
                        }
                    }
                    if (extraField.MaximumValue.IsNumeric())
                    {
                        if (extraField.MaximumValue.GetInteger(false) < fieldValue.GetInteger(false))
                        {
                            return ExtraFieldValidationResult.OutOfRangeValue;
                        }
                    }
                    break;
                case InputFieldType.Email:
                    if(!fieldValue.IsValidEmail())
                        return ExtraFieldValidationResult.InvalidValueForFieldType;
                    break;
                case InputFieldType.DateTime:
                    if (!fieldValue.IsDateTime())
                        return ExtraFieldValidationResult.InvalidValueForFieldType;

                    if (extraField.MinimumValue.IsDateTime())
                    {
                        if (extraField.MinimumValue.GetDateTime(false) > fieldValue.GetDateTime(false))
                        {
                            return ExtraFieldValidationResult.OutOfRangeValue;
                        }
                    }
                    if (extraField.MaximumValue.IsDateTime())
                    {
                        if (extraField.MaximumValue.GetDateTime(false) < fieldValue.GetDateTime(false))
                        {
                            return ExtraFieldValidationResult.OutOfRangeValue;
                        }
                    }
                    break;
                case InputFieldType.Color:
                    if(!fieldValue.IsColor())
                        return ExtraFieldValidationResult.InvalidValueForFieldType;
                    break;
                case InputFieldType.Dropdown:
                    break;
                case InputFieldType.ImageUpload:
                    break;
                case InputFieldType.FileUpload:
                    break;
                default:
                    return ExtraFieldValidationResult.UnknownError;
            }

            return ExtraFieldValidationResult.ValidField;
        }

        public static string GetDbFieldName(this ExtraField extraField)
        {
            return string.Format(ExtraFieldNameFormat, extraField.Id);
        }

        public static IList<Pair<ExtraField, string>> GetExtraFields(this IHasEntityProperties entity)
        {
            var entityProperties = entity.GetProperties();
            var extraFieldService = DependencyResolver.Resolve<IExtraFieldService>();
            var entityName = entity.GetType().Name;
            var typeExtraFields = extraFieldService.Get(x => x.EntityName == entityName).ToList();

            var extraFieldList = new List<Pair<ExtraField, string>>();

            foreach (var ef in typeExtraFields) {
                var fieldName = ef.GetDbFieldName();
                var ep = entityProperties.FirstOrDefault(x => x.EntityName == entityName && x.PropertyName == fieldName);
                var fieldValue = ef.DefaultValue;
                if (ep == null)
                {
                    fieldValue = ep.Value;
                }
                extraFieldList.Add(new Pair<ExtraField, string>()
                {
                    First = ef,
                    Second = fieldValue
                });
            }
            return extraFieldList;
        }
    }
}