using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NUnit.Dependencies
{
    /// <summary>
    /// An <see cref="DependantAttribute"/> <see cref="DependencyAttribute"/> which tests at least the happy path, and which result can be used from another test.
    /// </summary>
    /// <typeparam name="T">The result of the happy path of this action.</typeparam>
    [Dependant, Dependency]
    public abstract class DependencyFixture<T>
    {
        private T resultCache;

        /// <summary>
        /// Receives the result from the happy path of this action.
        /// </summary>
        public T Result
        {
            get
            {
                if (resultCache == null)
                {
                    try
                    {
                        resultCache = TestHappyPath();
                    }
                    catch (Exception e)
                    {
                        Assert.Ignore("A dependant action failed: " + e.ToString());
                    }
                }

                return resultCache;
            }
        }

        [Test]
        public void HappyPath()
        {
            TestHappyPath();
        }

        /// <summary>
        /// Tests the happy path for this action.
        /// </summary>
        /// <returns>The result of this happy path, which might be needed for another test to execute</returns>
        protected abstract T TestHappyPath();
    }

    [Dependant, Dependency]
    public abstract class AsyncDependencyFixture<T>
    {
        /*private Task<T> resultCache;

        public Task<T> Result
        {
            get
            {
                if (resultCache == null)
                {
                    resultCache = Task.Run(async () =>
                    {
                        try
                        {
                            return await TestHappyPath();
                        }
                        catch (Exception e)
                        {
                            throw new IgnoreException("A dependant action failed: " + e.ToString());
                        }
                    });
                }

                return resultCache;
            }
        }*/

        private T resultCache;

        /// <summary>
        /// synchronously receives the result from the happy path of the async action
        /// </summary>
        public T Result
        {
            get
            {
                if (resultCache == null)
                {
                    try
                    {
                        var task = TestHappyPath();
                        task.Wait();

                        if (task.Exception != null)
                        {
                            throw task.Exception;
                        }

                        resultCache = task.Result;
                    }
                    catch (Exception e)
                    {
                        Assert.Ignore("A dependant action failed: " + e.ToString());
                    }
                }

                return resultCache;
            }
        }

        /// <summary>
        /// DO NOT USE, your test will fail instead of being ignored if this action fails.
        /// </summary>
        [Test]
        public Task HappyPath()
        {
            return TestHappyPath();
        }

        /// <summary>
        /// Tests the happy path for this action.
        /// </summary>
        /// <returns>The result of this happy path, which might be needed for another test to execute</returns>
        protected abstract Task<T> TestHappyPath();
    }
}
