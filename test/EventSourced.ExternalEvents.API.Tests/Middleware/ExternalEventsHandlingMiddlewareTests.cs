﻿using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Exceptions;
using EventSourced.ExternalEvents.API.Configuration;
using EventSourced.ExternalEvents.API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace EventSourced.ExternalEvents.API.Tests.Middleware
{
    public class ExternalEventsHandlingMiddlewareTests
    {
        private readonly Mock<IExternalEventPublisher> _externalEventPublisherMock = new();

        [Fact]
        public async Task HandleAsync_WithoutCorrectPathString_CallsNext()
        {
            //Arrange
            var options = new EventSourcedExternalWebApiOptions("/EventSourced/ExternalEvent");
            using var webHost = await CreateWebHost(options);

            //Act
            var response = await webHost.GetTestClient()
                                        .GetAsync("/");

            //Arrange
            response.StatusCode.Should()
                    .Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_WithCorrectPathStringWithInvalidMethod_ReturnsMethodNotAllowed()
        {
            //Arrange
            var options = new EventSourcedExternalWebApiOptions("/EventSourced/ExternalEvent");
            using var webHost = await CreateWebHost(options);

            //Act
            var response = await webHost.GetTestClient()
                                        .GetAsync("/EventSourced/ExternalEvent");

            //Arrange
            response.StatusCode.Should()
                    .Be(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        public async Task HandleAsync_WithCorrectPathStringWithCorrectMethodWithInvalidBody_ReturnsUnprocessableEntity()
        {
            //Arrange
            var options = new EventSourcedExternalWebApiOptions("/EventSourced/ExternalEvent");
            using var webHost = await CreateWebHost(options);

            //Act
            var response = await webHost.GetTestClient()
                                        .PostAsJsonAsync<object?>("/EventSourced/ExternalEvent", null);

            //Arrange
            response.StatusCode.Should()
                    .Be(HttpStatusCode.UnprocessableEntity);
        }
        
        [Fact]
        public async Task HandleAsync_WithCorrectPathStringWithCorrectMethodWithValidBodyWithUnregisteredEventType_ReturnsNotFound()
        {
            //Arrange
            var options = new EventSourcedExternalWebApiOptions("/EventSourced/ExternalEvent");
            using var webHost = await CreateWebHost(options);
            _externalEventPublisherMock
                .Setup(m => m.PublishExternalEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ExternalEventNotRegisteredException("Mock exception"));
            //Act
            var response = await webHost.GetTestClient()
                                        .PostAsJsonAsync("/EventSourced/ExternalEvent", new PublishExternalEventRequest("TestEventType", "{}"));

            //Arrange
            response.StatusCode.Should()
                    .Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_WithEverythingValid_ReturnsOK()
        {
            //Arrange
            var options = new EventSourcedExternalWebApiOptions("/EventSourced/ExternalEvent");
            using var webHost = await CreateWebHost(options);
            
            //Act
            var response = await webHost.GetTestClient()
                                        .PostAsJsonAsync("/EventSourced/ExternalEvent", new PublishExternalEventRequest("TestEventType", "{}"));

            //Arrange
            response.StatusCode.Should()
                    .Be(HttpStatusCode.OK);
        }
        
        private Task<IHost> CreateWebHost(EventSourcedExternalWebApiOptions options)
        {
            return new HostBuilder().ConfigureWebHost(webBuilder =>
                                    {
                                        webBuilder.UseTestServer()
                                                  .ConfigureServices(services =>
                                                  {
                                                      services.AddEventSourcedExternalEventsWebApi(options);
                                                      services.AddSingleton(_externalEventPublisherMock.Object);
                                                  })
                                                  .Configure(app => app.UseEventSourcedExternalEventsWebApi());
                                    })
                                    .StartAsync();
        }
    }
}