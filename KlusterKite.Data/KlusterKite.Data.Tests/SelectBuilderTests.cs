// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectBuilderTests.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the <see cref="SelectBuilder" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using KlusterKite.API.Client;
    using KlusterKite.Data.EF;
    using KlusterKite.Data.Tests.Mock;

    using Microsoft.EntityFrameworkCore;

    using Xunit;

    /// <summary>
    /// Testing the <see cref="SelectBuilder"/>
    /// </summary>
    public class SelectBuilderTests
    {
        /// <summary>
        /// Testing the EF query with includes
        /// </summary>
        [Fact]
        public void EfQueryTest()
        {
            using (var context = this.CreateContext())
            {
                IQueryable<User> query = context.Users;
                var level2Fields = new List<ApiRequest>
                                       {
                                           new ApiRequest { FieldName = "name" },
                                           new ApiRequest
                                               {
                                                   FieldName = "users",
                                                   Fields = new[] { new ApiRequest { FieldName = "user" } }.ToList()
                                               }
                                       };

                var level1Fields = new List<ApiRequest>
                                       {
                                           new ApiRequest { FieldName = "login" },
                                           new ApiRequest
                                               {
                                                   FieldName = "roles",
                                                   Fields = new[] { new ApiRequest { FieldName = "role", Fields = level2Fields } }.ToList()
                                               }
                                       };
                
                var apiRequest = new ApiRequest
                {
                    Fields = level1Fields
                };

                query = query.SetIncludes(context, apiRequest);
                var user = query.FirstOrDefault();
                Assert.NotNull(user);
                Assert.NotNull(user.Roles);
                Assert.NotEmpty(user.Roles);
                var role = user.Roles.First().Role;
                Assert.NotNull(role);
                Assert.NotNull(role.Users);
                Assert.NotEmpty(role.Users);
                var subUser = role.Users.First().User;
                Assert.NotNull(subUser);
                Assert.NotNull(subUser.Roles);
                Assert.NotEmpty(subUser.Roles);
            }
        }

        /// <summary>
        /// Creates a test context
        /// </summary>
        /// <returns>The test context</returns>
        private TestDataContext CreateContext()
        {
            var name = Guid.NewGuid().ToString("N");
            var optionsBuilder = new DbContextOptionsBuilder<TestDataContext>();
            optionsBuilder.UseInMemoryDatabase(name);

            var context = new TestDataContext(optionsBuilder.Options);
            
            // context.Database.Delete();
            var users =
                Enumerable.Range(1, 100).Select(n => new User { Login = $"user{n:####}", Uid = Guid.NewGuid(), Roles = new List<RoleUser>() }).ToList();
            var roles =
                Enumerable.Range(1, 10).Select(n => new Role { Name = $"role{n:###}", Uid = Guid.NewGuid(), Users = new List<RoleUser>() }).ToList();

            for (var roleNum = 1; roleNum <= roles.Count; roleNum++)
            {
                var role = roles[roleNum - 1];
                for (var userNum = 1; userNum <= users.Count; userNum++)
                {
                    var user = users[userNum - 1];
                    if (userNum % roleNum == 0)
                    {
                        role.Users.Add(new RoleUser { User = user });
                    }

                    context.Users.Add(user);
                }

                context.Roles.Add(role);
            }

            context.SaveChanges();
            return new TestDataContext(optionsBuilder.Options);
        }
    }
}