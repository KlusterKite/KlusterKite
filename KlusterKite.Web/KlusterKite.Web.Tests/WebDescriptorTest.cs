﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebDescriptorTest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Tests the <seealso cref="WebDescriptorActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;

    using Autofac;

    using KlusterKite.Core;
    using KlusterKite.Core.TestKit;
    using KlusterKite.Web.Client;
    using KlusterKite.Web.Client.Messages;
    using KlusterKite.Web.Descriptor;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests the <seealso cref="WebDescriptorActor"/>
    /// </summary>
    public class WebDescriptorTest : BaseActorTest<WebDescriptorTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDescriptorTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The Xunit output.
        /// </param>
        public WebDescriptorTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test actor for correct <seealso cref="WebDescriptionResponse"/>
        /// </summary>
        [Fact]
        public void ResponseTest()
        {
            /*
            var testDescriptor =
                this.ActorOfAsTestActorRef<WebDescriptorActor>(this.Sys.DI().Props<WebDescriptorActor>());
            */
            this.Sys.StartNameSpaceActorsFromConfiguration();
            this.ExpectNoMsg();
            var response =
                this.Sys.GetWebDescriptor(Cluster.Get(this.Sys).SelfAddress)
                    .Ask<WebDescriptionResponse>(new WebDescriptionRequest(), TimeSpan.FromMilliseconds(500))
                    .Result;

            Assert.True(response.Services.Any(s => s.Route == "firstServiceRoot"));
            Assert.True(response.Services.Any(s => s.Route == "secondServiceRoot/secondServiceBranch"));
            Assert.True(response.Services.Any(s => s.Route == "thirdServiceRoot"));
            Assert.Equal(3, response.Services.Count);
            Assert.Equal(2, response.Services.Count(s => s.PublicHostName == "defaultHost"));
            Assert.Equal(1, response.Services.Count(s => s.PublicHostName == "otherHost"));
            Assert.Equal(3, response.Services.Count(s => s.ListeningPort == 8085));
        }

        /// <summary>
        /// The test configurator
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <inheritdoc />
            public override Config GetAkkaConfig(ContainerBuilder containerBuilder)
            {
                return ConfigurationFactory.ParseString(@"
                {
                    KlusterKite {
 		                Web {
 			                OwinBindAddress = ""http://*:8085""
                            Services {
                                First = {
                                    Port = 8085 //test
                                    PublicHostName = defaultHost
                                    Route = firstServiceRoot
                                }
                                Second = {
                                    Port = 8085
                                    PublicHostName = defaultHost
                                    Route = secondServiceRoot/secondServiceBranch
                                }
                                Third = {
                                    Port = 8085
                                    PublicHostName = otherHost
                                    Route = thirdServiceRoot
                                }
                            }
                        }
                    }
                }").WithFallback(base.GetAkkaConfig(containerBuilder));
            }

            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var installers = base.GetPluginInstallers();
                installers.Add(new Descriptor.Installer());
                return installers;
            }
        }
    }
}