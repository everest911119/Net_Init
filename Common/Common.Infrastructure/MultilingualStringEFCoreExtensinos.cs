using DomainModle;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Infrastructure
{
    public static class MultilingualStringEFCoreExtensinos
    {
        public static EntityTypeBuilder<TEntity> OwnsOneMultilingualString<TEntity>
            (this EntityTypeBuilder<TEntity> entityTypeBuilder, Expression<Func<TEntity,MultilingualString>> nagivationExpression,
            bool required=true, int maxLength =200) where TEntity : class
        {
            entityTypeBuilder.OwnsOne(nagivationExpression, dp =>
            {
                dp.Property(c => c.Chinese).IsRequired(required).HasMaxLength(maxLength).IsUnicode();
                dp.Property(c => c.English).IsRequired(required).HasMaxLength(maxLength).IsUnicode();
            });
            entityTypeBuilder.Navigation(nagivationExpression).IsRequired(required);
            return entityTypeBuilder;
        }
    }
}
