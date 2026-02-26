using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Infrastructure
{
    public  class ExpressionHelper
    {
        /// <summary>
        /// Users.SingleOrDefaultAsync(MakeEqual((User u) => u.PhoneNumber, phoneNumber))
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="propAccessor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Expression<Func<TItem,bool>> MakeEqual<TItem,TProp>(
            Expression<Func<TItem,TProp>> propAccess, TProp? other )
            where TProp : class
            where TItem : class
        {
            var e1 = propAccess.Parameters.Single();
            var e2 = propAccess.Body;
            BinaryExpression? condionalExpr = null;
            foreach (var prop in typeof(TProp).GetProperties())
            {
                BinaryExpression equalExpr;
                object? otherValue = null;
                if (other != null)
                {
                    otherValue = prop.GetValue(other);
                }
                Type propType = prop.PropertyType;
                var leftExpr = Expression.MakeMemberAccess(propAccess.Body, prop);
                var rightExpr = Expression.Convert(Expression.Constant(otherValue),propType);
                if (propType.IsPrimitive )
                {
                    equalExpr = Expression.Equal(leftExpr,rightExpr);
                }else
                {
                    equalExpr = Expression.MakeBinary(ExpressionType.Equal, leftExpr, rightExpr, false,
                        prop.PropertyType.GetMethod("op_Equality"));

                }
                if (condionalExpr == null)
                {
                    condionalExpr= equalExpr;
                }else
                {
                    condionalExpr= Expression.AndAlso(condionalExpr, equalExpr);
                }
            }
            if (condionalExpr == null)
            {
                throw new ArgumentException("There should be at least one property.");
            }
            return Expression.Lambda<Func<TItem, bool>>(condionalExpr, e1);
        }
    }
}
