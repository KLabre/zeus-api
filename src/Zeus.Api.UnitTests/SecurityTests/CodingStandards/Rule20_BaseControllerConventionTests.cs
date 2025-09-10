using Microsoft.AspNetCore.Mvc;

namespace Zeus.Api.UnitTests.SecurityTests.CodingStandards
{
    public class Rule20_BaseControllerConventionTests : SecurityConventionTestBase
    {
        /// <summary>
        /// Every controller that acts as a base class (except ControllerBase) must be abstract
        /// 
        /// RATIONALE: Base controllers are rarely instantiated on their own.
        /// </summary>
        [Fact]
        public void BaseControllers_ByConvetion_MustBeMarkedAsAbstract()
        {
            // ARRANGE
            var nonAbstractBaseControllers = Controllers.Where(c => c.BaseType != typeof(ControllerBase))
                .Select(c => c.BaseType)
                .Where(t => t is { IsAbstract: false}).ToList();

            // ASSERT
            Assert.NotNull(nonAbstractBaseControllers);
            var errorMessage = $"Every Controller that is derived from ControllerBase must be marked as an abstract class.\r\nYou must add 'public abstract class...' to the following controllers:\r\n\r\n{string.Join("\r\n", nonAbstractBaseControllers.Select(c => c?.FullName))}";
            Assert.True(nonAbstractBaseControllers.Count == 0, errorMessage);
        }

        /// <summary>
        /// Every non-abstract controller must inherit from BaseControler either directly or indirectly
        /// 
        /// RATIONALE: We wan common code to be executed for every action method.
        /// </summary>
        [Fact]
        public void AllNonAbstractControllers_ByConvention_MustDeriveFromApiBaseController()
        {
            // ARRANGE
            var controllersNotInheritedFromApiBaseController = Controllers
                .Except(AbstractControllers)
                .Where(IsNotInheritedFromApiControllerBase).ToList();

            // ASSERT
            Assert.NotNull(controllersNotInheritedFromApiBaseController);
            var errorMessage = $"Every Controller must inherit from ApiBaseController directly or indirectly \r\n The following controller must modified to inherit from ApiBaseController directly or indirectly:\r\n\r\n{string.Join("\r\n", controllersNotInheritedFromApiBaseController.Select(c => c.FullName))}";
            Assert.True(controllersNotInheritedFromApiBaseController.Count == 0, errorMessage);
        }
    }
}
