﻿using System.Runtime.Serialization;

namespace SimpleIdServer.AccountFilter.Basic.Common
{
    [DataContract]
    public enum ComparisonOperationsDto
    {
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.Equal)]
        Equal,
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.NotEqual)]
        NotEqual,
        [EnumMember(Value = Constants.ComparisonOperationsDtoNames.RegularExpression)]
        RegularExpression
    }
}
