using BitAddict.Aras.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#pragma warning disable 1591

namespace BitAddict.Aras.UnitTests
{
    [TestClass]
    public class TestCheckIdentityMembership : ArasUnitTestBase
    {
        [TestMethod]
        public void TestCurrentUserIsAdministrator()
        {
            // A developer should be an administrator to be able to develop

            var method = new CheckIdentityMembershipMethod();
            var bodyItem = Innovator.newItem("Method", "CheckIdentityMembership");
            bodyItem.setProperty("identity_name", "Administrators");

            var result = method.Apply(bodyItem);

            Assert.IsFalse(result.isError());
            Assert.AreEqual("true", result.getResult());
        }

        [TestMethod]
        public void TestCurrentUserIsNotSmurf()
        {
            var method = new CheckIdentityMembershipMethod();
            var bodyItem = Innovator.newItem("Method", "CheckIdentityMembership");
            bodyItem.setProperty("identity_name", "Smurfs");

            var result = method.Apply(bodyItem);

            Assert.IsFalse(result.isError());
            Assert.AreEqual("false", result.getResult());
        }

        [ClassInitialize]
        public new static void ClassInitialize(TestContext ctx)
        {
            ArasUnitTestBase.ClassInitialize(ctx);
        }

        [ClassCleanup]
        public new static void ClassCleanup()
        {
            ArasUnitTestBase.ClassCleanup();
        }
    }
}
