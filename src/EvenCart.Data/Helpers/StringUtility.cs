﻿#region License
// Copyright (c) Sojatia Infocrafts Private Limited.
// The following code is part of EvenCart eCommerce Software (https://evencart.co) Dual Licensed under the terms of
// 
// 1. GNU GPLv3 with additional terms (available to read at https://evencart.co/license)
// 2. EvenCart Proprietary License (available to read at https://evencart.co/license/commercial-license).
// 
// You can select one of the above two licenses according to your requirements. The usage of this code is
// subject to the terms of the license chosen by you.
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvenCart.Data.Helpers
{
    public class StringUtility
    {
        /// <summary>
        /// Returns appropriate string based on arity of the value passed
        /// </summary>
        public static string Arity(int value, string singularNoun, string pluralNoun)
        {
            var formatted = $"{value} {(value == 1 ? singularNoun : pluralNoun)}";
            return formatted;
        }
        
        /// <summary>
        /// Joins a string collection and returns a formatted string with separator character and words
        /// </summary>
        public static string FormattedJoin(ICollection<string> stringList, string separatorString = ",", string separatorWord = "and")
        {
            if (!stringList.Any())
                return string.Empty;

            //[a b c d e] , and
            var builder = new StringBuilder();
            var strList = stringList.ToList();
            for (var index = 0; index < stringList.Count - 1; index++)
            {
                var str = strList[index];
                builder.Append(str + separatorString);
                //a,b,c,d
            }
            builder.AppendFormat(" {0} ", separatorWord.Trim()); //a,b,c,d and

            builder.Append(strList.Last()); //a,b,c,d and e

            return builder.ToString();
        }
    }
}