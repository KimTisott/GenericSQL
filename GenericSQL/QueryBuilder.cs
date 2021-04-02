using GenericSQL.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GenericSQL
{
    public static class QueryBuilder
    {
        public static async Task<GenericQuery<T>> Query<T>(this GenericDB db)
        {
            var query = new GenericQuery<T>(db.ConnectionString);
            var tableName = typeof(T).CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute)).ConstructorArguments.FirstOrDefault().Value.ToString();
            if (string.IsNullOrWhiteSpace(tableName))
                throw new Exception("Could not find entity table");
            else
                query.TableName = tableName;
            return await Task.FromResult(query);
        }

        public static async Task<T> Get<T>(this Task<GenericQuery<T>> query)
        {
            return await Dapper.SelectItem(await query);
        }

        public static async Task<IEnumerable<T>> GetAll<T>(this Task<GenericQuery<T>> query)
        {
            return await Dapper.SelectList(await query);
        }

        public static async Task<GenericQuery<T>> Select<T>(this Task<GenericQuery<T>> query, Expression<Func<T, object>> expression)
        {
            var newQuery = await query;
            var body = expression.Body;
            if (body is NewExpression newExpression)
            {
                foreach (var argument in newExpression.Arguments)
                {
                    if (argument is MemberExpression memberExpression)
                    {
                        var tableName = memberExpression.Member.DeclaringType.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute)).ConstructorArguments.FirstOrDefault().Value.ToString();
                        if (string.IsNullOrWhiteSpace(tableName))
                            throw new Exception("Could not find column table");
                        else
                        {
                            if (tableName != newQuery.TableName)
                            {
                                var foreignKey = newQuery.Type.GetProperties().FirstOrDefault(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(ForeignKeyAttribute) && y.ConstructorArguments.Any(z => z.Value.ToString() == tableName))).CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(ColumnAttribute)).ConstructorArguments.FirstOrDefault().Value.ToString();
                                var primaryKey = memberExpression.Member.DeclaringType.GetProperties().FirstOrDefault(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(PrimaryKeyAttribute))).CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(ColumnAttribute)).ConstructorArguments.FirstOrDefault().Value.ToString();
                                newQuery.Joins.Add(new(tableName, $"{tableName}.{primaryKey}", $"{newQuery.TableName}.{foreignKey}"));
                            }
                            var columnName = memberExpression.Member.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(ColumnAttribute)).ConstructorArguments.FirstOrDefault().Value.ToString();
                            if (!string.IsNullOrWhiteSpace(columnName))
                                newQuery.ColumnNames.Add($"{tableName}.{columnName}");
                        }
                    }
                }
            }
            return await Task.FromResult(newQuery);
        }

        public static async Task<int> Add<T>(this Task<GenericQuery<T>> query, T entity)
        {
            return await Dapper.Insert(await query, entity);
        }

        public static async Task<int> Set<T>(this Task<GenericQuery<T>> query, T entity)
        {
            return await Dapper.Update(await query, entity);
        }

        public static async Task<int> Cut<T>(this Task<GenericQuery<T>> query, T entity)
        {
            return await Dapper.Delete(await query);
        }

        public static async Task<int?> Sum<T>(this Task<GenericQuery<T>> query)
        {
            return await Dapper.Count(await query);
        }

        public static async Task<GenericQuery<T>> Where<T>(this Task<GenericQuery<T>> query, Expression<Func<T, object>> expression)
        {
            var newQuery = await query;
            var body = expression.Body;
            if (body is UnaryExpression unaryExpression)
            {
                if (unaryExpression.Operand is BinaryExpression binaryExpression)
                {
                    if (binaryExpression.Left is ConstantExpression constantExpressionLeft)
                    {
                        newQuery.Where += constantExpressionLeft.Value.ToString();
                    }
                    else if (binaryExpression.Left is MemberExpression memberExpressionLeft)
                    {
                        newQuery.Where += memberExpressionLeft.Member.Name;
                    }
                    else if (binaryExpression.Left is UnaryExpression unaryExpressionLeft)
                    {
                        if (unaryExpressionLeft.Operand is MemberExpression memberExpressionLeftOperand)
                        {
                            newQuery.Where += memberExpressionLeftOperand.Member.Name;
                        }
                    }
                    newQuery.Where += await ExpressionTypeToSql(binaryExpression.NodeType);
                    if (binaryExpression.Right is ConstantExpression constantExpressionRight)
                    {
                        newQuery.Where += constantExpressionRight.Value.ToString();
                    }
                    else if (binaryExpression.Right is MemberExpression memberExpressionRight)
                    {
                        newQuery.Where += memberExpressionRight.Member.Name;
                    }
                }
                else
                    ;
            }
            else
                ;            
            return await Task.FromResult(newQuery);
        }

        private static async Task<string> ExpressionTypeToSql(ExpressionType expressionType)
        {
            string result = default;
            switch (expressionType)
            {
                case ExpressionType.Add:
                    break;
                case ExpressionType.AddChecked:
                    break;
                case ExpressionType.And:
                    break;
                case ExpressionType.AndAlso:
                    break;
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.ArrayIndex:
                    break;
                case ExpressionType.Call:
                    break;
                case ExpressionType.Coalesce:
                    break;
                case ExpressionType.Conditional:
                    break;
                case ExpressionType.Constant:
                    break;
                case ExpressionType.Convert:
                    break;
                case ExpressionType.ConvertChecked:
                    break;
                case ExpressionType.Divide:
                    break;
                case ExpressionType.Equal:
                    result = "=";
                    break;
                case ExpressionType.ExclusiveOr:
                    break;
                case ExpressionType.GreaterThan:
                    result = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    result = ">=";
                    break;
                case ExpressionType.Invoke:
                    break;
                case ExpressionType.Lambda:
                    break;
                case ExpressionType.LeftShift:
                    break;
                case ExpressionType.LessThan:
                    result = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    result = "<=";
                    break;
                case ExpressionType.ListInit:
                    break;
                case ExpressionType.MemberAccess:
                    break;
                case ExpressionType.MemberInit:
                    break;
                case ExpressionType.Modulo:
                    break;
                case ExpressionType.Multiply:
                    break;
                case ExpressionType.MultiplyChecked:
                    break;
                case ExpressionType.Negate:
                    break;
                case ExpressionType.UnaryPlus:
                    break;
                case ExpressionType.NegateChecked:
                    break;
                case ExpressionType.New:
                    break;
                case ExpressionType.NewArrayInit:
                    break;
                case ExpressionType.NewArrayBounds:
                    break;
                case ExpressionType.Not:
                    break;
                case ExpressionType.NotEqual:
                    break;
                case ExpressionType.Or:
                    break;
                case ExpressionType.OrElse:
                    break;
                case ExpressionType.Parameter:
                    break;
                case ExpressionType.Power:
                    break;
                case ExpressionType.Quote:
                    break;
                case ExpressionType.RightShift:
                    break;
                case ExpressionType.Subtract:
                    break;
                case ExpressionType.SubtractChecked:
                    break;
                case ExpressionType.TypeAs:
                    break;
                case ExpressionType.TypeIs:
                    break;
                case ExpressionType.Assign:
                    break;
                case ExpressionType.Block:
                    break;
                case ExpressionType.DebugInfo:
                    break;
                case ExpressionType.Decrement:
                    break;
                case ExpressionType.Dynamic:
                    break;
                case ExpressionType.Default:
                    break;
                case ExpressionType.Extension:
                    break;
                case ExpressionType.Goto:
                    break;
                case ExpressionType.Increment:
                    break;
                case ExpressionType.Index:
                    break;
                case ExpressionType.Label:
                    break;
                case ExpressionType.RuntimeVariables:
                    break;
                case ExpressionType.Loop:
                    break;
                case ExpressionType.Switch:
                    break;
                case ExpressionType.Throw:
                    break;
                case ExpressionType.Try:
                    break;
                case ExpressionType.Unbox:
                    break;
                case ExpressionType.AddAssign:
                    break;
                case ExpressionType.AndAssign:
                    break;
                case ExpressionType.DivideAssign:
                    break;
                case ExpressionType.ExclusiveOrAssign:
                    break;
                case ExpressionType.LeftShiftAssign:
                    break;
                case ExpressionType.ModuloAssign:
                    break;
                case ExpressionType.MultiplyAssign:
                    break;
                case ExpressionType.OrAssign:
                    break;
                case ExpressionType.PowerAssign:
                    break;
                case ExpressionType.RightShiftAssign:
                    break;
                case ExpressionType.SubtractAssign:
                    break;
                case ExpressionType.AddAssignChecked:
                    break;
                case ExpressionType.MultiplyAssignChecked:
                    break;
                case ExpressionType.SubtractAssignChecked:
                    break;
                case ExpressionType.PreIncrementAssign:
                    break;
                case ExpressionType.PreDecrementAssign:
                    break;
                case ExpressionType.PostIncrementAssign:
                    break;
                case ExpressionType.PostDecrementAssign:
                    break;
                case ExpressionType.TypeEqual:
                    break;
                case ExpressionType.OnesComplement:
                    break;
                case ExpressionType.IsTrue:
                    break;
                case ExpressionType.IsFalse:
                    break;
                default:
                    result = "";
                    break;
            }
            return await Task.FromResult(result);
        }



        //internal IEnumerable<string> GetColumnNames<T>(Expression<Func<T, object>> expression)
        //{
        //    var result = new List<string>();
        //    if (expression == null)
        //        result.Add("*");
        //    else
        //    {
        //        var body = expression.Body;
        //        MemberExpression memberExpression = body as MemberExpression;
        //        if (memberExpression == null)
        //        {
        //            UnaryExpression unaryExpression = body as UnaryExpression;
        //            if (unaryExpression == null)
        //                result.AddRange(body.Type.GetProperties().Select(x => x.GetCustomAttribute<ColumnAttribute>().Name));
        //            else
        //                result.Add(((MemberExpression)unaryExpression.Operand).Member.GetCustomAttribute<ColumnAttribute>().Name);
        //        }
        //        else
        //            result.Add(memberExpression.Member.GetCustomAttribute<ColumnAttribute>().Name);
        //    }
        //    return result;
        //}

        //internal string GetTableName<T>()
        //{
        //    var attribute = GetAttribute<TableAttribute>(typeof(T));
        //    return attribute.Name;
        //}

        //internal string GetColumnName<T>()
        //{
        //    var attribute = GetAttribute<ColumnAttribute>(typeof(T));
        //    return attribute.Name;
        //}

        //internal T GetAttribute<T>(Type type) where T : Attribute
        //{
        //    var attribute = type.GetCustomAttribute<T>();
        //    if (attribute == null)
        //        throw new Exception($"Could not find the {typeof(T).Name} attribute for {type.Name}."); ;
        //    return attribute;
        //}
    }
}