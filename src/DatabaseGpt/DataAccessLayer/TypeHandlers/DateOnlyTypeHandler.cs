using System.Data;
using Dapper;

namespace DatabaseGpt.DataAccessLayer.TypeHandlers;

internal class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
        => DateOnly.FromDateTime(Convert.ToDateTime(value));

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}
