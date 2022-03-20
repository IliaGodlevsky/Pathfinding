﻿using EnumerationValues.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnumerationValues.Realizations
{
    public sealed class EnumValues<TEnum> : IEnumValues<TEnum>
        where TEnum : Enum
    {
        private readonly Lazy<TEnum[]> values;

        public IReadOnlyCollection<TEnum> Values => values.Value;

        public EnumValues()
        {
            values = new Lazy<TEnum[]>(GetValues, isThreadSafe: true);
        }

        private TEnum[] GetValues()
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
        }
    }
}
