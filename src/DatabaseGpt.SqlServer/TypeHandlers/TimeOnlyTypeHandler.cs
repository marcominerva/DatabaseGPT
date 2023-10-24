using System.Data;
using Dapper;

namespace DatabaseGpt.SqlServer.TypeHandlers;

internal class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value)
        => TimeOnly.FromTimeSpan(TimeSpan.Parse(value.ToString()!));

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}