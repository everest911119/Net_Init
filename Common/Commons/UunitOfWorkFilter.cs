using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Commons
{
    public class UunitOfWorkFilter : IAsyncActionFilter

    {
        private static UnitOfWorkAttribute? GetUoWAttr(ActionDescriptor actionDes )
        {
            
            var controlDes = actionDes as ControllerActionDescriptor;
            if ( controlDes == null) 
            {
                return null;
            }
            
            var uowAttr = controlDes.ControllerTypeInfo.GetCustomAttribute<UnitOfWorkAttribute>();
            if ( uowAttr != null )
            {
                return uowAttr;
            }else
            {
                return controlDes.MethodInfo.GetCustomAttribute<UnitOfWorkAttribute>();
            }
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionDes = context.ActionDescriptor;
            var uowAttr = GetUoWAttr(actionDes);

            if (uowAttr == null)
            {
                await next();
                return;
            }

            // scope
            using (TransactionScope txScope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead},
                TransactionScopeAsyncFlowOption.Enabled))
            {
                List<DbContext> dbCtxs = new List<DbContext>();
                foreach(var dbContextType in uowAttr.DbContextTypes)
                {
                    DbContext dbCtx = (DbContext)context.HttpContext.RequestServices.GetService(dbContextType);
                    dbCtxs.Add(dbCtx);   
                }
                var result = await next();
                if (result.Exception == null)
                {
                    foreach (var dbContext in dbCtxs)
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    txScope.Complete();
                }
               
            }

        }
    }
}
