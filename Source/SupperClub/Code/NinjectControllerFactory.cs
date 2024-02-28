using System;
using System.Collections.Generic;
using System.Linq;

using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using Ninject.Modules;

using SupperClub.Domain;
using SupperClub.Data.EntityFramework;
using SupperClub.Domain.Repository;
using SupperClub.Data;
using SupperClub.Services;

namespace SupperClub.Web.Infrastructure
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        // A Ninject "kernel" is the thing that can supply object instances
        private IKernel kernel = new StandardKernel(new InjectServices());

        // ASP.NET MVC calls this to get the controller for each request
        protected override IController GetControllerInstance(RequestContext context, Type controllerType)
        {
            if (controllerType == null)
                return null;
            return (IController)kernel.Get(controllerType);
        }

        private class InjectServices : NinjectModule
        {
            public override void Load()
            {
                IKernel kernel = new StandardKernel();
                Bind<ISupperClubRepository>().To<SupperClubRepository>();
                DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));

            }
        }

    }
}
