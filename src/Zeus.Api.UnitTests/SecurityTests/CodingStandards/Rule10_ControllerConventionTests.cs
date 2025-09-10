using System.Web.Mvc;

namespace Zeus.Api.UnitTests.SecurityTests.CodingStandards
{
    public class Rule10_ControllerConventionTests : SecurityConventionTestBase
    {

        [Fact]
        public void AllApiControllers_ByConvention_ShoulBeAuthorized()
        {
            // ARRANGE
            var controllers = Controllers
                .Except(AbstractControllers)
                    .Where(type => !type.IsDefined(typeof(AllowAnonymousAttribute), false)).ToList();

            var authorizedControllers = AuthorizedControllers.ToList();
            var nonAuthorizedControllers = controllers.Where(type => authorizedControllers.All(ac => type.FullName != ac.FullName));
            var errorMessage = $"All controllers in the API should be authorize. \r\nYou must add the '[Authorize]' attribute to the following controllers: \r\n\r\n{string.Join("\r\n", nonAuthorizedControllers.Select(c => c.FullName))}\r\n";


            // ASSERT
            Assert.True(controllers != null && authorizedControllers != null && controllers.Any() && authorizedControllers.Any(),
                "There must be at least one Controller with the '[Authorize]' attribute in this API project.'");
            Assert.True(controllers.Count == authorizedControllers.Count, errorMessage);
        }
    }
}
