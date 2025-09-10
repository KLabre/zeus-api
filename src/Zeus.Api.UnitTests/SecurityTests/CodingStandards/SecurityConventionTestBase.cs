using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Zeus.Api.Controllers;

namespace Zeus.Api.UnitTests.SecurityTests.CodingStandards
{
    public abstract class SecurityConventionTestBase
    {

        protected IEnumerable<Type> Controllers;
        protected IEnumerable<Type> AuthorizedControllers;
        protected IEnumerable<Type> AbstractControllers => Controllers.Where(type => type.IsAbstract);
        
        protected SecurityConventionTestBase()
        {
            var candidateAssemblies = new[]
            {
                typeof(RootController).Assembly,
            };

            Controllers = GetAllControllers(fromAssemblies: candidateAssemblies);
            AuthorizedControllers = GetAuthorizedControllers(fromControllers: Controllers);
        }

        protected IEnumerable<Type> GetAllControllers(IEnumerable<Assembly> fromAssemblies)
        {
            var result = new List<Type>();
            foreach (var assembly in fromAssemblies)
                result.AddRange(GetAllControllers(fromAssembly: assembly));

            return result;
        }

        protected IEnumerable<Type> GetAllControllers(Assembly fromAssembly)
        {
            return fromAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(ControllerBase)));
        }

        protected IEnumerable<Type> GetAuthorizedControllers(IEnumerable<Type> fromControllers)
        {
            return fromControllers.Where(ControllerIsAuthorized);
        }

        /*
         * typeof(MyController).GetCutomAttributes() will return a list of the attributes from MyController;
         * then MyBaseController; and so on: the lower trhe index in the list, the higher the priority.
         * 
         * Implication: it is not sufficient to look for and attribute of Authorize or AllowAnonymous; we need to look at
         * which attribute comes first in the list to see which has priority
         * 
         * MyBaseController has a single action method with no explicit permission: it is an Authorized mehtod in
         * MyBaseController, but is Anonymous in MyController
         *
         */

        protected bool ControllerIsAuthorized(Type controllerType)
        {
            var attriubutes = controllerType.GetCustomAttributes();
            var enumerable = attriubutes.ToList();
            var firstAuthorizedAttribute =
                enumerable.FirstOrDefault(a => a.GetType().FullName == typeof(AuthorizeAttribute).FullName);

            if (firstAuthorizedAttribute == null) return false;

            var authorizedIndex = enumerable.IndexOf(firstAuthorizedAttribute);
            if(authorizedIndex == -1) return false;

            var firstAnonymousAttribbute = enumerable.FirstOrDefault(a => a is AllowAnonymousAttribute);
            var anonymousIndex = -1;
            if(firstAnonymousAttribbute != null) anonymousIndex = enumerable.IndexOf(firstAnonymousAttribbute);
            if (anonymousIndex == -1) return authorizedIndex >= 0;

            return authorizedIndex < anonymousIndex;
        }

        protected bool IsNotInheritedFromApiControllerBase(Type type)
        {
            // Change this to ApiControllerBase if we ever need to everride OnExecute
            return !typeof(ControllerBase).IsAssignableFrom(type);
        }
    }
}