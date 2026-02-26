using DomainModle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Infrastructure
{
    public static class EFCoreExtensions
    {

        public static void EnableSoftDelte(this ModelBuilder modelBuilder)
        {
           

            var entityDelete = modelBuilder.Model.GetEntityTypes().Where(c =>
            c.ClrType.IsAssignableTo(typeof(ISoftDelete)));

            var methodToCall = typeof(EFCoreExtensions)
                .GetMethod(nameof(EFCoreExtensions.GetSoftDeleteFilter), 1,
                BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { }, null);

            foreach (var entityType in entityDelete)
            {
                var properties = entityType.GetProperties();
                var modelType = entityType.ClrType;
               // var baseType = entityType.BaseType;
               var scehma = entityType.GetSchema();
                var tableName = entityType.GetTableName();
                var method = methodToCall.MakeGenericMethod(modelType);
                var filter = method.Invoke(null, new object[] { });
                entityType.SetQueryFilter((LambdaExpression)filter);
            }
        }




        private static LambdaExpression GetSoftDeleteFilter<TEntity>()
            where TEntity : class, ISoftDelete
        {
            Expression<Func<TEntity, bool>> filters = x => !x.IsDelete;
            return filters;
        }
    }
}
