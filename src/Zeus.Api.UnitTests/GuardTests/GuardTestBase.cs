using Ardalis.GuardClauses;
using System.Web.Mvc;
using Zeus.Api.Infrastructure;

namespace Zeus.Api.UnitTests.GuardTests
{
    public abstract class GuardTestBase
    {
        protected class ExcludedClass
        {
            public Type ? Type { get; set; }
            public string? Because { get; set; }
        }

        protected void AssertArgumentNullExceptionIsThrown(Action action, string parameterName)
        {
            ArgumentNullException? thrownException = default;

            try
            {
                action();
            }
            catch (ArgumentNullException ex)
            {
                thrownException = ex;
            }

            if (thrownException == null)
            {
                Assert.Fail($"Expected exception of type '{typeof(ArgumentNullException)}' for the parameter called '{parameterName} was not thrown.");
            }
        }

        protected virtual bool ClassIsNotAController(Type? type)
        {
            return !typeof(Controller).IsAssignableFrom(type);
        }

        protected virtual IEnumerable<Type?> ClassesWhoseParametersAreNotMockable => new[] { 
            Exclude(typeof(UserEnricher), because: "we cannot mock HttpContextAccessor"),
        }.Select(pair => pair.Type);

        protected virtual IEnumerable<Type?> ClassesWhoseContructorsContainNullableParameters => new[]
        {
            Exclude(typeof(Exception), because: "message and inner exceptions are nullable parameters."),
            Exclude(typeof(NotFoundException), because: "message and inner exceptions are nullable parameters."),

        }.Select(pair => pair.Type);

        protected virtual IEnumerable<Type?> ClassesWhoeConstructorsDirectlyInvokedTheBaseConstructor => new []
        {
            Exclude(typeof(RequiredIfAttribute), because: "the base constructor is not accessible"),
        }
        .Select(pair => pair.Type);


    protected ExcludedClass Exclude(Type type, string because)
        {
            return new ExcludedClass
            {
                Type = type,
                Because = because
            };
        }
    }
}
